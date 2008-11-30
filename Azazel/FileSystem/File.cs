using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using xstream.Converters;

namespace Azazel.FileSystem {
    public class File : FileSystemElement<FileInfo, File>, Launchable {
        public static Converter Converter = new FileSystemElementConverter<File, FileInfo>(path => new File(path));
        public static readonly File NULL = new NullFile();

        internal File(FileInfo fileInfo) : base(fileInfo) {}
        public File(string path) : this(new FileInfo(path)) {}

        protected File() : this((FileInfo) null) {}

        public override string Name {
            get { return Path.GetFileNameWithoutExtension(fileSystemInfo.Name); }
        }

        public virtual ImageSource Icon {
            get { return IconExtractor.Instance.Extract(this, IconSize.Large); }
        }

        public Folder ParentFolder {
            get { return new Folder(fileSystemInfo.Directory); }
        }

        public static File First(Files fileInfos) {
            foreach (File file in fileInfos)
                if (file.IsHidden) return file;
            return NULL;
        }

        public override string ToString() {
            return Name;
        }

        private class NullFile : File {
            public override string Name {
                get { return FullName; }
            }

            public override ImageSource Icon {
                get { return new BitmapImage(); }
            }

            public override string FullName {
                get { return ""; }
            }

            public override void Launch(string arguments) {}

            public override bool ShouldStoreHistory {
                get { return false; }
            }

            public override string Contents() {
                return
                    @"<DictionaryOfStringAndLaunchable class='System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[Azazel.FileSystem.Launchable, Azazel, Version=0.9.2867.32514, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'>
</DictionaryOfStringAndLaunchable>";
            }

            public override void WriteAllText(string contents) {}

            public override bool Exists() {
                return false;
            }

            private static bool Equals(NullFile nullFile) {
                return nullFile != null;
            }

            public override bool Equals(FileSystemElement<FileInfo, File> fileSystemElement) {
                return Equals(((object) fileSystemElement));
            }

            public override bool Equals(object obj) {
                return Equals(obj as NullFile);
            }

            public override int GetHashCode() {
                return 0;
            }
        }

        public virtual void Launch(string arguments) {
            new Runner(FullName, arguments).AsyncStart();
        }

        public virtual bool ShouldStoreHistory {
            get { return true; }
        }

        public Actions Actions {
            get { return new Actions(); }
        }

        public delegate bool BoolDelegate();

        public static bool Exists(string path) {
            return System.IO.File.Exists(path);
        }

        public static void WriteAllText(string path, string contents) {
            var parentFolderPath = new File(path).ParentFolder.FullName;
            if (!Folder.Exists(parentFolderPath)) Folder.Create(parentFolderPath);
            System.IO.File.WriteAllText(path, contents);
        }

        public static string Contents(string path) {
            return System.IO.File.ReadAllText(path);
        }

        public virtual void WriteAllText(string contents) {
            WriteAllText(FullName, contents);
        }

        public virtual string Contents() {
            return Contents(FullName);
        }

        public virtual bool Exists() {
            return Exists(FullName);
        }

        public File Copy(string destinationFileName) {
            if (!Path.IsPathRooted(destinationFileName)) destinationFileName = Paths.Combine(ParentFolder.FullName, destinationFileName);
            System.IO.File.Copy(FullName, destinationFileName, true);
            return new File(destinationFileName);
        }
    }
}
