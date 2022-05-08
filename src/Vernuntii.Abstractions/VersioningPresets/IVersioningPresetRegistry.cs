using Vernuntii.Collections;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A registry for items of type <see cref="IVersioningPreset"/>.
    /// </summary>
    public interface IVersioningPresetRegistry : INamedItemRegistry<IVersioningPreset>
    {
    }
}
