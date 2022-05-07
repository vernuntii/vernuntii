using Vernuntii.HeightConventions;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Registry for height conventions.
    /// </summary>
    public interface IHeightConventionRegistry
    {
        /// <summary>
        /// Height convention identifiers.
        /// </summary>
        IEnumerable<string?> HeightConventionIdentifiers { get; }

        /// <summary>
        /// Adds a height convention associated to a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="heightConvention"></param>
        void AddHeightConvention(string name, IHeightConvention? heightConvention);

        /// <summary>
        /// Gets the height convention by name.
        /// </summary>
        /// <param name="name"></param>
        IHeightConvention? GetHeightConvention(string name);

        /// <summary>
        /// Clears all height conventions.
        /// </summary>
        void ClearHeightConventions();
    }
}
