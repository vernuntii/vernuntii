namespace Vernuntii.Coroutines;

public class KeyEqualityComparer : EqualityComparer<Key>
{
    public static bool Equals(in Key x, in Key y) => x.SequenceEqual(in y);
    public static uint GetHashCode([DisallowNull] in Key obj) => obj._hash;

    public new static readonly KeyEqualityComparer Default = new();

    public override bool Equals(Key x, Key y) => x.SequenceEqual(y);
    public override int GetHashCode([DisallowNull] Key obj) => obj.GetHashCode();
}
