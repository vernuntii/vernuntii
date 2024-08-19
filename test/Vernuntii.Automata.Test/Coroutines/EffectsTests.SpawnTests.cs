namespace Vernuntii.Coroutines;

partial class EffectsTests
{
    public class SpawnTests
    {
        [Fact]
        public async Task AsyncSpawn_FlowsCorretly()
        {
            const int expectedResult = 1;
            var awaitableResult = await Coroutine.Start(() => Spawn(async () => expectedResult)).ConfigureAwait(false);
            var result = await awaitableResult;
            Assert.Equal(expectedResult, result);
        }

        [UIFact]
        public async Task AsyncCallAwaitingAsyncSpawn_FlowsCorrectly()
        {
            int[] expectedRecords = [1];
            var records = new List<int>();

            await Coroutine.Start(() => Call(async () => {
                await Spawn(async () => {
                    await Task.Delay(CancellationTimeInMs).ConfigureAwait(false);
                    records.Add(2);
                }).ConfigureAwait(false);

                records.Add(1);
            })).ConfigureAwait(false);

            Assert.Equal(expectedRecords, records);
        }

        [Fact]
        public async Task AwaitingMultipleAsyncSpawn_FlowCorretly()
        {
            int[] expectedRecords = [1];
            var records = new List<int>();

            await Coroutine.Start(() => Call(() => {
                var spawn = Spawn(async () => {
                    await Task.Delay(CancellationTimeInMs).ConfigureAwait(false);
                    records.Add(2);
                });

                records.Add(1);
                return spawn;
            })).ConfigureAwait(false);

            Assert.Equal(expectedRecords, records);
        }
    }
}
