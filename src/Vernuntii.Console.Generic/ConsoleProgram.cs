using Vernuntii.Console;

var i = new B() as A as I;
i.Call();

await using var runner = new VernuntiiRunner() {
    ConsoleArgs = args
};

return await runner.RunConsoleAsync();

interface I
{
    void Call();
}

public class A : I
{
    void I.Call() => Console.WriteLine("A");
}

public class B : A, I
{
    void I.Call() {
        Console.WriteLine("B");
        A test = this;
        var test2 = (I)test;
        test2.Call();
    }
}
