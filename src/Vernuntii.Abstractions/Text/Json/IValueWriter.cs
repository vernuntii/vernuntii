namespace Vernuntii.Text.Json
{
    /// <summary>
    /// Enables to write any value that is serializable.
    /// </summary>
    public interface IValueWriter<in T> : IDisposable
        where T : class
    {
        /// <summary>
        /// Writes the value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        void WriteValue(T value);
    }
}
