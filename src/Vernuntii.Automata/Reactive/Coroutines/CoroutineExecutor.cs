using Vernuntii.Reactive.Coroutines.AsyncEffects;

namespace Vernuntii.Reactive.Coroutines;

public delegate IAsyncEnumerable<IEffect> CoroutineDefinition();
public delegate Coroutine CoroutineFactory();

internal class CoroutineExecutor : ICoroutineExecutor
{
    private readonly IReadOnlyDictionary<EffectHandlerId, IEffectStore> _steps;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;
    private List<CoroutineLifetime> _coroutineLifetimes;

    internal CoroutineExecutor(IReadOnlyDictionary<EffectHandlerId, IEffectStore> steps)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        _coroutineLifetimes = new List<CoroutineLifetime>();
        _steps = steps;
    }

    public void Start(CoroutineDefinition coroutineDefinition, CancellationToken cancellationToken = default)
    {
        var scopedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, cancellationToken);
        var scopedCancellationToken = scopedCancellationTokenSource.Token;

        var coroutine = Task.Run(async () => {
            await foreach (var step in coroutineDefinition().WithCancellation(scopedCancellationToken)) {
                if (!_steps.TryGetValue(step.HandlerId, out var stepStore)) {
                    throw new KeyNotFoundException();
                }

                await stepStore.HandleAsync(step);
            }
        });

        var coroutineLifetime = new CoroutineLifetime() {
            CancellationTokenSource = scopedCancellationTokenSource,
            Coroutines = new List<Task>() { coroutine }
        };

        _coroutineLifetimes.Add(coroutineLifetime);
    }

    public void Start(CoroutineFactory coroutineFactory, CancellationToken cancellationToken = default)
    {
        var scopedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, cancellationToken);
        var scopedCancellationToken = scopedCancellationTokenSource.Token;

        var coroutine = Task.Run(async () => {

            CoroutineMethodBuilder.CoroutineSite site;

            while (true) {
                if (Interlocked.CompareExchange(ref CoroutineMethodBuilder.s_locker, 1, 0) == 0) {
                    CoroutineMethodBuilder.s_site = site = new();
                    _ = coroutineFactory();
                    break;
                }
            }

            await foreach (var stepCompletionHandler in site.IncomingStepChannel.Reader.ReadAllAsync(cancellationToken)) {
                var step = stepCompletionHandler.Effect;
                if (!_steps.TryGetValue(step.HandlerId, out var stepStore)) {
                    throw new KeyNotFoundException();
                }

                await stepStore.HandleAsync(step).ConfigureAwait(false);
                stepCompletionHandler.CompleteEffect();
            }
        }, cancellationToken);

        var coroutineLifetime = new CoroutineLifetime() {
            CancellationTokenSource = scopedCancellationTokenSource,
            Coroutines = new List<Task>() { coroutine }
        };

        _coroutineLifetimes.Add(coroutineLifetime);
    }

    public Task WhenAll() => Task.WhenAll(_coroutineLifetimes.SelectMany(x => x.Coroutines));

    private sealed record CoroutineLifetime : IDisposable
    {
        public required CancellationTokenSource CancellationTokenSource { get; init; }
        public required List<Task> Coroutines { get; init; }

        private bool _isDisposed;

        private void Cancel()
        {
            if (CancellationTokenSource.IsCancellationRequested) {
                CancellationTokenSource.Cancel();
            }
        }

        public void Dispose()
        {
            if (_isDisposed) {
                return;
            }

            Cancel();
            CancellationTokenSource.Dispose();
            _isDisposed = true;
        }
    }
}
