namespace Vernuntii.Coroutines;

partial class CoroutineTests
{
    public class FactoryTests
    {
        [Fact]
        public async Task Start_StartsChildCoroutine()
        {
            var task = Task.Run(async () => await Coroutine.Start(async () => { }).ConfigureAwait(false));
            var taskWinner = await Task.WhenAny(task, Task.Delay(CancellationTimeInMs)).ConfigureAwait(false);
            Assert.Equal(task, taskWinner);
        }

        [Fact]
        public async Task Start_StartsGenericChildCoroutine()
        {
            var task = Task.Run(async () => await Coroutine.Start<int>(async () => default).ConfigureAwait(false));
            var taskWinner = await Task.WhenAny(task, Task.Delay(CancellationTimeInMs)).ConfigureAwait(false);
            Assert.Equal(task, taskWinner);
        }
    }
}
