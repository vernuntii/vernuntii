namespace Vernuntii.PluginSystem
{
    internal interface ISealed
    {
        bool IsSealed { get; }

        public void EnsureNotSealed()
        {
            if (IsSealed) {
                throw new InvalidOperationException($"The object of type {GetType()} is sealed and cannot be changed anymore");
            }
        }

        public void EnsureSealed()
        {
            if (IsSealed) {
                throw new InvalidOperationException($"The object of type {GetType()} is not sealed yet");
            }
        }
    }
}
