namespace Vernuntii.Coroutines;

public sealed partial class Effect
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Effect extension point for easier third-party access")]
    public static readonly Effect __co = new Effect();

    public partial class Arguments
    {
    }
}
