using System.Xml.Serialization;
using Azazel.PluggingIn;

namespace Azazel.FileSystem {
    public class FoldersToParse {
        internal readonly Folders Folders;
        [XmlIgnore] private readonly LaunchablePlugins launchablePlugins;

        public FoldersToParse(Folders folders, LaunchablePlugins launchablePlugins) {
            Folders = folders;
            this.launchablePlugins = launchablePlugins;
        }

        public Launchables LoadFiles() {
            var files = Folders.GetFiles();
            foreach (var plugin in launchablePlugins) files.AddRange(plugin.Launchables());
            return files;
        }

        public void AddFolder(string path) {
            var newFolder = new Folder(path);
            if (!Folders.Contains(newFolder)) Folders.Add(newFolder);
        }
    }
}