using Microsoft.Extensions.Configuration;
using Vernuntii.MessageVersioning;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCaseArguments"/>.
    /// </summary>
    public static class CalculatorBranchCaseArgumentsExtensions
    {
        /// <summary>
        /// The "VersioningMode"-key.
        /// </summary>
        public const string VersioningModeKey = "VersioningMode";

        #region VersioningModeExtension

        /// <summary>
        /// Binds configuration to new or existing instance of type <see cref="MessageVersioningModeExtensionOptions"/>.
        /// </summary>
        /// <param name="branchCaseArguments"></param>
        /// <param name="configuration"></param>
        public static IBranchCaseArguments TryCreateVersioningModeExtension(this IBranchCaseArguments branchCaseArguments, IConfiguration? configuration = null)
        {
            if (configuration is null) {
                configuration = branchCaseArguments.GetConfigurationExtension();
            }

            var versioningModeSection = configuration.GetSection(VersioningModeKey);
            Func<MessageVersioningModeExtensionOptions> extensionFactory;

            if (!versioningModeSection.Exists() || versioningModeSection.Value != null) {
                extensionFactory = CreateFromString;

                MessageVersioningModeExtensionOptions CreateFromString() => new MessageVersioningModeExtensionOptions {
                    VersionTransformerOptions = VersionTransformerBuilderOptions.GetPreset(configuration.GetValue(VersioningModeKey, default(string)))
                };
            } else {
                extensionFactory = CreateFromObject;

                MessageVersioningModeExtensionOptions CreateFromObject()
                {
                    var versioningModeObject = new VersioningModeObject();
                    versioningModeSection.Bind(versioningModeObject);

                    if (versioningModeObject.MessageConvention == null && versioningModeObject.Preset == null) {
                        throw new ArgumentException($"Field \"{nameof(versioningModeObject.MessageConvention)}\" is null." +
                            $" Either set it or specifiy the field \"{nameof(versioningModeObject.Preset)}\".");
                    } else if (versioningModeObject.IncrementMode == null && versioningModeObject.Preset == null) {
                        throw new ArgumentException($"Field \"{nameof(versioningModeObject.IncrementMode)}\" is null." +
                            $" Either set it or specifiy the field \"{nameof(versioningModeObject.Preset)}\".");
                    }

                    MessageVersioningModeExtensionOptions options;

                    if (versioningModeObject.Preset == null) {
                        options = new MessageVersioningModeExtensionOptions();
                    } else {
                        options = MessageVersioningModeExtensionOptions.WithPreset(versioningModeObject.Preset);
                    }

                    if (versioningModeObject.MessageConvention != null) {
                        options = options with {
                            VersionTransformerOptions = options.VersionTransformerOptions.WithConvention(versioningModeObject.MessageConvention)
                        };
                    }

                    if (versioningModeObject.IncrementMode != null) {
                        options = options with {
                            VersionTransformerOptions = options.VersionTransformerOptions with { IncrementMode = versioningModeObject.IncrementMode.Value }
                        };
                    }

                    return options;
                }
            }

            _ = branchCaseArguments.GetExtensionOrCreate(MessageVersioningModeExtensionOptions.ExtensionName, extensionFactory);
            return branchCaseArguments;
        }

        /// <summary>
        /// Gets the instance of <see cref="MessageVersioningModeExtensionOptions"/> from <see cref="IBranchCaseArguments.Extensions"/>.
        /// </summary>
        /// <param name="branchCaseArguments"></param>
        public static MessageVersioningModeExtensionOptions GetVersioningModeExtension(this IBranchCaseArguments branchCaseArguments) =>
            branchCaseArguments.GetExtension<MessageVersioningModeExtensionOptions>(MessageVersioningModeExtensionOptions.ExtensionName);

        #endregion

        internal class VersioningModeObject
        {
            public string? Preset { get; set; }
            public string? MessageConvention { get; set; }
            public VersionIncrementMode? IncrementMode { get; set; }
        }
    }
}
