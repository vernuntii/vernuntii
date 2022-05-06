namespace Vernuntii.Collections
{
    internal class KeyValuePairEqualityComparer<TKey, TValue> : EqualityComparer<KeyValuePair<TKey, TValue>>
        where TKey : notnull
    {
        public new static KeyValuePairEqualityComparer<TKey, TValue> Default = new KeyValuePairEqualityComparer<TKey, TValue>(EqualityComparer<TKey>.Default);

        public static KeyValuePairEqualityComparer<TKey, TValue> Create(IEqualityComparer<TKey> keyComparer) =>
            new KeyValuePairEqualityComparer<TKey, TValue>(keyComparer);

        private readonly IEqualityComparer<TKey> _keyComparer;

        public KeyValuePairEqualityComparer(IEqualityComparer<TKey> keyComparer) =>
            _keyComparer = keyComparer ?? throw new ArgumentNullException(nameof(keyComparer));

        public override bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) =>
            _keyComparer.Equals(x.Key, y.Key);

        public override int GetHashCode(KeyValuePair<TKey, TValue> obj) =>
            _keyComparer.GetHashCode(obj.Key);
    }
}
