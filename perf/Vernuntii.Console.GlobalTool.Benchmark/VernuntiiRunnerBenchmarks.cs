using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Vernuntii.Git;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.Reactive;
using Vernuntii.Runner;

namespace Vernuntii.Console.GlobalTool.Benchmark
{
    [SimpleJob(RunStrategy.Monitoring)]
    public class VernuntiiRunnerBenchmarks
    {
        private readonly TemporaryRepository _repository = null!;
        private string _randomCacheId = null!;

        public VernuntiiRunnerBenchmarks()
        {
            _repository = new TemporaryRepository();
            System.Console.WriteLine($"Use repository: {_repository.GitCommand.WorkingTreeDirectory}");
            _repository.CommitEmpty();
        }

        private VernuntiiRunner CreateRunner(string cacheId) => new VernuntiiRunnerBuilder()
            .ConfigurePlugins(plugins => {
                plugins.Add(PluginAction.HandleEvents(events =>
                    events.Every(GitEvents.OnCustomizeGitCommandCreation).Subscribe(request => request.GitCommandFactory = new GitCommandFactory(_repository.GitCommand))));

                plugins.Add(PluginAction.HandlePlugin<ILoggingPlugin>(plugin =>
                    plugin.WriteToStandardError = false));
            })
            .Build(new[] {
                "--cache-id",
                cacheId,
                "--verbosity",
                "Verbose"
            });

        private VernuntiiRunner CreateStaticRunner() => CreateRunner("BENCHMARKS");

        [GlobalSetup(Target = nameof(RunConsoleWithCache))]
        public async Task BeforeRunConsoleWithCache() => await CreateStaticRunner().RunAsync();

        [Benchmark]
        public async Task RunConsoleWithCache() => await CreateStaticRunner().RunAsync();

        //[IterationSetup(Target = nameof(RunConsoleWithoutCache))]
        //public void BeforeRunConsoleWithoutCache() => _randomCacheId = Guid.NewGuid().ToString();

        //[Benchmark]
        //public Task<int> RunConsoleWithoutCache() => CreateRunner(_randomCacheId).RunAsync();

        [GlobalCleanup]
        public void Cleanup() => _repository.Dispose();

        private class GitCommandFactory : IGitCommandFactory
        {
            private readonly IGitCommand _gitCommand;

            public GitCommandFactory(IGitCommand gitCommand) =>
                _gitCommand = gitCommand;

            public IGitCommand CreateCommand(string gitDirectory) =>
                _gitCommand;
        }
    }
}
