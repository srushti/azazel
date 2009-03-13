using System.Collections.Generic;
using Azazel.FileSystem;

namespace Azazel.PluggingIn {
    public interface LaunchablePlugin : Plugin {
        Launchables Launchables();
        event FileChangedDelegate FileChanged;
    }

    public delegate void FileChangedDelegate(LaunchablePlugin plugin);

    public class LaunchablePlugins : List<LaunchablePlugin> {
        public LaunchablePlugins(IEnumerable<LaunchablePlugin> launchablePlugins) : base(launchablePlugins) {}

        public LaunchablePlugins() {}
    }
}