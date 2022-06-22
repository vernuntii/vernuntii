namespace Vernuntii.Text.Json
{
    /// <summary>
    /// Enables to write any value that is serializable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValueWriter<in T> where T : class
    {
        /// <summary>
        /// Overwrites the value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        void Overwrite(T value);
    }
}
