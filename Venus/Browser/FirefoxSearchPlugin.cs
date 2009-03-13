using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Azazel.Extensions;
using Azazel.FileSystem;
using Azazel.PluggingIn;
using Microsoft.Win32;
using File=Azazel.FileSystem.File;

namespace Venus.Browser {
    public class FirefoxSearchPlugin : LaunchablePlugin {
        private readonly Folder searchPluginsFolder;
        private readonly bool isAvailable = true;

        public FirefoxSearchPlugin() {
            try {
                var firefoxSubKey = Registry.LocalMachine.OpenSubKey(@"software\mozilla\mozilla firefox");
                var curVersionNumber = (string) firefoxSubKey.GetValue("CurrentVersion");
                var firefoxInstallationDirectory = (string) firefoxSubKey.OpenSubKey(curVersionNumber + @"\Main").GetValue("Install Directory");
                searchPluginsFolder = new Folder(Paths.Combine(firefoxInstallationDirectory, "searchplugins"));
                var watcher = new FileSystemWatcher(searchPluginsFolder.FullName);
                watcher.Created += delegate { FileChanged(this); };
                watcher.Deleted += delegate { FileChanged(this); };
            }
            catch (NullReferenceException) {
                isAvailable = false;
            }
        }

        public bool IsAvailable {
            get { return isAvailable; }
        }

        public Launchables Launchables() {
            var launchables = new Launchables();
            var searchConfigurations = new Launchables();
            searchPluginsFolder.GetFiles().ForEach(launchable => { if (((File) launchable).FullName.EndsWith(".xml")) searchConfigurations.Add(launchable); });
            foreach (File searchConfiguration in searchConfigurations) launchables.Add(CreateSearchPlugin(searchConfiguration.Contents()));
            return launchables;
        }

        public event FileChangedDelegate FileChanged = delegate { };

        public static Search CreateSearchPlugin(string contents) {
            var name = Regex.Match(contents, "<ShortName>(.*)</ShortName>").Groups[1].Value;
            var url = Regex.Match(contents, "<SearchForm>(.*)</SearchForm>").Groups[1].Value;
            var searchQuery = ParseSearchQuery(contents);
            return new Search(name, url, searchQuery, UrlLauncher.BrowserIcon);
        }

        private static string ParseSearchQuery(string contents) {
            var searchQuery = new StringBuilder();
            var searchNode = contents.Substring("<Url type=\"text/html\"", "<!-- Dynamic parameters -->").Replace("{searchTerms}", "%s");
            searchQuery.Append(Regex.Match(searchNode, "template=\"(.*)\">").Groups[1].Value);
            searchQuery.Append("?");
            foreach (Match match in Regex.Matches(searchNode, "<Param name=\"(.*)\" value=\"(.*)\"/>")) {
                searchQuery.Append(match.Groups[1]);
                searchQuery.Append("=");
                searchQuery.Append(match.Groups[2]);
                searchQuery.Append("&");
            }
            return searchQuery.ToString();
        }
    }
}