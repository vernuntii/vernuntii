using System.Text;

namespace Vernuntii.Coroutines;

public sealed partial class Yielders
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Effect extension point for easier third-party integration and access")]
    public static readonly Yielders __co = new Yielders();

    public partial class Arguments
    {
        private readonly static byte[] s_scope = Encoding.ASCII.GetBytes("@vernuntii");

        internal readonly static Key s_callArgumentType = new(s_scope, 1);
        internal readonly static Key s_launchArgumentType = new(s_scope, 2);
        internal readonly static Key s_spawnArgumentType = new(s_scope, 3);
        internal readonly static Key s_withContextArgumentType = new(s_scope, 4);
        internal readonly static Key s_returnArgumentType = new(s_scope, 5);
        internal readonly static Key s_throwArgumentType = new(s_scope, 6);
        internal readonly static Key s_yieldArgumentType = new(s_scope, 7);
    }
}
