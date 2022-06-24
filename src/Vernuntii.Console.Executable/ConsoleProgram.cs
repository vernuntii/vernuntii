using Vernuntii.Console;

await using var runner = new VernuntiiRunner() {
    ConsoleArgs = args
};

return await runner.RunConsoleAsync();
