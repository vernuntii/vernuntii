using Vernuntii.Runner;

await using var runner = new VernuntiiRunnerBuilder().Build(args);
return await runner.RunAsync();
