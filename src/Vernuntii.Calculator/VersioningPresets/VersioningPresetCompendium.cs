using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageVersioning;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A compendium for all kind of presets.
    /// </summary>
    public class VersioningPresetCompendium : IVersioningPresetCompendium
    {
        /// <summary>
        /// Creates a default preset compendium.
        /// </summary>
        public static VersioningPresetCompendium CreateDefault()
        {
            var presets = new VersioningPresetCompendium();

            // Adds presets including message conventions and height conventions.
            presets.Add(nameof(VersioningPresetKind.Default), VersioningPreset.Default);
            presets.Add(nameof(VersioningPresetKind.Manual), VersioningPreset.Manual);
            presets.Add(nameof(VersioningPresetKind.ContinousDelivery), VersioningPreset.ContinousDelivery);
            presets.Add(nameof(VersioningPresetKind.ContinousDeployment), VersioningPreset.ContinousDeployment);
            presets.Add(nameof(VersioningPresetKind.ConventionalCommitsDelivery), VersioningPreset.ConventionalCommitsDelivery);
            presets.Add(nameof(VersioningPresetKind.ConventionalCommitsDeployment), VersioningPreset.ConventionalCommitsDeployment);

            // Add message conventions.
            presets.AddMessageConvention(nameof(MessageConventionKind.Continous), VersioningPreset.ContinousDelivery.MessageConvention);
            presets.AddMessageConvention(nameof(MessageConventionKind.ConventionalCommits), VersioningPreset.ConventionalCommitsDelivery.MessageConvention);

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

        private Dictionary<string, IVersioningPreset> _versioningPresets = new Dictionary<string, IVersioningPreset>();
        private Dictionary<string, IMessageConvention?> _messageConventions = new Dictionary<string, IMessageConvention?>();
        private Dictionary<string, IHeightConvention?> _heightConventions = new Dictionary<string, IHeightConvention?>();

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
        public void ClearVersioningPresets() => _versioningPresets.Clear();

        /// <inheritdoc/>
        public void ClearMessageConventions() => _messageConventions.Clear();

        /// <inheritdoc/>
        public void ClearHeightConventions() => _heightConventions.Clear();

        /// <inheritdoc/>
        public void Clear()
        {
            ClearVersioningPresets();
            ClearMessageConventions();
            ClearHeightConventions();
        }
    }
}
