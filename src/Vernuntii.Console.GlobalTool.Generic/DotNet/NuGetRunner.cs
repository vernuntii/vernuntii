using System.Text;
using Kenet.SimpleProcess;
using Microsoft.Extensions.Logging;
using Vernuntii.Console.GlobalTool.Extensions;
using Vernuntii.Console.GlobalTool.NuGet;
using Vernuntii.Logging;

namespace Vernuntii.Console.GlobalTool.DotNet
{
    internal class NuGetRunner
    {
        private const Verbosity DefaultVerbosity = Verbosity.Error;

        private static DownloadedPackage? GetGlobalPackage(string packageName, string packageVersion) =>
            NuGetPackageResolver.GetGlobalDownloadedPackage(packageName, packageVersion);

        private readonly ILogger<NuGetRunner> _logger;

        public NuGetRunner(ILogger<NuGetRunner> logger) =>
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

                ProcessExecutorBuilder.CreateDefault(
                        SimpleProcessStartInfo.NewBuilder("dotnet")
                            .PasteArguments("restore", restoreProjectFile, "--verbosity", verbosity.ToDotNetVerbosity())
                            .Build()
                            .LogDebug(_logger))
                    .AddOutputWriter(bytes => _logger.LogDebug(Encoding.UTF8.GetString(bytes)))
                    .AddErrorWriter(bytes => _logger.LogError(Encoding.UTF8.GetString(bytes)))
                    .RunToCompletion();
            } finally {
                File.Delete(restoreProjectFile);
            }
        }

        public DownloadedPackage? GetGlobalPackage(
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
