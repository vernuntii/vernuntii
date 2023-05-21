namespace Vernuntii.IO
{
    public interface IAnyPath : IEquatable<IAnyPath>
    {
        string PathString { get; }
    }
}
