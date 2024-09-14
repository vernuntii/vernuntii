using System.Text;

namespace Vernuntii.Coroutines.Iterators;

public static partial class Yielders
{
    public static partial class Arguments
    {
        private readonly static byte[] s_scope = Encoding.ASCII.GetBytes("__co/itr");

        public readonly static Key ExchangeKey = new(s_scope, 1);
    }
}
