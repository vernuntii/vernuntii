using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vernuntii.PluginSystem.Lifecycle;

namespace Vernuntii.PluginSystem
{
    internal class PluginRegistrations
    {
        public IReadOnlySet<PluginRegistration> Ordered => _orderedPlugins;

        public IReadOnlyDictionary<Type, IPlugin> FirstByServiceType {
            get {
                _sealedObject.ThrowIfNotSealed();
                return _ascendedPlugins;
            }
        }

        private SortedSet<PluginRegistration> _orderedPlugins = new SortedSet<PluginRegistration>(PluginRegistrationComparer.Default);
        private Dictionary<Type, IPlugin> _ascendedPlugins = new Dictionary<Type, IPlugin>();
        private ISealed _sealedObject;

        public PluginRegistrations(ISealed sealedObject) =>
            _sealedObject = sealedObject ?? throw new ArgumentNullException(nameof(sealedObject));

        public void Add(PluginRegistration pluginRegistration) =>
            _orderedPlugins.Add(pluginRegistration);

        private void InitializeFirstPluginByTypeDictinary()
        {
            foreach (var pluginRegistration in _orderedPlugins) {
                var pluginServiceType = pluginRegistration.ServiceType;

                if (_ascendedPlugins.ContainsKey(pluginServiceType)) {
                    continue;
                }

                _ascendedPlugins.Add(pluginServiceType, pluginRegistration.Plugin);
            }
        }

        public void Seal()
        {
            _sealedObject.ThrowIfSealed();
            InitializeFirstPluginByTypeDictinary();
        }
    }
}
