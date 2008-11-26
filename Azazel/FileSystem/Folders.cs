using System.Collections.Generic;
using System.IO;

namespace Azazel.FileSystem {
    public class Folders : List<Folder> {
        internal static readonly Folder QuickLaunch = new Folder(Constants.QuickLaunch);
        internal static readonly Folder StartMenu = new Folder(Constants.StartMenu);
        internal static readonly Folder AllUsersStartMenu = new Folder(Constants.AllUsersStartMenu);

        public Folders(params Folder[] folders) : this((IEnumerable<Folder>) folders) {}

        public Folders(IEnumerable<Folder> folders) {
            AddRange(folders);
        }

        public Folders(IEnumerable<DirectoryInfo> directoryInfos) {
            foreach (var directoryInfo in directoryInfos) Add(new Folder(directoryInfo));
        }

        public Files GetFiles() {
            var files = new Files();
            var copyOfFoldersToIterate = new Folders(this);
            foreach (var folder in copyOfFoldersToIterate) {
                if (!Folder.Exists(folder.FullName)) Remove(folder);
                files.AddRange(folder.GetFiles());
            }
            return files;
        }
    }
}