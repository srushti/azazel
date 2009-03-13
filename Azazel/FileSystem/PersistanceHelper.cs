using System;
using xstream;

namespace Azazel.FileSystem {
    public static class PersistanceHelper {
        public static T SaveOrLoadAndSave<T>(XStream xStream, string filePath, LoadFile loadFile, VoidDelegate defaultSave, string messageBoxText) {
            if (!File.Exists(filePath))
                defaultSave();
            try {
                return (T) xStream.FromXml(loadFile().Trim());
            }
            catch (Exception) {
                var file = new File(filePath);
                File.WriteAllText(file.FullName + DateTime.Now.ToString().Replace("/", "").Replace("\\", "").Replace(" ", "").Replace(":", "") + file.Extension,
                                  File.Contents(filePath));
                defaultSave();
                return (T) xStream.FromXml(File.Contents(filePath).Trim());
            }
        }
    }

    public delegate string LoadFile();
}