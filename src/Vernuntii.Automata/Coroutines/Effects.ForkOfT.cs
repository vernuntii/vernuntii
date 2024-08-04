namespace Vernuntii.Coroutines;

partial class Effects
{
    public async static Coroutine<Coroutine<T>> ForkAsync<T>(Func<Coroutine<T>> provider)
    {
        var completionSource = new CoroutineCompletionSource<Coroutine<T>>();
        var forkedCoroutine = await new Coroutine<Coroutine<T>>(completionSource.CreateValueTask(), ArgumentReceiverDelegate);
        return forkedCoroutine;

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new ForkArgument<T>(provider, completionSource);
            argumentReceiver.ReceiveArgument(ref argument, ArgumentType.Default);
        }
    }

    internal struct ForkArgument<T>(Func<Coroutine<T>> provider, CoroutineCompletionSource<Coroutine<T>> completionSource)
    {
        private readonly Func<Coroutine<T>> _provider = provider;

        public void CreateCoroutine(ref ForkCoroutineAwaiterReceiver awaiterReceiver)
        {
            var coroutine = _provider();
            var awaiter = coroutine.GetAwaiter();
            awaiterReceiver.ReceiveCoroutineAwaiter(ref awaiter);
            completionSource.SetResult(coroutine);
        }
    }
}
