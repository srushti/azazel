using System;
using Azazel.Logging;
using xstream;

namespace Azazel.FileSystem {
    public class PersistanceHelper {
        private XStream xStream;
        protected PersistanceHelper() {}

        public PersistanceHelper(XStream xStream) : this() {
            this.xStream = xStream;
        }

        public virtual T LoadOrSaveAndLoad<T>(string filePath, Func<T> defaultObjectCreator) {
            if (!File.Exists(filePath))
                Save(filePath, xStream, defaultObjectCreator());
            try {
                return (T) xStream.FromXml(((LoadFile) (() => File.Contents(filePath)))().Trim());
            }
            catch (Exception e) {
                LogManager.WriteLog(e);
                return SaveAndLoad(filePath, defaultObjectCreator());
            }
        }

        public T LoadOrSaveAndLoad<T>(string filePath, T defaultObject) {
            return LoadOrSaveAndLoad(filePath, () => defaultObject);
        }

        private T SaveAndLoad<T>(string filePath, T defaultObject) {
            var file = new File(filePath);
            File.WriteAllText(file.FullName + DateTime.Now.ToString().Replace("/", "").Replace("\\", "").Replace(" ", "").Replace(":", "") + file.Extension,
                              File.Contents(filePath));
            Save(filePath, xStream, defaultObject);
            return (T) xStream.FromXml(File.Contents(filePath).Trim());
        }

        public static void Save<T>(string path, XStream xStream, T t) {
            try {
                File.WriteAllText(path, xStream.ToXml(t));
            }
            catch (Exception e) {
                LogManager.WriteLog(e);
            }
        }
    }

    public delegate string LoadFile();
}