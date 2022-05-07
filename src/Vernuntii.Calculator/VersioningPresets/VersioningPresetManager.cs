using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageConventions.MessageIndicators;

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
        public IEnumerable<string> VersioningPresetIdentifiers =>
            _versioningPresets.Keys;

        /// <inheritdoc/>
        public IEnumerable<string> MessageConventionIdentifiers =>
            _messageConventions.Keys;

        /// <inheritdoc/>
        public IEnumerable<string> HeightConventionIdentifiers =>
            _heightConventions.Keys;

        /// <inheritdoc/>
        public IEnumerable<string> MessageIndicatorIdentifiers =>
            _messageIndicators.Keys;

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
        public void AddVersioningPreset(string name, IVersioningPreset preset)
        {
            EnsureNotExistingPreset(name);
            _versioningPresets.Add(name, preset);
        }

        /// <inheritdoc/>
        public IVersioningPreset GetVersioningPreset(string name)
        {
            if (!_versioningPresets.TryGetValue(name, out var value)) {
                throw new VersioningPresetMissingException($"Versioning preset was missing: \"{value}\"");
            }

            return value;
        }

        /// <inheritdoc/>
        public void AddMessageConvention(string name, IMessageConvention? messageConvention)
        {
            EnsureNotExistingPreset(name);
            _messageConventions.Add(name, messageConvention);
        }

        /// <inheritdoc/>
        public IMessageConvention? GetMessageConvention(string name)
        {
            if (!_messageConventions.TryGetValue(name, out var value)) {
                throw new MessageConventionMissingException($"Message convention was missing: \"{value}\"");
            }

            return value;
        }

        /// <inheritdoc/>
        public void AddHeightConvention(string name, IHeightConvention? heightConvention)
        {
            EnsureNotExistingPreset(name);
            _heightConventions.Add(name, heightConvention);
        }

        /// <inheritdoc/>
        public IHeightConvention? GetHeightConvention(string name)
        {
            if (!_heightConventions.TryGetValue(name, out var value)) {
                throw new MessageConventionMissingException($"Height convention was missing: \"{value}\"");
            }

            return value;
        }

        /// <inheritdoc/>
        public void AddMessageIndicator(string name, IMessageIndicator messageIndicator) =>
            _messageIndicators.Add(name, messageIndicator);

        /// <inheritdoc/>
        public IMessageIndicator GetMessageIndicator(string name)
        {
            if (!_messageIndicators.TryGetValue(name, out var value)) {
                throw new MessageConventionMissingException($"Message indicator was missing: \"{value}\"");
            }

            return value;
        }

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
