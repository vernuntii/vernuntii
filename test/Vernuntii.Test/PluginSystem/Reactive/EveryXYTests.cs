using FluentAssertions;
using Vernuntii.Reactive;

namespace Vernuntii.PluginSystem.Reactive;

public class EveryXYTests
{
    private static EventDiscriminator<int> s_number1 = new EventDiscriminator<int>();
    private static EventDiscriminator<int> s_number2 = new EventDiscriminator<int>();
    private static EventDiscriminator<int> s_number3 = new EventDiscriminator<int>();

    [Fact]
    public async Task Two_and_operands_produces_tuple()
    {
        var pluginSystem = new EventSystem();
        var expectedNumbers = (1, 1);
        (int, int) actualNumbers = default;

        using var _ = pluginSystem
            .Every(s_number1)
            .And(s_number1)
            .Subscribe(eventData => actualNumbers = eventData);

        await pluginSystem.EmitAsync(s_number1.EventId, 1);

        actualNumbers.Should().Be(expectedNumbers);
    }

    [Fact]
    public async Task Three_and_operands_produces_tuple()
    {
        var pluginSystem = new EventSystem();
        var expectedNumbers = (1, 2);
        (int, int) actualNumbers = default;

        using var _ = pluginSystem
             .Every(s_number1)
             .And(s_number2)
             .Subscribe(eventData => actualNumbers = eventData);

        await pluginSystem.EmitAsync(s_number1.EventId, 1);
        actualNumbers.Should().Be((0, 0));

        await pluginSystem.EmitAsync(s_number2.EventId, 2);
        actualNumbers.Should().Be(expectedNumbers);
    }

    [Fact]
    public async Task Four_and_operands_produces_tuple()
    {
        var pluginSystem = new EventSystem();
        var expectedNumbers = ((1, 2), 3);
        ((int, int), int) actualNumbers = default;

        using var _ = pluginSystem
            .Every(s_number1)
            .And(s_number2)
            .And(s_number3)
            .Subscribe(eventData => actualNumbers = eventData);

        await pluginSystem.EmitAsync(s_number1.EventId, 1);
        actualNumbers.Should().Be(((0, 0), 0));

        await pluginSystem.EmitAsync(s_number2.EventId, 2);
        actualNumbers.Should().Be(((0, 0), 0));

        await pluginSystem.EmitAsync(s_number3.EventId, 3);
        actualNumbers.Should().Be(expectedNumbers);
    }
}
