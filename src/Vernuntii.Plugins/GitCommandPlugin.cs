using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Git;
using Vernuntii.Git.Command;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins
{
    /// <summary>
    
    /// </summary>
    public class GitCommandPlugin : Plugin, IGitCommandPlugin
    {

        /// <inheritdoc/>
        protected override void OnEvents()
        {
        }
    }
}
