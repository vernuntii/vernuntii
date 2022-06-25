using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;
using static Nuke.Common.Tooling.NuGetPackageResolver;

namespace Vernuntii.Console.GlobalTool
{
    internal class MSBuildIntegrationLocateCommandPlugin : Plugin
    {
        private const string VernuntiiConsoleMSBuild = $"{nameof(Vernuntii)}.{nameof(Console)}.MSBuild";

        public Command Command => _command;

        private Command _command;
        private ILoggingPlugin _loggingPlugin = null!;
        private ILogger _logger = null!;
        private NuGetTasks nugetTasks = null!;

        private Argument<MSBuildFileLocation> locateArgument = new Argument<MSBuildFileLocation>("locate") {
            Description = "The file location you are asking for."
        };

        private Option<string?> packageNameOption = new Option<string?>(new[] { "--package-name" }, () => VernuntiiConsoleMSBuild) {
            Description = $"A variant of {VernuntiiConsoleMSBuild} to use."
        };

        private Option<string?> packageVersionOption = new Option<string?>(new[] { "--package-version" }) {
            Description = $"The pacakge version of {VernuntiiConsoleMSBuild} or variant to use."
        };

        private Option<bool> downloadPackageOption = new Option<bool>(new[] { "--download-package" }) {
            Description = $"Downloads {VernuntiiConsoleMSBuild} or variant in case it is not."
        };

        private Option<string?> packageSourceOption = new Option<string?>(new[] { "--package-source" }) {
            Description = $"Sets the NuGet-specific packageSource for {VernuntiiConsoleMSBuild} or variant." +
                $"Either url e.g. https://api.nuget.org/v3/index.json (default) or a directory containing packages." +
                $"Multiple sources are allowed when separating values by a semicolon."
        };

        public MSBuildIntegrationLocateCommandPlugin()
        {
            _command = new Command("locate") {
                locateArgument,
                packageNameOption,
                packageVersionOption,
                downloadPackageOption,
                packageSourceOption
            };

            _command.Handler = CommandHandler.Create(OnInvokeCommand);
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
            var buildDirectory = packageDirectory / "build";
            var depsDirectory = packageDirectory / "deps";
            var msbuildTaskDirectory = depsDirectory / "msbuild" / "netstandard2.0";

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

            InstalledPackage GetGlobalPackage()
            {
                try {
                    return nugetTasks.GetGlobalPackage(
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

        protected override void OnAfterRegistration()
        {
            Plugins.First<MSBuildIntegrationCommandPlugin>().Command.AddCommand(_command);

            _loggingPlugin = Plugins.First<ILoggingPlugin>();
            _logger = _loggingPlugin.CreateLogger<MSBuildIntegrationLocateCommandPlugin>();

            var nugetActionLogger = Plugins.First<ILoggingPlugin>().CreateLogger<NuGetTasks>();
            nugetTasks = new NuGetTasks(nugetActionLogger);
        }
    }
}
