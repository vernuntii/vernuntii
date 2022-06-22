using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Represents a subscription that can be signaled once.
    /// </summary>
    public interface IOneSignalSubscription : IOneSignal, IDisposable
    {
        /// <summary>
        /// <see langword="true"/> if subscription has been disposed.
        /// </summary>
        bool IsDisposed { get; }
    }
}
