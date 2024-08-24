using System.Text;

namespace Vernuntii.Coroutines;

public sealed partial class Effect
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Effect extension point for easier third-party integration and access")]
    public static readonly Effect __co = new Effect();

    public partial class Arguments
    {
        private readonly static byte[] s_scope = Encoding.ASCII.GetBytes("@vernuntii");

        internal readonly static Key s_callArgumentType = new(s_scope, 1);
        internal readonly static Key s_launchArgumentType = new(s_scope, 2);
        internal readonly static Key s_spawnArgumentType = new(s_scope, 3);
        internal readonly static Key s_withContextArgumentType = new(s_scope, 4);
    }
}
