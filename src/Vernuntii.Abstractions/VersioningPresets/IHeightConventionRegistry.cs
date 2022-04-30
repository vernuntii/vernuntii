using Vernuntii.HeightConventions;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Registry for height conventions.
    /// </summary>
    public interface IHeightConventionRegistry
    {
        /// <summary>
        /// Data source of height conventions.
        /// </summary>
        IReadOnlyDictionary<string, IHeightConvention?> HeightConventions { get; }

        /// <summary>
        /// Adds a height convention associated to a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="heightConvention"></param>
        void AddHeightConvention(string name, IHeightConvention? heightConvention);

        /// <summary>
        /// Clears all height conventions.
        /// </summary>
        void ClearHeightConventions();
    }
}
