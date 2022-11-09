using Vernuntii.VersionIncrementing;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// The inbuilt presets.
    /// </summary>
    public enum InbuiltVersioningPreset
    {
        /// <summary>
        /// Continuous delivery preset consisting of
        /// <br/> - FalsyMessageIndicator for major and minor,
        /// <br/> - TruthyMessageIndicator for patch and
        /// <br/> - <see cref="VersionIncrementMode.Consecutive"/>
        /// </summary>
        ContinuousDelivery,
        /// <summary>
        /// Continuous deployment preset consisting of
        /// <br/> - FalsyMessageIndicator for major and minor,
        /// <br/> - TruthyMessageIndicator for patch and
        /// <br/> - <see cref="VersionIncrementMode.Successive"/>
        /// </summary>
        ContinuousDeployment,
        /// <summary>
        /// Conventional commits preset consisting of
        /// <br/> - ConventionalCommitsMessageIndicator for version core and
        /// <br/> - <see cref="VersionIncrementMode.Consecutive"/>
        /// </summary>
        ConventionalCommitsDelivery,
        /// <summary>
        /// Conventional commits preset consisting of
        /// <br/> - ConventionalCommitsMessageIndicator for version core and
        /// <br/> - <see cref="VersionIncrementMode.Successive"/>
        /// </summary>
        ConventionalCommitsDeployment,
        /// <summary>
        /// Manual preset consisting of
        /// <br/> - none message indicators and
        /// <br/> - <see cref="VersionIncrementMode.None"/>
        /// </summary>
        Manual,
        /// <summary>
        /// Default preset is <see cref="ContinuousDelivery"/>.
        /// </summary>
        Default
    }
}
