using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Azazel.FileSystem;
using Azazel.PluggingIn;
using File=Azazel.FileSystem.File;

namespace Venus.Browser {
    public class InternetExplorer : LaunchablePlugin {
        private static readonly ImageSource icon = UrlLauncher.BrowserIcon;
        private readonly string path;

        public InternetExplorer() {
            path = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
            var watcher = new FileSystemWatcher(path, "*.url");
            watcher.Created += delegate { FileChanged(this); };
            watcher.Deleted += delegate { FileChanged(this); };
        }

        public bool IsAvailable {
            get { return Folder.Exists(path); }
        }

        public Launchables Launchables() {
            var folder = new Folder(path);
            var favouritesFiles = new Launchables();
            folder.GetFiles().ForEach(launchable => { if (((File) launchable).FullName.EndsWith(".url")) favouritesFiles.Add(launchable); });
            var favourites = new Launchables();
            foreach (File favouritesFile in favouritesFiles) {
                var contents = favouritesFile.Contents();
                favourites.Add(CreateBookmark(favouritesFile.Name, contents));
            }
            return favourites;
        }

        public event FileChangedDelegate FileChanged = delegate { };

        public static Bookmark CreateBookmark(string name, string contents) {
            return new Bookmark(name, Regex.Match(contents, "URL=(.*)\r").Groups[1].Value, icon);
        }
    }
}