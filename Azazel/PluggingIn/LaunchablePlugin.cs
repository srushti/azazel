using System.Collections.Generic;
using Azazel.FileSystem;

namespace Azazel.PluggingIn {
    public interface LaunchablePlugin : Plugin {
        Launchables Launchables();
        event PluginChangedDelegate Changed;
    }

    public delegate void PluginChangedDelegate(LaunchablePlugin plugin);

    public class LaunchablePlugins : List<LaunchablePlugin> {
        public LaunchablePlugins(IEnumerable<LaunchablePlugin> launchablePlugins) : base(launchablePlugins) {}

        public LaunchablePlugins() {}
    }
}