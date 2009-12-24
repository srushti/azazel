using System.Xml.Serialization;
using Azazel.Logging;
using Azazel.PluggingIn;

namespace Azazel.FileSystem {
    public class FoldersToParse {
        [XmlIgnore] private readonly LaunchablePlugins launchablePlugins;

        public FoldersToParse(Folders folders, LaunchablePlugins launchablePlugins) {
            this.launchablePlugins = new LaunchablePlugins(launchablePlugins) {folders};
        }

        public LaunchablesDictionary LoadLaunchables() {
            LogManager.WriteLog("Loading all launchables");
            var dictionary = new LaunchablesDictionary();
            foreach (var plugin in launchablePlugins) {
                LogManager.WriteLog("Loading launchables from " + plugin);
                dictionary.SetValue(plugin.GetType(), plugin.Launchables());
                plugin.Changed += FileChanged;
            }
            LogManager.WriteLog("Done loading all launchables");
            return dictionary;
        }

        private void FileChanged(LaunchablePlugin plugin) {
            LaunchablesChanged(plugin, plugin.Launchables());
        }

        public event LaunchablesChangedDelegate LaunchablesChanged = delegate { };
    }

    public delegate void LaunchablesChangedDelegate(LaunchablePlugin plugin, Launchables launchables);
}