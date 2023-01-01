namespace Vernuntii.Serialization
{
    /// <summary>
    /// Enables to write any value that is serializable.
    /// </summary>
    public interface IManagedValueWriter<in T> : IValueWriter<T>, IDisposable
        where T : class
    {
    }
}
