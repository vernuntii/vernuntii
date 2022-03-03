using Vernuntii.MessageVersioning.MessageIndicators;

namespace Vernuntii.MessageVersioning
{
    /// <summary>
    /// The inbuilt presets.
    /// </summary>
    public enum VersioningModePreset
    {
        /// <summary>
        /// Continous delivery preset consisting of
        /// <br/> - <see cref="FalsyMessageIndicator"/> for major and minor,
        /// <br/> - <see cref="TruthyMessageIndicator"/> for patch and
        /// <br/> - <see cref="VersionIncrementMode.Consecutive"/>
        /// </summary>
        ContinousDelivery,
        /// <summary>
        /// Conventional commits preset consisting of
        /// <br/> - <see cref="ConventionalCommitsMessageIndicator"/> for version core and
        /// <br/> - <see cref="VersionIncrementMode.Consecutive"/>
        /// </summary>
        ConventionalCommitsDelivery,
        /// <summary>
        /// Conventional commits preset consisting of
        /// <br/> - <see cref="ConventionalCommitsMessageIndicator"/> for version core and
        /// <br/> - <see cref="VersionIncrementMode.Successive"/>
        /// </summary>
        ConventionalCommitsDeployment,
        /// <summary>
        /// Continous deployment preset consisting of
        /// <br/> - <see cref="FalsyMessageIndicator"/> for major and minor,
        /// <br/> - <see cref="TruthyMessageIndicator"/> for patch and
        /// <br/> - <see cref="VersionIncrementMode.Successive"/>
        /// </summary>
        ContinousDeployment,
        /// <summary>
        /// Manual preset consisting of
        /// <br/> - none message indicators and
        /// <br/> - <see cref="VersionIncrementMode.None"/>
        /// </summary>
        Manual
    }
}
