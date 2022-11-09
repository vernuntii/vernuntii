namespace Vernuntii.PluginSystem
{
    internal interface ISealed
    {
        bool IsSealed { get; }

        public void ThrowIfSealed()
        {
            if (IsSealed) {
                throw new InvalidOperationException($"The object of type {GetType()} is sealed and cannot be changed anymore");
            }
        }

        public void ThrowIfNotSealed()
        {
            if (!IsSealed) {
                throw new InvalidOperationException($"The object of type {GetType()} is not sealed yet");
            }
        }
    }
}
