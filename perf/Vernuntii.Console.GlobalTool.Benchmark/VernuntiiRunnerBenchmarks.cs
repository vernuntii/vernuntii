using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vernuntii.Git;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;

namespace Vernuntii.Console.GlobalTool.Benchmark
{
    [SimpleJob(RunStrategy.Monitoring)]
    public class VernuntiiRunnerBenchmarks
    {
        private TemporaryRepository _repository = null!;
        private string _randomCacheId = null!;

        public VernuntiiRunnerBenchmarks()
        {
            _repository = new TemporaryRepository();
            System.Console.WriteLine($"Use repository: {_repository.GitCommand.WorkingTreeDirectory}");
            _repository.CommitEmpty();
        }

        private VernuntiiRunner CreateRunner(string cacheId) => new VernuntiiRunner() {
            ConsoleArgs = new[] {
                "--cache-id",
                cacheId,
                "--verbosity",
                "Verbose"
            },
            PluginDescriptors = new[] {
                PluginDescriptor.Create(new PluginAction.AfterRegistration<IGitPlugin>(plugin =>
                    plugin.SetAlternativeRepository(_repository, _repository.GitCommand))),
                PluginDescriptor.Create(new PluginAction.AfterRegistration<ILoggingPlugin>(plugin =>
                    plugin.WriteToStandardError = false))
            }
        };

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
