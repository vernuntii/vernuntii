using Vernuntii.Reactive.Emissions;

namespace Vernuntii.Reactive.Centralized;

public partial class WheneverThenResubscribeTests
{
    public static IEnumerable<object[]> Emitting_whenever_then_replay_succeeds_generator()
    {
        yield return new object[] {
            new (object, object)[] { (1, 1), (2, 2) },
            new (object, object)[] { (1, 1), (2, 1) }
        };
    }

    [Theory]
    [MemberData(nameof(Emitting_whenever_then_replay_succeeds_generator))]
    public async Task Emitting_whenever_then_replay_succeeds((object, object)[] inputValues, (object, object)[] expectedValues)
    {
        var eventStore = new EventStore();
        var whenevertDiscriminator = EventDiscriminator.New<object>();
        var resubscribeDiscriminator = EventDiscriminator.New<object>();
        var actualEventDatas = new List<(object, object)>();

        using var _ = eventStore.WheneverThenResubscribe(whenevertDiscriminator.Every(), resubscribeDiscriminator.Earliest()).Subscribe(actualEventDatas.Add);

        foreach (var input in inputValues) {
            await eventStore.EmitAsync(whenevertDiscriminator, input.Item1);
            await eventStore.EmitAsync(resubscribeDiscriminator, input.Item2);
        }

        Assert.Equal(expectedValues, actualEventDatas);
    }
}
