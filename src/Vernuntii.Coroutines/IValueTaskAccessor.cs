namespace Vernuntii.Coroutines;

internal interface IValueTaskAccessor
{
    internal object? _obj { get; set; }
    internal short _token { get; set; }
}
