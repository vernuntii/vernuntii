namespace Vernuntii.Text.Json
{
    /// <summary>
    /// Enables to write any value that is serializable.
    /// </summary>
    public interface IManagedValueWriter<in T> : IValueWriter<T>, IDisposable
        where T : class
    {
    }
}
