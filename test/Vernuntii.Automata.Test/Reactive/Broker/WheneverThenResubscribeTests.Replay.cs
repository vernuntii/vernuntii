using Vernuntii.Reactive.Events;

namespace Vernuntii.Reactive.Broker;

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
        var eventBroker = new EventBroker();
        var whenevertDiscriminator = EventDiscriminator.New<object>();
        var resubscribeDiscriminator = EventDiscriminator.New<object>();
        var actualEventDatas = new List<(object, object)>();

        using var _ = eventBroker.WheneverThenResubscribe(whenevertDiscriminator.Every(), resubscribeDiscriminator.Earliest()).Subscribe(actualEventDatas.Add);

        foreach (var input in inputValues) {
            await eventBroker.EmitAsync(whenevertDiscriminator, input.Item1);
            await eventBroker.EmitAsync(resubscribeDiscriminator, input.Item2);
        }

        Assert.Equal(expectedValues, actualEventDatas);
    }
}
