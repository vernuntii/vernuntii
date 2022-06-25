using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nuke.Common.Tools.DotNet;
using Vernuntii.Console.GlobalTool.Extensions;
using Vernuntii.Diagnostics;
using Vernuntii.Logging;
using static Nuke.Common.Tooling.NuGetPackageResolver;

namespace Vernuntii.Console.GlobalTool
{
    internal class NuGetTasks
    {
        private const Verbosity DefaultVerbosity = Verbosity.Error;

        private static InstalledPackage? GetGlobalPackage(string packageName, string packageVersion) =>
            GetGlobalInstalledPackage(packageName, packageVersion, packagesConfigFile: null);

        ILogger<NuGetTasks> _logger;

        public NuGetTasks(ILogger<NuGetTasks> logger) =>
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private const string RestoreProjectTemplate = """
            <Project Sdk="Microsoft.NET.Sdk">

                <PropertyGroup>
                    <TargetFramework>net6.0</TargetFramework>
                    <RestoreSources>{{PackageSources}}</RestoreSources>
                </PropertyGroup>
  
                <ItemGroup>
                    <PackageReference Include="{{PackageName}}" Version="{{PackageVersion}}" />
                </ItemGroup>
  
            </Project>
            """;

        public void DownloadPackage(
            string packageName,
            string packageVersion,
            string? packageSource = null,
            Verbosity verbosity = DefaultVerbosity)
        {
            var restoreProjectFile = Path.GetTempFileName();

            try {
                packageSource ??= "https://api.nuget.org/v3/index.json";

                var restoreProjectContent = RestoreProjectTemplate
                    .Replace("{{PackageName}}", packageName, StringComparison.InvariantCulture)
                    .Replace("{{PackageVersion}}", packageVersion, StringComparison.InvariantCulture)
                    .Replace("{{PackageSources}}", packageSource, StringComparison.InvariantCulture);

                _logger.LogTrace(@"Written restore file ({RestoreProjectFile}):
{RestoreProjectContent}", restoreProjectFile, restoreProjectContent);

                File.WriteAllText(restoreProjectFile, restoreProjectContent);

#pragma warning disable CA2254 // Template should be a static expression
                SimpleProcess.StartThenWaitForExit(
                    new SimpleProcessStartInfo(
                        "dotnet",
                        args: $"restore {restoreProjectFile} --verbosity {verbosity.ToDotNetVerbosity()}").LogDebug(_logger),
                    outputReceived: message => _logger.LogDebug(message),
                    errorReceived: message => _logger.LogError(message));
#pragma warning restore CA2254 // Template should be a static expression
            } finally {
                File.Delete(restoreProjectFile);
            }
        }

        public InstalledPackage? GetGlobalPackage(
            string packageName,
            string packageVersion,
            bool downloadPackage = false,
            string? packageSource = null,
            Verbosity verbosity = DefaultVerbosity)
        {
            var package = GetGlobalPackage(packageName, packageVersion);

            if (package is null && downloadPackage) {
                DownloadPackage(
                    packageName,
                    packageVersion,
                    packageSource: packageSource,
                    verbosity: verbosity);

                package = GetGlobalPackage(packageName, packageVersion);
            }

            return package;
        }
    }
}
