using FluentAssertions;
using Vernuntii.PluginSystem.Meta;
using Xunit;

namespace Vernuntii.PluginSystem
{
    public class PluginProviderTests
    {
        [Fact]
        public void Add_should_add_plugin_descriptor()
        {
            PluginProviderBuilder builder = new();
            PluginDescriptor pluginDescriptor = PluginDescriptor.Create<EmptyPlugin>();
            builder.Add(pluginDescriptor);
            IReadOnlySet<PluginDescriptor> set = builder.BuildOrderlySet();

            set.Should().ContainSingle().
                Subject.Should().BeEquivalentTo(pluginDescriptor);
        }

        [Fact]
        public void Add_should_supplement_plugin_descriptor()
        {
            PluginProviderBuilder builder = new();
            PluginDescriptor pluginDescriptor = PluginDescriptor.Create<PluginWithDependency>();
            builder.Add(pluginDescriptor);
            IReadOnlySet<PluginDescriptor> set = builder.BuildOrderlySet();

            PluginDescriptor supplementedPluginDescriptor = pluginDescriptor with
            {
                PluginDependencies = new[] { new ImportPluginAttribute<EmptyPlugin>() { TryRegister = true } }
            };

            set.Should().ContainInConsecutiveOrder(
                PluginDescriptor.Create<EmptyPlugin>(),
                supplementedPluginDescriptor);
        }

        [Fact]
        public void Build_should_consider_bigger_plugin_order()
        {
            PluginProviderBuilder builder = new();
            PluginDescriptor pluginDescriptor = PluginDescriptor.Create<PluginWithSmallerOrder>();
            builder.Add(pluginDescriptor);
            IReadOnlySet<PluginDescriptor> set = builder.BuildOrderlySet();

            PluginDescriptor supplementedPluginDescriptor = pluginDescriptor with
            {
                PluginDependencies = new[] { new ImportPluginAttribute<EmptyPlugin>() { TryRegister = true } },
                PluginOrder = 1
            };

            set.Should().ContainInConsecutiveOrder(
                supplementedPluginDescriptor,
                PluginDescriptor.Create<EmptyPlugin>());
        }

        private class EmptyPlugin : Plugin
        {
        }

        [ImportPlugin<EmptyPlugin>(TryRegister = true)]
        private class PluginWithDependency : Plugin
        {
        }

        [ImportPlugin<EmptyPlugin>(TryRegister = true)]
        [Plugin(Order = -1)]
        private class PluginWithSmallerOrder : Plugin
        {
        }
    }
}
