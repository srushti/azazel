using System;
using System.IO;

namespace Azazel.FileSystem {
    public class FileSystemStalker {
        private readonly FileChangedDelegate changedDelegate;
        private readonly FileSystemWatcher watcher;

        public FileSystemStalker(Folder folder, FileChangeTypes fileChangeTypes, FileChangedDelegate changedDelegate) {
            this.changedDelegate = changedDelegate;
            if (!folder.Exists()) return;
            watcher = new FileSystemWatcher(folder.FullName) {IncludeSubdirectories = true, EnableRaisingEvents = true};
            if (AskedForEvent(fileChangeTypes, FileChangeTypes.Created)) watcher.Created += RaiseEvent;
            if (AskedForEvent(fileChangeTypes, FileChangeTypes.Changed)) watcher.Changed += RaiseEvent;
            if (AskedForEvent(fileChangeTypes, FileChangeTypes.Deleted)) watcher.Deleted += RaiseEvent;
            if (AskedForEvent(fileChangeTypes, FileChangeTypes.Renamed)) watcher.Renamed += RaiseEvent;
        }

        private void RaiseEvent(object sender, FileSystemEventArgs e) {
            changedDelegate(new File(e.FullPath));
        }

        private static bool AskedForEvent(FileChangeTypes fileChangeTypes, FileChangeTypes eventToCheck) {
            return (eventToCheck & fileChangeTypes) == eventToCheck;
        }
    }

    public delegate void FileChangedDelegate(File changedFile);

    [Flags]
    public enum FileChangeTypes {
        Created = 1,
        Changed = 2,
        Deleted = 4,
        Renamed = 8,
        AllOfTheAbove = 0xFFF,
    }
}