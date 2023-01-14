using System.CommandLine;

namespace Vernuntii.Plugins.CommandLine
{
    /// <summary>
    /// A wrapper for <see cref="Command"/>.
    /// </summary>
    public interface ICommand : IExtensibleCommand
    {
        /// <summary>
        /// Sets the command handler.
        /// </summary>
        /// <param name="handlerFunc"></param>
        void SetHandler(Func<Task<int>>? handlerFunc);
    }
}
