using FluentAssertions;
using Vernuntii.PluginSystem.Events;
using Vernuntii.Reactive;
using Xunit;

namespace Vernuntii.PluginSystem.Reactive;

public class ZipTests
{
    private static EventDiscriminator<int> s_number1 = new EventDiscriminator<int>();
    private static EventDiscriminator<int> s_number2 = new EventDiscriminator<int>();
    private static EventDiscriminator<int> s_number3 = new EventDiscriminator<int>();

    [Fact]
    public async Task One_event_zip_produces_tuple()
    {
        var pluginSystem = new EventSystem();
        var expectedNumbers = (1, 1);
        (int, int) actualNumbers = default;

        using var _ = pluginSystem
            .Every(s_number1)
            .Zip(s_number1)
            .Subscribe(eventData => actualNumbers = eventData);

        await pluginSystem.FullfillAsync(s_number1.EventId, 1);

        actualNumbers.Should().Be(expectedNumbers);
    }

    [Fact]
    public async Task Two_event_zip_produces_tuple()
    {
        var pluginSystem = new EventSystem();
        var expectedNumbers = (1, 2);
        (int, int) actualNumbers = default;

        using var _ = pluginSystem
             .Every(s_number1)
             .Zip(s_number2)
             .Subscribe(eventData => actualNumbers = eventData);

        await pluginSystem.FullfillAsync(s_number1.EventId, 1);
        actualNumbers.Should().Be((0, 0));

        await pluginSystem.FullfillAsync(s_number2.EventId, 2);
        actualNumbers.Should().Be((1, 2));
    }

    [Fact]
    public async Task Three_event_zip_produces_tuple()
    {
        var pluginSystem = new EventSystem();
        var expectedNumbers = ((1, 2), 3);
        ((int, int), int) actualNumbers = default;

        using var _ = pluginSystem
            .Every(s_number1)
            .Zip(s_number2)
            .Zip(s_number3)
            .Subscribe(eventData => actualNumbers = eventData);

        await pluginSystem.FullfillAsync(s_number1.EventId, 1);
        actualNumbers.Should().Be(((0, 0), 0));

        await pluginSystem.FullfillAsync(s_number2.EventId, 2);
        actualNumbers.Should().Be(((0, 0), 0));

        await pluginSystem.FullfillAsync(s_number3.EventId, 3);
        actualNumbers.Should().Be(((1, 2), 3));
    }
}
