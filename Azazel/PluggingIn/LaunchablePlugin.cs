using System.Collections.Generic;
using Azazel.FileSystem;

namespace Azazel.PluggingIn {
    public interface LaunchablePlugin: Plugin {
        Launchables Launchables();
    }

    public class LaunchablePlugins : List<LaunchablePlugin> {}
}