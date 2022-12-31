using Vernuntii.Runner;

await using var runner = VernuntiiRunnerBuilder.ForNextVersion().Build(args);
return await runner.RunAsync();
