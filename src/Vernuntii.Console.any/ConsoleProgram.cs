using Vernuntii.Runner;

await using var runner = VernuntiiRunnerBuilder.WithNextVersionRequirements().Build(args);
return await runner.RunAsync();
