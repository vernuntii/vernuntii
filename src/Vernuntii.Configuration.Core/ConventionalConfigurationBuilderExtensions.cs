using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="IConventionalConfigurationBuilder"/>.
    /// </summary>
    public static class ConventionalConfigurationBuilderExtensions
    {
        private static HashSet<string> EliminateDuplicates(IEnumerable<string> fileNames) =>
            new HashSet<string>(fileNames);

        /// <summary>
        /// Tries to add the first existing file with the help of current <see cref="IConventionalConfigurationBuilder.ConventionalFileFinders"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="directoryPath"></param>
        /// <param name="fileNames"></param>
        /// <param name="addedFilePath"></param>
        /// <param name="configureProviderBuilder"></param>
        private static bool TryAddFirstConventionalFileCore(
            this IConventionalConfigurationBuilder builder,
            string directoryPath,
            HashSet<string> fileNames,
            [NotNullWhen(true)] out string? addedFilePath,
            Action<IFileShadowedConfigurationProviderBuilderConfigurer>? configureProviderBuilder = null)
        {
            if (builder.ConventionalFileFinders.Count == 0) {
                throw new InvalidOperationException("No conventional file finder has been added");
            }

            if (fileNames.Count == 0) {
                throw new ArgumentException("At least one file have to be specified", nameof(fileNames));
            }

            var candidates = fileNames
                .SelectMany(fileName => builder.ConventionalFileFinders
                    .Where(x => x.IsProbeable(fileName))
                    .Select(x => new {
                        FileFinder = x,
                        FileFindingEnumerator = x.FindFile(directoryPath, fileName)
                    }))
                .ToList();

            nextCandidatesHavingNext:
            var candidatesHavingNextEnumerator = candidates.Where(x => x.FileFindingEnumerator.MoveNext()).GetEnumerator();

            if (candidatesHavingNextEnumerator.MoveNext()) {
                do {
                    var candidate = candidatesHavingNextEnumerator.Current;

                    if (candidate.FileFindingEnumerator.Current != null) {
                        var filePath = addedFilePath = candidate.FileFindingEnumerator.GetCurrentFilePath();

                        candidate.FileFinder.AddFile(
                            builder,
                            filePath,
                            configureProviderBuilder is null
                                ? null
                                : configurator => configureProviderBuilder(
                                    new FileShadowedConfigurationProviderBuilderConfigurator(
                                        new FileInfo(filePath),
                                        configurator)));

                        // We found file, so we exit.
                        return true;
                    }
                } while (candidatesHavingNextEnumerator.MoveNext());

                // We previously had at least had one
                // successful MoveNext(), so we try again.
                goto nextCandidatesHavingNext;
            }

            addedFilePath = null;
            return false;
        }

        /// <summary>
        /// Tries to add the first existing file with the help of current <see cref="IConventionalConfigurationBuilder.ConventionalFileFinders"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="directoryPath"></param>
        /// <param name="fileNames"></param>
        /// <param name="addedFilePath"></param>
        /// <param name="configureProviderBuilder"></param>
        public static bool TryAddFirstConventionalFile(
            this IConventionalConfigurationBuilder builder,
            string directoryPath,
            IEnumerable<string> fileNames,
            [NotNullWhen(true)] out string? addedFilePath,
            Action<IFileShadowedConfigurationProviderBuilderConfigurer>? configureProviderBuilder = null) =>
            builder.TryAddFirstConventionalFileCore(directoryPath, EliminateDuplicates(fileNames), out addedFilePath, configureProviderBuilder);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowFileNotFound(string directoryPath, IEnumerable<string> fileNames) =>
            throw new FileNotFoundException($"Not one of the following files has been found in {directoryPath} or above: {string.Join(", ", fileNames)}");

        /// <summary>
        /// Adds first existing file with the help of current <see cref="IConventionalConfigurationBuilder.ConventionalFileFinders"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="directoryPath"></param>
        /// <param name="fileNames"></param>
        /// <param name="addedFilePath"></param>
        /// <param name="configureProviderBuilder"></param>
        public static IConventionalConfigurationBuilder AddFirstConventionalFile(
            this IConventionalConfigurationBuilder builder,
            string directoryPath,
            IEnumerable<string> fileNames,
            out string addedFilePath,
            Action<IFileShadowedConfigurationProviderBuilderConfigurer>? configureProviderBuilder = null)
        {
            var uniqueFileNames = EliminateDuplicates(fileNames);

            if (!builder.TryAddFirstConventionalFileCore(
                directoryPath,
                uniqueFileNames,
                out var nullableAddedFilePath,
                configureProviderBuilder)) {
                ThrowFileNotFound(directoryPath, uniqueFileNames);
            }

            addedFilePath = nullableAddedFilePath;
            return builder;
        }

        /// <summary>
        /// Tries to first existing file with the help of current <see cref="IConventionalConfigurationBuilder.ConventionalFileFinders"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="fileOrDirectory"></param>
        /// <param name="alternativeFileNames"></param>
        /// <param name="addedFilePath"></param>
        /// <param name="configureProviderBuilder"></param>
        private static bool TryAddFileOrFirstConventionalFileCore(
            this IConventionalConfigurationBuilder builder,
            ref string fileOrDirectory,
            ref IEnumerable<string> alternativeFileNames,
            [NotNullWhen(true)] out string? addedFilePath,
            Action<IFileShadowedConfigurationProviderBuilderConfigurer>? configureProviderBuilder = null)
        {
            string directory;
            IEnumerable<string> fileNames;

            if (Directory.Exists(fileOrDirectory)) {
                directory = fileOrDirectory;
                fileNames = alternativeFileNames;
            } else {
                directory = Path.GetDirectoryName(fileOrDirectory) ?? throw new ArgumentException("File path didn't include directory path", nameof(fileOrDirectory));

                if (directory == string.Empty) {
                    directory = Directory.GetCurrentDirectory();
                }

                var fileName = Path.GetFileName(fileOrDirectory);
                fileNames = fileName == null ? alternativeFileNames : new[] { fileName };
            }

            fileOrDirectory = directory;
            var uniqueFileNames = EliminateDuplicates(fileNames);
            alternativeFileNames = uniqueFileNames;

            return builder.TryAddFirstConventionalFileCore(
                directory,
                uniqueFileNames,
                out addedFilePath,
                configureProviderBuilder);
        }

        /// <summary>
        /// Tries to first existing file with the help of current <see cref="IConventionalConfigurationBuilder.ConventionalFileFinders"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="fileOrDirectory"></param>
        /// <param name="alternativeFileNames"></param>
        /// <param name="addedFilePath"></param>
        /// <param name="configureProviderBuilder"></param>
        public static bool TryAddFileOrFirstConventionalFile(
            this IConventionalConfigurationBuilder builder,
            string fileOrDirectory,
            IEnumerable<string> alternativeFileNames,
            [NotNullWhen(true)] out string? addedFilePath,
            Action<IFileShadowedConfigurationProviderBuilderConfigurer>? configureProviderBuilder = null) =>
            builder.TryAddFileOrFirstConventionalFileCore(
                ref fileOrDirectory,
                ref alternativeFileNames,
                out addedFilePath,
                configureProviderBuilder);

        /// <summary>
        /// Adds first existing file with the help of current <see cref="IConventionalConfigurationBuilder.ConventionalFileFinders"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="fileOrDirectory"></param>
        /// <param name="alternativeFileNames"></param>
        /// <param name="addedFilePath"></param>
        /// <param name="configureProviderBuilder"></param>
        public static IConventionalConfigurationBuilder AddFileOrFirstConventionalFile(
            this IConventionalConfigurationBuilder builder,
            string fileOrDirectory,
            IEnumerable<string> alternativeFileNames,
            out string addedFilePath,
            Action<IFileShadowedConfigurationProviderBuilderConfigurer>? configureProviderBuilder = null)
        {
            if (!TryAddFileOrFirstConventionalFileCore(builder, ref fileOrDirectory, ref alternativeFileNames, out var nullableAddedFilePath, configureProviderBuilder)) {
                ThrowFileNotFound(fileOrDirectory, alternativeFileNames);
            }

            addedFilePath = nullableAddedFilePath;
            return builder;
        }
    }
}
