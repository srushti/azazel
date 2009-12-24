using System;
using Azazel.Logging;
using xstream;

namespace Azazel.FileSystem {
    public class PersistanceHelper {
        private XStream xStream;

        public PersistanceHelper(XStream xStream) {
            this.xStream = xStream;
        }

        public T LoadOrSaveAndLoad<T>(string filePath, T defaultObject) {
            if (!File.Exists(filePath))
                Save(filePath, xStream, defaultObject);
            try {
                return (T) xStream.FromXml(((LoadFile) (() => File.Contents(filePath)))().Trim());
            }
            catch (Exception e) {
                LogManager.WriteLog(e);
                return SaveAndLoad(filePath, defaultObject);
            }
        }

        public T SaveAndLoad<T>(string filePath, T defaultObject) {
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