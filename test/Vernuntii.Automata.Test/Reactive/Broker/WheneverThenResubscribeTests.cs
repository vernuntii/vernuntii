using FluentAssertions;
using Vernuntii.Reactive.Events;

namespace Vernuntii.Reactive.Broker;

public partial class WheneverThenResubscribeTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Emitting_whenever_then_resubscribe_succeeds(int wheneverThenResubscribeEmitCounter)
    {
        var eventBroker = new EventBroker();
        var whenevertDiscriminator = EventDiscriminator.New();
        var resubscribeDiscriminator = EventDiscriminator.New();
        (object?, object?) result = ("<inavlid>", "<inavlid>");
        var emissionCounter = 0;

        using var _ = eventBroker.WheneverThenResubscribe(whenevertDiscriminator.Every(), resubscribeDiscriminator.One()).Subscribe(eventData => {
            result = eventData;
            emissionCounter++;
        });

        var currentWheneverThenResubscribeEmitCounter = wheneverThenResubscribeEmitCounter;
        while (currentWheneverThenResubscribeEmitCounter-- > 0) {
            await eventBroker.EmitAsync(whenevertDiscriminator);
            await eventBroker.EmitAsync(resubscribeDiscriminator);
        }

        result.Should().BeEquivalentTo((default(object), default(object)));
        emissionCounter.Should().Be(wheneverThenResubscribeEmitCounter);
    }

    [Fact]
    public async Task Emitting_resubscribe_then_whenever_fails()
    {
        var eventBroker = new EventBroker();
        var whenevertDiscriminator = EventDiscriminator.New();
        var resubscribeDiscriminator = EventDiscriminator.New();
        (object?, object?) invalid = ("<inavlid>", "<inavlid>");
        using var _ = eventBroker.WheneverThenResubscribe(whenevertDiscriminator.One(), resubscribeDiscriminator.One()).Subscribe(eventData => invalid = eventData);
        await eventBroker.EmitAsync(resubscribeDiscriminator);
        await eventBroker.EmitAsync(whenevertDiscriminator);
        invalid.Should().BeEquivalentTo(invalid);
    }

    [Fact]
    public async Task Emitting_whenever_then_two_resubscribe_leads_to_two_emissions()
    {
        var eventBroker = new EventBroker();
        var whenevertDiscriminator = EventDiscriminator.New();
        var resubscribeDiscriminator = EventDiscriminator.New();
        var emissionCounter = 0;
        using var _ = eventBroker.WheneverThenResubscribe(whenevertDiscriminator.One(), resubscribeDiscriminator.Every()).Subscribe(_ => emissionCounter++);
        await eventBroker.EmitAsync(whenevertDiscriminator);
        await eventBroker.EmitAsync(resubscribeDiscriminator);
        await eventBroker.EmitAsync(resubscribeDiscriminator);
        emissionCounter.Should().Be(2);
    }
}
