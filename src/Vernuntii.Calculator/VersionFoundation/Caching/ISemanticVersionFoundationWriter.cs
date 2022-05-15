namespace Vernuntii.VersionFoundation.Caching
{
    /// <summary>
    /// Enables to write an instance of <see cref="ISingleVersionCalculation"/>.
    /// </summary>
    public interface ISemanticVersionFoundationWriter<in T> : IDisposable
        where T : class
    {
        /// <summary>
        /// Writes the value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        void WriteVersionFoundation(T value);
    }
}
