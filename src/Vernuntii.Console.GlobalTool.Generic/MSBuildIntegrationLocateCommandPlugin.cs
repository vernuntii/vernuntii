using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Logging;
using Vernuntii.Console.GlobalTool.DotNet;
using Vernuntii.Console.GlobalTool.NuGet;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;

namespace Vernuntii.Console.GlobalTool
{
    internal class MSBuildIntegrationLocateCommandPlugin : Plugin
    {
        private const string VernuntiiConsoleMSBuild = $"{nameof(Vernuntii)}.{nameof(Console)}.MSBuild";

        public Command Command => _command;

        private readonly MSBuildIntegrationCommandPlugin _msbuildIntegrationCommandPlugin;
        private readonly ILoggingPlugin _loggingPlugin;
        private readonly ILogger<MSBuildIntegrationLocateCommandPlugin> _logger;
        private readonly Command _command;
        private NuGetRunner _nugetRunner = null!;

        private readonly Argument<MSBuildFileLocation> locateArgument = new("locate") {
            Description = "The file location you are asking for."
        };

        private readonly Option<string?> packageNameOption = new(new[] { "--package-name" }, () => VernuntiiConsoleMSBuild) {
            Description = $"A variant of {VernuntiiConsoleMSBuild} to use."
        };

        private readonly Option<string?> packageVersionOption = new(new[] { "--package-version" }) {
            Description = $"The pacakge version of {VernuntiiConsoleMSBuild} or variant to use."
        };

        private readonly Option<bool> downloadPackageOption = new(new[] { "--download-package" }) {
            Description = $"Downloads {VernuntiiConsoleMSBuild} or variant in case it is not."
        };

        private readonly Option<string?> packageSourceOption = new(new[] { "--package-source" }) {
            Description = $"Sets the NuGet-specific packageSource for {VernuntiiConsoleMSBuild} or variant." +
                $"Either url e.g. https://api.nuget.org/v3/index.json (default) or a directory containing packages." +
                $"Multiple sources are allowed when separating values by a semicolon."
        };

        public MSBuildIntegrationLocateCommandPlugin(MSBuildIntegrationCommandPlugin msbuildIntegrationCommandPlugin, ILoggingPlugin loggingPlugin)
        {
            _command = new Command("locate") {
                locateArgument,
                packageNameOption,
                packageVersionOption,
                downloadPackageOption,
                packageSourceOption
            };

            _command.Handler = CommandHandler.Create(OnInvokeCommand);
            _msbuildIntegrationCommandPlugin = msbuildIntegrationCommandPlugin;
            _loggingPlugin = loggingPlugin;
            _logger = _loggingPlugin.CreateLogger<MSBuildIntegrationLocateCommandPlugin>();
        }

        // Parameter names are bound to naming convention, do not change!
        private void OnInvokeCommand(
            MSBuildFileLocation locate,
            string packageName,
            string? packageVersion,
            bool downloadPackage,
            string? packageSource)
        {
            packageName ??= VernuntiiConsoleMSBuild;
            packageVersion ??= FindVersion(packageVersion);
            var package = GetGlobalPackage();

            var packageDirectory = package.Directory;
            var buildDirectory = Path.Combine(packageDirectory, "build");
            var depsDirectory = Path.Combine(packageDirectory, "deps");
            var msbuildTaskDirectory = Path.Combine(depsDirectory, "msbuild", "netstandard2.0");

            var filePath = locate switch {
                MSBuildFileLocation.PackageDirectory => packageDirectory,
                MSBuildFileLocation.AutoImportTargets => Path.Combine(buildDirectory, $"{packageName}.targets"),
                MSBuildFileLocation.AutoImportProps => Path.Combine(buildDirectory, $"{packageName}.props"),
                MSBuildFileLocation.MSBuildTaskDirectory => msbuildTaskDirectory,
                MSBuildFileLocation.MSBuildTaskDll => Path.Combine(msbuildTaskDirectory, $"{packageName}.dll"),
                _ => throw new ArgumentException("The locate argument you specified is invalid")
            };

            System.Console.Write(filePath);

            string FindVersion(string? version)
            {
                if (version == null) {
                    var pkgverFile = Path.Combine(AppContext.BaseDirectory, $"{packageName}.pkgver");

                    if (File.Exists(pkgverFile)) {
                        version = File.ReadAllText(pkgverFile).Trim();
                    } else {
                        _logger.LogTrace("MSBuild Integration package version file (\"{PackageVersionFile}\") not found", pkgverFile);
                    }
                }

                if (version is null) {
                    throw new VersionNotSpecifiedException($"A package version was not specified for package \"{packageName}\"");
                }

                return version;
            }

            DownloadedPackage GetGlobalPackage()
            {
                _nugetRunner ??= new NuGetRunner(_loggingPlugin.CreateLogger<NuGetRunner>());

                try {
                    return _nugetRunner.GetGlobalPackage(
                        packageName,
                        packageVersion,
                        downloadPackage: downloadPackage,
                        packageSource: packageSource,
                        verbosity: _loggingPlugin.Verbosity)
                        ?? throw new PackageNotInstalledException($"Package {packageName}@{packageVersion} is not installed");
                } catch (PackageNotInstalledException) {
                    if (_logger.IsEnabled(LogLevel.Debug)) {
                        var packageSources = packageSource?.Split(";");

                        if (packageSources != null) {
                            foreach (var singlePackageSource in packageSources) {
                                if (Directory.Exists(singlePackageSource)) {
                                    _logger.LogDebug(@"Recognized Package source ""{PackageSource}"" as directory: (directory content)
{DirectoryContent}", singlePackageSource, string.Join(Environment.NewLine, Directory.EnumerateFileSystemEntries(singlePackageSource, "*")));
                                }
                            }
                        }
                    }

                    throw;
                }
            }
        }

        protected override void OnExecution() =>
            _msbuildIntegrationCommandPlugin.Command.AddCommand(_command);
    }
}
