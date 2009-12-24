using System;
using System.IO;
using xstream;
using xstream.Converters;

namespace Azazel.FileSystem {
    public abstract class FileSystemElement<T, U> where T : FileSystemInfo where U : FileSystemElement<T, U> {
        protected readonly T fileSystemInfo;

        public T FileSystemInfo {
            get { return fileSystemInfo; }
        }

        protected FileSystemElement(T fileSystemInfo) {
            this.fileSystemInfo = fileSystemInfo;
        }

        private FileAttributes Attributes {
            get { return fileSystemInfo.Attributes; }
        }

        public bool IsHidden {
            get { return (Attributes & FileAttributes.Hidden) == FileAttributes.Hidden; }
        }

        public virtual string FullName {
            get { return fileSystemInfo.FullName; }
        }

        public abstract string Name { get; }

        protected virtual bool Equals(FileSystemElement<T, U> fileSystemElement) {
            if (fileSystemElement == null) return false;
            return Equals(FullName, fileSystemElement.FullName);
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || Equals(obj as FileSystemElement<T, U>);
        }

        public override int GetHashCode() {
            return fileSystemInfo.FullName.GetHashCode();
        }

        protected class FileSystemElementConverter<V, X> : Converter where V : FileSystemElement<X, V> where X : FileSystemInfo {
            public delegate V NewFileSystemElement(string path);

            private readonly NewFileSystemElement newT;

            public FileSystemElementConverter(NewFileSystemElement newT) {
                this.newT = newT;
            }

            public bool CanConvert(Type type) {
                return typeof (V).Equals(type);
            }

            public void ToXml(object value, XStreamWriter writer, MarshallingContext context) {
                writer.SetValue(((V) value).fileSystemInfo.FullName);
            }

            public object FromXml(XStreamReader reader, UnmarshallingContext context) {
                return newT(reader.GetValue());
            }
        }
    }
}