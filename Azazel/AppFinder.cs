using System;
using System.IO;
using System.Windows.Forms;
using Azazel.FileSystem;
using Azazel.Logging;
using Azazel.PluggingIn;
using xstream;
using File=Azazel.FileSystem.File;

namespace Azazel {
    public class AppFinder {
        private readonly History history;
        private readonly XStream xstream = SelfPlugin.xstream;
        private Launchables allLaunchables;
        private readonly CharacterPlugins characterPlugins;
        private LaunchablesDictionary dictionary;

        public AppFinder(LaunchablePlugins launchablePlugins, CharacterPlugins characterPlugins) {
            LoadFiles(launchablePlugins);
            history = new History(new File(new FileInfo(Paths.Instance.History)), xstream);
            this.characterPlugins = characterPlugins;
        }

        internal void LoadFiles(LaunchablePlugins launchablePlugins) {
            var foldersToParse = new FoldersToParse(LoadFoldersToParse(), launchablePlugins);
            dictionary = foldersToParse.LoadLaunchables();
            allLaunchables = dictionary.Launchables;
            foldersToParse.LaunchablesChanged += ((plugin, launchables) => new Runner(delegate {
                                                                                          LogManager.WriteLog("Refresh caused by " + plugin);
                                                                                          dictionary.SetValue(plugin.GetType(), launchables);
                                                                                          allLaunchables = dictionary.Launchables;
                                                                                      }).AsyncStart());
        }

        private Folders LoadFoldersToParse() {
            return PersistanceHelper.SaveOrLoadAndSave<Folders>(xstream, Paths.Instance.Folders, () => File.Contents(Paths.Instance.Folders),
                                                                StoreDefaultFolders,
                                                                "Because of some error (probably connected with plugins) all the folders you have added have been lost!! You will have to add them again!!");
        }

        private void StoreDefaultFolders() {
            File.WriteAllText(Paths.Instance.Folders, xstream.ToXml(new Folders(Folders.QuickLaunch, Folders.StartMenu, Folders.AllUsersStartMenu)));
        }

        public Launchables FindFiles(string searchString) {
            return FindFiles(allLaunchables, searchString);
        }

        public Launchables FindFiles(Launchables launchables, string searchString) {
            foreach (var plugin in characterPlugins) {
                if (plugin.IsValidFor(searchString))
                    return new Launchables(plugin.Launchable(searchString));
            }
            return launchables.Find(searchString, history);
        }

        public void AppLaunched(string input, Launchable file) {
            new Thread(() => history.Add(input, file)).Start();
        }

        public void AddFolder(LaunchablePlugins launchablePlugins) {
            var folders = LoadFoldersToParse();
            var dialog = new FolderBrowserDialog {SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal)};
            if (dialog.ShowDialog() == DialogResult.OK) folders.Add(new Folder(dialog.SelectedPath));
            File.WriteAllText(Paths.Instance.Folders, xstream.ToXml(folders));
            LoadFiles(launchablePlugins);
        }
    }
}