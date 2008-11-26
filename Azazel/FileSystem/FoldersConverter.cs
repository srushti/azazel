using System;
using xstream;
using xstream.Converters;

namespace Azazel.FileSystem {
    internal class FoldersConverter : Converter {
        public bool CanConvert(Type type) {
            return typeof (Folders).Equals(type);
        }

        public void ToXml(object value, XStreamWriter writer, MarshallingContext context) {
            var folders = (Folders) value;
            foreach (var folder in folders)
                context.ConvertOriginal(folder);
        }

        public object FromXml(XStreamReader reader, UnmarshallingContext context) {
            var folders = new Folders();
            var count = reader.NoOfChildren();
            reader.MoveDown();
            for (var i = 0; i < count; i++) {
                folders.Add((Folder) context.ConvertAnother());
                reader.MoveNext();
            }
            reader.MoveUp();
            return folders;
        }
    }
}