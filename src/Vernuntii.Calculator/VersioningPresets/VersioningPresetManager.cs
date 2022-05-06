using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.MessageVersioning;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A compendium for all kind of presets.
    /// </summary>
    public class VersioningPresetManager : IVersioningPresetManager
    {
        /// <summary>
        /// Creates a default preset compendium.
        /// </summary>
        public static VersioningPresetManager CreateDefault()
        {
            var presets = new VersioningPresetManager();

            // Adds presets including message conventions and height conventions.
            presets.Add(nameof(InbuiltVersioningPreset.Default), VersioningPreset.Default);
            presets.Add(nameof(InbuiltVersioningPreset.Manual), VersioningPreset.Manual);
            presets.Add(nameof(InbuiltVersioningPreset.ContinousDelivery), VersioningPreset.ContinousDelivery);
            presets.Add(nameof(InbuiltVersioningPreset.ContinousDeployment), VersioningPreset.ContinousDeployment);
            presets.Add(nameof(InbuiltVersioningPreset.ConventionalCommitsDelivery), VersioningPreset.ConventionalCommitsDelivery);
            presets.Add(nameof(InbuiltVersioningPreset.ConventionalCommitsDeployment), VersioningPreset.ConventionalCommitsDeployment);

            // Add message conventions.
            presets.AddMessageConvention(nameof(InbuiltMessageConvention.Continous), VersioningPreset.ContinousDelivery.MessageConvention);
            presets.AddMessageConvention(nameof(InbuiltMessageConvention.ConventionalCommits), VersioningPreset.ConventionalCommitsDelivery.MessageConvention);

            // Add message indicators.
            presets.AddMessageIndicator(nameof(InbuiltMessageIndicator.ConventionalCommits), ConventionalCommitsMessageIndicator.Default);
            presets.AddMessageIndicator(nameof(InbuiltMessageIndicator.Falsy), FalsyMessageIndicator.Default);
            presets.AddMessageIndicator(nameof(InbuiltMessageIndicator.Truthy), TruthyMessageIndicator.Default);

            return presets;
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IVersioningPreset> VersioningPresets =>
            _versioningPresets;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IMessageConvention?> MessageConventions =>
            _messageConventions;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IHeightConvention?> HeightConventions =>
            _heightConventions;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IMessageIndicator> MessageIndicators =>
            _messageIndicators;

        private Dictionary<string, IVersioningPreset> _versioningPresets = new Dictionary<string, IVersioningPreset>();
        private Dictionary<string, IMessageConvention?> _messageConventions = new Dictionary<string, IMessageConvention?>();
        private Dictionary<string, IHeightConvention?> _heightConventions = new Dictionary<string, IHeightConvention?>();
        private Dictionary<string, IMessageIndicator> _messageIndicators = new Dictionary<string, IMessageIndicator>();

        private void EnsureNotExistingPreset(string name)
        {
            if (_versioningPresets.ContainsKey(name)) {
                throw new ArgumentException($"A preset with the name \"{name}\" already exists");
            }
        }

        /// <summary>
        /// Adds a preset with name and what it actually includes.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="preset"></param>
        /// <param name="includes"></param>
        public void Add(string name, IVersioningPreset preset, VersioningPresetIncludes includes = VersioningPresetIncludes.Preset)
        {
            EnsureNotExistingPreset(name);

            if (includes.HasFlag(VersioningPresetIncludes.Preset)) {
                _versioningPresets.Add(name, preset);
            }

            if (includes.HasFlag(VersioningPresetIncludes.MessageConvention)) {
                _messageConventions.Add(name, preset.MessageConvention);
            }

            if (includes.HasFlag(VersioningPresetIncludes.HeightConvention)) {
                _heightConventions.Add(name, preset.HeightConvention);
            }
        }

        /// <inheritdoc/>
        public void AddPreset(string name, IVersioningPreset preset)
        {
            EnsureNotExistingPreset(name);
            _versioningPresets.Add(name, preset);
        }

        /// <inheritdoc/>
        public void AddMessageConvention(string name, IMessageConvention? messageConvention)
        {
            EnsureNotExistingPreset(name);
            _messageConventions.Add(name, messageConvention);
        }

        /// <inheritdoc/>
        public void AddHeightConvention(string name, IHeightConvention? heightConvention)
        {
            EnsureNotExistingPreset(name);
            _heightConventions.Add(name, heightConvention);
        }

        /// <inheritdoc/>
        public void AddMessageIndicator(string name, IMessageIndicator messageIndicator) =>
            _messageIndicators.Add(name, messageIndicator);

        /// <inheritdoc/>
        public void ClearVersioningPresets() => _versioningPresets.Clear();

        /// <inheritdoc/>
        public void ClearMessageConventions() => _messageConventions.Clear();

        /// <inheritdoc/>
        public void ClearHeightConventions() => _heightConventions.Clear();

        /// <inheritdoc/>
        public void ClearMessageIndicators() => _messageIndicators.Clear();

        /// <inheritdoc/>
        public void Clear()
        {
            ClearVersioningPresets();
            ClearMessageConventions();
            ClearHeightConventions();
        }
    }
}
