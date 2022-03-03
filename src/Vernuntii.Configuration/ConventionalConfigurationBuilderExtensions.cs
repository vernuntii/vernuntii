namespace Vernuntii.Configuration
{
    /// <summary>
    /// Extension methods for <see cref="IConventionalConfigurationBuilder"/>.
    /// </summary>
    public static class ConventionalConfigurationBuilderExtensions
    {
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
            Action<IFileShadowedConfigurationProviderBuilderConfigurator>? configureProviderBuilder = null)
        {
            if (builder.ConventionalFileFinders.Count == 0) {
                throw new InvalidOperationException("No conventional file finder has been added");
            }

            var uniqueFileNames = new HashSet<string>(fileNames, StringComparer.OrdinalIgnoreCase);

            if (uniqueFileNames.Count == 0) {
                throw new ArgumentException("At least one file have to be specified", nameof(fileNames));
            }

            var candidates = uniqueFileNames
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
                        goto exit;
                    }
                } while (candidatesHavingNextEnumerator.MoveNext());

                // We previously had at least had one
                // successful MoveNext(), so we try again.
                goto nextCandidatesHavingNext;
            }

            throw new FileNotFoundException($"Not one of the following files has been found in {directoryPath} or above: {string.Join(", ", uniqueFileNames)}");

            exit:
            return builder;
        }

        /// <summary>
        /// Adds first existing file with the help of current <see cref="IConventionalConfigurationBuilder.ConventionalFileFinders"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="directoryPath"></param>
        /// <param name="fileNames"></param>
        /// <param name="configureProviderBuilder"></param>
        public static IConventionalConfigurationBuilder AddFirstConventionalFile(
            this IConventionalConfigurationBuilder builder,
            string directoryPath,
            IEnumerable<string> fileNames,
            Action<IFileShadowedConfigurationProviderBuilderConfigurator>? configureProviderBuilder = null) =>
            builder.AddFirstConventionalFile(
                directoryPath,
                fileNames,
                out _,
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
            Action<IFileShadowedConfigurationProviderBuilderConfigurator>? configureProviderBuilder = null)
        {
            string directory;
            IEnumerable<string> fileNames;

            if (Directory.Exists(fileOrDirectory)) {
                directory = fileOrDirectory;
                fileNames = alternativeFileNames;
            } else {
                directory = Path.GetDirectoryName(fileOrDirectory) ?? throw new ArgumentException("File path didn't include directory path", nameof(fileOrDirectory));
                var fileName = Path.GetFileName(fileOrDirectory);
                fileNames = fileName == null ? alternativeFileNames : new[] { fileName };
            }

            builder.AddFirstConventionalFile(directory, fileNames, out addedFilePath, configureProviderBuilder);
            return builder;
        }

        /// <summary>
        /// Adds first existing file with the help of current <see cref="IConventionalConfigurationBuilder.ConventionalFileFinders"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="fileOrDirectory"></param>
        /// <param name="alternativeFileNames"></param>
        /// <param name="configureProviderBuilder"></param>
        public static IConventionalConfigurationBuilder AddFileOrFirstConventionalFile(
            this IConventionalConfigurationBuilder builder,
            string fileOrDirectory,
            IEnumerable<string> alternativeFileNames,
            Action<IFileShadowedConfigurationProviderBuilderConfigurator>? configureProviderBuilder = null) =>
            builder.AddFileOrFirstConventionalFile(
                fileOrDirectory,
                alternativeFileNames,
                out _,
                configureProviderBuilder);
    }
}
