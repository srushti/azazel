using System;
using System.IO;

namespace Azazel.FileSystem {
    public class FileSystemStalker {
        private readonly FileChangeTypes fileChangeTypes;
        private readonly FileChangedDelegate changedDelegate;
        private readonly FileSystemWatcher watcher;

        public FileSystemStalker(Folder folder, FileChangeTypes fileChangeTypes, FileChangedDelegate changedDelegate) {
            this.fileChangeTypes = fileChangeTypes;
            this.changedDelegate = changedDelegate;
            watcher = new FileSystemWatcher(folder.FullName) {IncludeSubdirectories = true, EnableRaisingEvents = true};
            watcher.Created += ((sender, args) => RaiseEvent(FileChangeTypes.Created, args.FullPath));
            watcher.Changed += ((sender, args) => RaiseEvent(FileChangeTypes.Changed, args.FullPath));
            watcher.Deleted += ((sender, args) => RaiseEvent(FileChangeTypes.Deleted, args.FullPath));
            watcher.Renamed += ((sender, args) => RaiseEvent(FileChangeTypes.Renamed, args.FullPath));
        }

        private void RaiseEvent(FileChangeTypes changeType, string fileName) {
            if ((changeType & fileChangeTypes) == changeType) changedDelegate(new File(fileName));
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