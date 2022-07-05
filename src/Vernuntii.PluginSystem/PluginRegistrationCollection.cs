using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vernuntii.PluginSystem
{
    internal class PluginRegistrationCollection
    {
        public IReadOnlySet<PluginRegistration> Sorted => _sorted;

        public IReadOnlyDictionary<Type, IPlugin> AcendedPlugins {
            get {
                _sealedObject.EnsureSealed();
                return _ascendedPlugins;
            }
        }

        private SortedSet<PluginRegistration> _sorted = new SortedSet<PluginRegistration>(PluginRegistrationComparer.Default);
        private Dictionary<Type, IPlugin> _ascendedPlugins = new Dictionary<Type, IPlugin>();
        private ISealed _sealedObject;

        public PluginRegistrationCollection(ISealed sealedObject) =>
            _sealedObject = sealedObject ?? throw new ArgumentNullException(nameof(sealedObject));

        public void Add(PluginRegistration pluginRegistration) =>
            _sorted.Add(pluginRegistration);

        private void InitializeFirstPluginByTypeDictinary()
        {
            foreach (var pluginRegistration in _sorted) {
                var pluginServiceType = pluginRegistration.ServiceType;

                if (_ascendedPlugins.ContainsKey(pluginServiceType)) {
                    continue;
                }

                _ascendedPlugins.Add(pluginServiceType, pluginRegistration.Plugin);
            }
        }

        public void Seal()
        {
            _sealedObject.EnsureNotSealed();
            InitializeFirstPluginByTypeDictinary();
        }
    }
}
