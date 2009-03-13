using System.Collections.Generic;
using System.IO;
using Azazel.PluggingIn;

namespace Azazel.FileSystem {
    public class Folders : List<Folder>, LaunchablePlugin {
        internal static readonly Folder QuickLaunch = new Folder(Constants.QuickLaunch);
        internal static readonly Folder StartMenu = new Folder(Constants.StartMenu);
        internal static readonly Folder AllUsersStartMenu = new Folder(Constants.AllUsersStartMenu);

        public Folders(params Folder[] folders) : this((IEnumerable<Folder>) folders) {}

        public Folders(IEnumerable<Folder> folders) {
            AddRange(folders);
            foreach (var folder in folders) WatchFolder(folder);
        }

        private void WatchFolder(Folder folder) {
            new FileSystemStalker(folder, FileChangeTypes.Created | FileChangeTypes.Deleted | FileChangeTypes.Renamed, delegate(File changedFile) {
                                                                                                                           if (!changedFile.Exists() ||
                                                                                                                               !changedFile.IsHidden)
                                                                                                                               Changed(this);
                                                                                                                       });
        }

        public new void Add(Folder folder) {
            base.Add(folder);
            WatchFolder(folder);
        }

        public Folders(IEnumerable<DirectoryInfo> directoryInfos) {
            foreach (var directoryInfo in directoryInfos) Add(new Folder(directoryInfo));
        }

        public Launchables Launchables() {
            var files = new Files();
            var copyOfFoldersToIterate = new Folders(this);
            foreach (var folder in copyOfFoldersToIterate) {
                if (!Folder.Exists(folder.FullName)) Remove(folder);
                files.AddRange(folder.GetFiles());
            }
            return files;
        }

        public event PluginChangedDelegate Changed = delegate { };

        public bool IsAvailable {
            get { return true; }
        }
    }
}