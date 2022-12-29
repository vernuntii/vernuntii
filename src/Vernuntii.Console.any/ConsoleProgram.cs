using Vernuntii.Console;

await using var runner = VernuntiiRunnerBuilder.ForNextVersion().Build(args);
return await runner.RunAsync();
