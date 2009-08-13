using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Azazel.Extensions;
using Azazel.FileSystem;
using Azazel.PluggingIn;
using Microsoft.Win32;

namespace Venus.Browser {
    public class FirefoxSearchPlugin : LaunchablePlugin {
        private readonly Folder searchPluginsFolder;
        private readonly bool isAvailable = true;
        private readonly ImageSource icon = new PluginIconLoader().Png("firefox");

        public FirefoxSearchPlugin() {
            try {
                var firefoxSubKey = Registry.LocalMachine.OpenSubKey(@"software\mozilla\mozilla firefox");
                var curVersionNumber = (string) firefoxSubKey.GetValue("CurrentVersion");
                var firefoxInstallationDirectory = (string) firefoxSubKey.OpenSubKey(curVersionNumber + @"\Main").GetValue("Install Directory");
                searchPluginsFolder = new Folder(Paths.Combine(firefoxInstallationDirectory, "searchplugins"));
                new FileSystemStalker(searchPluginsFolder, FileChangeTypes.Created | FileChangeTypes.Deleted, delegate { Changed(this); });
                isAvailable = searchPluginsFolder.Exists();
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

        public event PluginChangedDelegate Changed = delegate { };

        public Search CreateSearchPlugin(string contents) {
            var name = Regex.Match(contents, "<ShortName>(.*)</ShortName>").Groups[1].Value;
            var url = Regex.Match(contents, "<SearchForm>(.*)</SearchForm>").Groups[1].Value;
            var searchQuery = ParseSearchQuery(contents);
            return new Search(name, url, searchQuery, icon);
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