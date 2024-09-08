using Vernuntii.Examples.Reactive.Coroutines;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await new PingPongExample().PingPongAsync();
    }
}
