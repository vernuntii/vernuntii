using System.Text;
using Vernuntii.Coroutines;

namespace Vernuntii.Reactive.Extensions.Coroutines;

internal static partial class YieldersExtensions
{
    internal partial class Arguments
    {
        internal readonly static Key ObserveKey = new(Encoding.ASCII.GetBytes("vernuntii/rx"), 1);
        internal readonly static Key TakeKey = new(Encoding.ASCII.GetBytes("vernuntii/rx"), 2);
        internal readonly static Key EmitKey = new(Encoding.ASCII.GetBytes("vernuntii/rx"), 3);
    }
}
