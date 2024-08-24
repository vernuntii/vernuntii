using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;

namespace Vernuntii.Coroutines.Generators;

internal static class AsyncIterator
{
    internal static readonly Key s_asyncIteratorKey = new Key(Encoding.ASCII.GetBytes(nameof(AsyncIterator<VoidResult>)));
}

internal class AsyncIteratorContextService(ChannelWriter<AsyncIteratorResultObject> resultWriter, TaskCompletionSource<object> completionSourceReplacement)
{ 
    public ChannelWriter<AsyncIteratorResultObject> ResultWriter = resultWriter;
    public TaskCompletionSource<object> CompletionSourceReplacement = completionSourceReplacement;
}

internal class AsyncIterator<TResult> : IAsyncIterator, IAsyncIterator<TResult>
{
    readonly Coroutine<TResult> _coroutine;
    Coroutine<TResult> _coroutineAsResult;

    readonly bool _isCoroutineGeneric;

    public AsyncIterator(Coroutine coroutine)
    {
        ref var coroutineByRef = ref _coroutine;
        coroutineByRef = ref Unsafe.As<Coroutine, Coroutine<TResult>>(ref Unsafe.AsRef(in coroutine));
        _isCoroutineGeneric = false;
    }

    public AsyncIterator(Coroutine<TResult> coroutine)
    {
        ref var coroutineByRef = ref _coroutine;
        coroutineByRef = Unsafe.AsRef(in coroutine);
        _isCoroutineGeneric = true;
    }

    public async Coroutine<AsyncIteratorResultObject> Next()
    {
        //var a = Channel.CreateBounded<AsyncIteratorResultObject>(new BoundedChannelOptions(1));
        //var b = Channel.CreateBounded<object>(new BoundedChannelOptions(1));
        //var c = ValueTaskCompletionSource<object?>.RentFromCache();

        //var slim = new SemaphoreSlim(0, 1);
        //var coroutineContext = new CoroutineContext(new Dictionary<Key, object>() { { AsyncIterator.s_asyncIteratorKey, slim } }, keyedServicesToBequest: null);
        //_coroutineAsResult = await WithContext(default, static coroutine => Spawn(static coroutine => coroutine, coroutine), _coroutine).ConfigureAwait(false);
        //var waitTask = slim.WaitAsync();
        //var taskWinner = await Task.WhenAny(waitTask, _coroutineAsResult.AsTask()).ConfigureAwait(false);

        if (_coroutine.IsChildCoroutine) {

        }

        //_coroutine.

        return new AsyncIteratorResultObject();
    }
    public Coroutine<AsyncIteratorResultObject> Next<TNextResult>(TNextResult nextResult) => throw new NotImplementedException();
    public Coroutine Return() => throw new NotImplementedException();
    public Coroutine Return(Exception e) => throw new NotImplementedException();
    public Coroutine Return(TResult result) => throw new NotImplementedException();
}
