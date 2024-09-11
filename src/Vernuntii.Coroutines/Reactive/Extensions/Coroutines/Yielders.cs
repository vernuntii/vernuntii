using System.Text;
using Vernuntii.Coroutines;

namespace Vernuntii.Reactive.Extensions.Coroutines;

public static partial class Yielders
{
    public partial class Arguments
    {
        internal readonly static Key ChannelKey = new(Encoding.ASCII.GetBytes("__co/rx"), 1);
        internal readonly static Key TakeKey = new(Encoding.ASCII.GetBytes("__co/rx"), 2);
        internal readonly static Key EmitKey = new(Encoding.ASCII.GetBytes("__co/rx"), 3);
    }
}
