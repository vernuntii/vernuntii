using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vernuntii.PluginSystem
{
    internal class DuplicatePluginException : Exception
    {
        public DuplicatePluginException() : base()
        {
        }

        public DuplicatePluginException(string? message) : base(message)
        {
        }

        public DuplicatePluginException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
