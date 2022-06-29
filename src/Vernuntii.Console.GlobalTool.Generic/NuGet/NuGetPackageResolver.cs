using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;

namespace Vernuntii.Console.GlobalTool.NuGet
{
    internal static class NuGetPackageResolver
    {
        public static string GetGlobalPackagesDirectory()
        {
            var settings = Settings.LoadDefaultSettings(
                root: Directory.GetCurrentDirectory(),
                configFileName: null,
                machineWideSettings: null);

            return SettingsUtility.GetGlobalPackagesFolder(settings);
        }

        public static DownloadedPackage? GetGlobalDownloadedPackage(string packageName, string packageVersion)
        {
            var packagesDirectory = GetGlobalPackagesDirectory();

            if (string.IsNullOrWhiteSpace(packagesDirectory)) {
                throw new ArgumentException("Global packages directory is null or empty");
            }

            var packageNameDirectory = Directory.GetDirectories(packagesDirectory, packageName, SearchOption.TopDirectoryOnly).SingleOrDefault();

            if (packageNameDirectory != null) {
                var packageNameVersionDirectory = Directory.GetDirectories(packageNameDirectory, packageVersion, SearchOption.TopDirectoryOnly).SingleOrDefault();

                if (packageNameVersionDirectory != null) {
                    var correctCasedPackageVersion = Path.GetDirectoryName(packagesDirectory)!;
                    return new DownloadedPackage(packageNameVersionDirectory, correctCasedPackageVersion);
                }
            }

            return null;
        }
    }
}
