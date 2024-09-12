using System.Text;

namespace Vernuntii.Coroutines;

public sealed partial class Yielders
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Yielers extension point for easier third-party integration and access")]
    public static readonly Yielders __co = new();

    public partial class Arguments
    {
        private readonly static byte[] s_scope = Encoding.ASCII.GetBytes("__co");

        public readonly static Key CallKey = new(s_scope, 1);
        public readonly static Key LaunchKey = new(s_scope, 2);
        public readonly static Key SpawnKey = new(s_scope, 3);
        public readonly static Key WithContextKey = new(s_scope, 4);
        public readonly static Key YieldReturnKey = new(s_scope, 5);
        public readonly static Key ThrowKey = new(s_scope, 6);
        public readonly static Key YieldKey = new(s_scope, 7);
    }
}
