using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Browser {
    public class InternetExplorer : LaunchablePlugin {
        private static readonly ImageSource icon = UrlLauncher.BrowserIcon;
        private readonly string path;

        public InternetExplorer() {
            path = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
            new FileSystemStalker(new Folder(path), FileChangeTypes.Created | FileChangeTypes.Deleted, delegate { Changed(this); });
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

        public event PluginChangedDelegate Changed = delegate { };

        public static Bookmark CreateBookmark(string name, string contents) {
            return new Bookmark(name, Regex.Match(contents, "URL=(.*)\r").Groups[1].Value, icon);
        }
    }
}