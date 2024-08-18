namespace Vernuntii.Coroutines;

partial class EffectsTests
{
    /* FYI: Given `Coroutine.Start(() => Call(..))`, then the Calls's coroutine of Start's provider waits for coroutine of Call's provider. */
    public class LaunchTests
    {
        [UIFact]
        public async Task AsyncCallAwaitingAsyncLaunch_FlowsCorrectly()
        {
            int[] expectedRecords = [1, 2];
            var records = new List<int>();

            await Coroutine.Start(() => Call(async () => {
                await Launch(async () => {
                    await Task.Yield();
                    records.Add(2);
                }).ConfigureAwait(false);

                records.Add(1);
            })).ConfigureAwait(false);

            Assert.Equal(expectedRecords, records);
        }

        [Fact]
        public async Task AwaitingMultipleAsyncLaunch_FlowCorretly()
        {
            int[] expectedRecords = [1, 2];
            var records = new List<int>();

            await Coroutine.Start(() => Call(() => {
                var launch = Launch(async () => {
                    records.Add(2);
                });

                records.Add(1);
                return launch;
            })).ConfigureAwait(false);

            Assert.Equal(expectedRecords, records);
        }
    }
}
