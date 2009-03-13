using System.Xml.Serialization;
using Azazel.PluggingIn;

namespace Azazel.FileSystem {
    public class FoldersToParse {
        [XmlIgnore] private readonly LaunchablePlugins launchablePlugins;

        public FoldersToParse(Folders folders, LaunchablePlugins launchablePlugins) {
            this.launchablePlugins = new LaunchablePlugins(launchablePlugins) {folders};
        }

        public LaunchablesDictionary LoadLaunchables() {
            var dictionary = new LaunchablesDictionary();
            foreach (var plugin in launchablePlugins) {
                dictionary.SetValue(plugin.GetType(), plugin.Launchables());
                plugin.FileChanged += FileChanged;
            }
            return dictionary;
        }

        private void FileChanged(LaunchablePlugin plugin) {
            LaunchablesChanged(plugin, plugin.Launchables());
        }

        public event LaunchablesChangedDelegate LaunchablesChanged = delegate { };
    }

    public delegate void LaunchablesChangedDelegate(LaunchablePlugin plugin, Launchables launchables);
}