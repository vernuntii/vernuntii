namespace Vernuntii.Collections
{
    /// <summary>
    /// A registry for items of type <typeparamref name="TItem"/>.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public interface INamedItemRegistry<TItem>
    {
        /// <summary>
        /// Registered names.
        /// </summary>
        IEnumerable<string> Names { get; }

        /// <summary>
        /// Registered items.
        /// </summary>
        IEnumerable<TItem> Items { get; }

        /// <summary>
        /// Adds an item associated to a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        void AddItem(string name, TItem item);

        /// <summary>
        /// Checks whether the name exists.
        /// </summary>
        /// <param name="name"></param>
        bool ContainsName(string name);

        /// <summary>
        /// Gets the item by name.
        /// </summary>
        /// <param name="name"></param>
        TItem GetItem(string name);

        /// <summary>
        /// Clears all message indicators.
        /// </summary>
        void ClearItems();
    }
}
