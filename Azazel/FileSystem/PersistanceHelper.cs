using System;
using Azazel.Logging;
using xstream;

namespace Azazel.FileSystem {
    public static class PersistanceHelper {
        public static T LoadOrSaveAndLoad<T>(XStream xStream, string filePath, T defaultObject) {
            if (!File.Exists(filePath))
                Save(filePath, xStream, defaultObject);
            try {
                return (T) xStream.FromXml(((LoadFile) (() => File.Contents(filePath)))().Trim());
            }
            catch (Exception e) {
                LogManager.WriteLog(e);
                var file = new File(filePath);
                File.WriteAllText(
                    file.FullName + DateTime.Now.ToString().Replace("/", "").Replace("\\", "").Replace(" ", "").Replace(":", "") + file.Extension,
                    File.Contents(filePath));
                Save(filePath, xStream, defaultObject);
                return (T) xStream.FromXml(File.Contents(filePath).Trim());
            }
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