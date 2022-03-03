namespace Vernuntii.SemVer.Parser.Extensions
{
    internal static class RangeExtensions
    {
        public static Range WithStart(this Range range, Index start)
        {
            return new Range(start, range.End);
        }

        public static Range WithEnd(this Range range, Index end)
        {
            return new Range(range.Start, end);
        }
    }
}
