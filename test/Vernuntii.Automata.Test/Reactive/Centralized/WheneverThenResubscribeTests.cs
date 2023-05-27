using FluentAssertions;
using Vernuntii.Reactive.Emissions;

namespace Vernuntii.Reactive.Centralized;

public partial class WheneverThenResubscribeTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Emitting_whenever_then_resubscribe_succeeds(int wheneverThenResubscribeEmitCounter)
    {
        var eventStore = new EventStore();
        var whenevertDiscriminator = EventDiscriminator.New();
        var resubscribeDiscriminator = EventDiscriminator.New();
        (object?, object?) result = ("<inavlid>", "<inavlid>");
        var emissionCounter = 0;

        using var _ = eventStore.WheneverThenResubscribe(whenevertDiscriminator.Every(), resubscribeDiscriminator.One()).Subscribe(eventData => {
            result = eventData;
            emissionCounter++;
        });

        var currentWheneverThenResubscribeEmitCounter = wheneverThenResubscribeEmitCounter;
        while (currentWheneverThenResubscribeEmitCounter-- > 0) {
            await eventStore.EmitAsync(whenevertDiscriminator);
            await eventStore.EmitAsync(resubscribeDiscriminator);
        }

        result.Should().BeEquivalentTo((default(object), default(object)));
        emissionCounter.Should().Be(wheneverThenResubscribeEmitCounter);
    }

    [Fact]
    public async Task Emitting_resubscribe_then_whenever_fails()
    {
        var eventStore = new EventStore();
        var whenevertDiscriminator = EventDiscriminator.New();
        var resubscribeDiscriminator = EventDiscriminator.New();
        (object?, object?) invalid = ("<inavlid>", "<inavlid>");
        using var _ = eventStore.WheneverThenResubscribe(whenevertDiscriminator.One(), resubscribeDiscriminator.One()).Subscribe(eventData => invalid = eventData);
        await eventStore.EmitAsync(resubscribeDiscriminator);
        await eventStore.EmitAsync(whenevertDiscriminator);
        invalid.Should().BeEquivalentTo(invalid);
    }

    [Fact]
    public async Task Emitting_whenever_then_two_resubscribe_leads_to_two_emissions()
    {
        var eventStore = new EventStore();
        var whenevertDiscriminator = EventDiscriminator.New();
        var resubscribeDiscriminator = EventDiscriminator.New();
        var emissionCounter = 0;
        using var _ = eventStore.WheneverThenResubscribe(whenevertDiscriminator.One(), resubscribeDiscriminator.Every()).Subscribe(_ => emissionCounter++);
        await eventStore.EmitAsync(whenevertDiscriminator);
        await eventStore.EmitAsync(resubscribeDiscriminator);
        await eventStore.EmitAsync(resubscribeDiscriminator);
        emissionCounter.Should().Be(2);
    }
}
