using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Browser {
    public class InternetExplorer : LaunchablePlugin {
        private static readonly ImageSource icon = UrlLauncher.BrowserIcon;

        public bool IsAvailable {
            get { return true; }
        }

        public Launchables Launchables() {
            var folder = new Folder(Environment.GetFolderPath(Environment.SpecialFolder.Favorites));
            var favouritesFiles = new Launchables();
            folder.GetFiles().ForEach(delegate(Launchable launchable) { if (((File) launchable).FullName.EndsWith(".url")) favouritesFiles.Add(launchable); });
            var favourites = new Launchables();
            foreach (File favouritesFile in favouritesFiles) {
                var contents = favouritesFile.Contents();
                favourites.Add(CreateBookmark(favouritesFile.Name, contents));
            }
            return favourites;
        }

        public static Bookmark CreateBookmark(string name, string contents) {
            return new Bookmark(name, Regex.Match(contents, "URL=(.*)\r").Groups[1].Value, icon);
        }
    }
}