namespace Vernuntii.Coroutines;

public class KeyEqualityComparer : EqualityComparer<Key>
{
    public new static readonly KeyEqualityComparer Default = new();

    public override bool Equals(Key x, Key y) => x.SequenceEqual(y);
    public override int GetHashCode([DisallowNull] Key obj) => obj.GetHashCode();
}
