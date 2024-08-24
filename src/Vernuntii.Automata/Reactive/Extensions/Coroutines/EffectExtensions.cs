using System.Text;
using Vernuntii.Coroutines;

namespace Vernuntii.Reactive.Extensions.Coroutines;

internal static partial class EffectExtensions
{
    internal partial class Arguments
    {
        internal readonly static Key ObserveArgumentType = new Key(Encoding.ASCII.GetBytes("@vernuntii/rx"), 1);
    }
}
