using System;
using System.IO;
using xstream.Converters;

namespace Azazel.FileSystem {
    public class Folder : FileSystemElement<DirectoryInfo, Folder> {
        public static Converter Converter = new FileSystemElementConverter<Folder, DirectoryInfo>(path => new Folder(path));
        public Folder(string folderPath) : this(new DirectoryInfo(folderPath)) {}
        public Folder(DirectoryInfo directoryInfo) : base(directoryInfo) {}

        public Folders GetFolders() {
            return new Folders(fileSystemInfo.GetDirectories());
        }

        private Files GetTopFiles() {
            return new Files(fileSystemInfo.GetFiles());
        }

        public override string ToString() {
            return fileSystemInfo.Name;
        }

        public File GetFile(string fileName) {
            return new File(Paths.Combine(fileSystemInfo.FullName, fileName));
        }

        public Launchables GetFiles() {
            try {
                var files = GetTopFiles();
                GetFolders().ForEach(folder => {
                                         if (!folder.IsHidden)
                                             files.AddRange(folder.GetFiles());
                                     });
                return files;
            }
            catch (UnauthorizedAccessException) {
                return new Launchables();
            }
        }

        public Files GetFiles(string searchPattern) {
            return new Files(fileSystemInfo.GetFiles(searchPattern));
        }

        public static bool Exists(string path) {
            return Directory.Exists(path);
        }

        public bool Exists() {
            return Exists(FullName);
        }

        public static void Create(string path) {
            Directory.CreateDirectory(path);
        }

        public override string Name {
            get { return fileSystemInfo.Name; }
        }
    }
}
