using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Vernuntii.Git;
using Vernuntii.Plugins;

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
                plugins.Add(PluginAction.WhenExecuting<IGitPlugin>.CreatePluginDescriptor(plugin =>
                    plugin.SetAlternativeRepository(_repository, _repository.GitCommand)));

                plugins.Add(PluginAction.WhenExecuting<ILoggingPlugin>.CreatePluginDescriptor(plugin =>
                    plugin.WriteToStandardError = false));
            })
            .Build(new[] {
                "--cache-id",
                cacheId,
                "--verbosity",
                "Verbose"
            });

        private VernuntiiRunner CreateStaticRunner() => CreateRunner(nameof(VernuntiiRunnerBenchmarks));

        [GlobalSetup(Target = nameof(RunConsoleWithCache))]
        public Task BeforeRunConsoleWithCache() => CreateStaticRunner().RunConsoleAsync();

        [Benchmark]
        public Task<int> RunConsoleWithCache() => CreateStaticRunner().RunConsoleAsync();

        [IterationSetup(Target = nameof(RunConsoleWithoutCache))]
        public void BeforeRunConsoleWithoutCache() => _randomCacheId = Guid.NewGuid().ToString();

        [Benchmark]
        public Task<int> RunConsoleWithoutCache() => CreateRunner(_randomCacheId).RunConsoleAsync();

        [GlobalCleanup]
        public void Cleanup() => _repository.Dispose();
    }
}
