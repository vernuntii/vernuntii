using Vernuntii.Console;

await using var runner = new VernuntiiRunnerBuilder().Build(args);
return await runner.RunConsoleAsync();
