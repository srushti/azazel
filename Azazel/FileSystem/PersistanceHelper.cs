using System;
using Azazel.Logging;
using xstream;

namespace Azazel.FileSystem {
    public static class PersistanceHelper {
        public static T LoadOrSaveAndLoad<T>(XStream xStream, string filePath, LoadFile loadFile, VoidDelegate defaultSave, string messageBoxText) {
            if (!File.Exists(filePath))
                defaultSave();
            try {
                return (T) xStream.FromXml(loadFile().Trim());
            }
            catch (Exception e) {
                LogManager.WriteLog(e);
                var file = new File(filePath);
                File.WriteAllText(
                    file.FullName + DateTime.Now.ToString().Replace("/", "").Replace("\\", "").Replace(" ", "").Replace(":", "") + file.Extension,
                    File.Contents(filePath));
                defaultSave();
                return (T) xStream.FromXml(File.Contents(filePath).Trim());
            }
        }

        public static void Save<T>(XStream xStream, File file, T t) {
            try {
                file.WriteAllText(xStream.ToXml(t));
            }
            catch (Exception e) {
                LogManager.WriteLog(e);
            }
        }
    }

    public delegate string LoadFile();
}