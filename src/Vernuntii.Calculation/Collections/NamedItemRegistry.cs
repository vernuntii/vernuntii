namespace Vernuntii.Collections
{
    /// <summary>
    /// A registry for items of type <typeparamref name="TItem"/>.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    internal class NamedItemRegistry<TItem> : INamedItemRegistry<TItem>
    {
        /// <inheritdoc/>
        public IEnumerable<string> Names => NamedItems.Keys;

        /// <inheritdoc/>
        public IEnumerable<TItem> Items => NamedItems.Values;

        internal Dictionary<string, TItem> NamedItems = new(StringComparer.InvariantCultureIgnoreCase);

        /// <inheritdoc/>
        public void AddItem(string name, TItem item) =>
            NamedItems.Add(name, item);

        /// <inheritdoc/>
        public bool ContainsName(string name) =>
            NamedItems.ContainsKey(name);

        /// <inheritdoc/>
        public TItem GetItem(string name)
        {
            if (!NamedItems.TryGetValue(name, out var value)) {
                throw new ItemMissingException($"Item was missing: \"{value}\"");
            }

            return value;
        }

        /// <inheritdoc/>
        public void ClearItems() => NamedItems.Clear();
    }
}
