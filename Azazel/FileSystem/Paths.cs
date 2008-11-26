using System;
using System.IO;

namespace Azazel.FileSystem {
    public class Paths {
        public static Paths Instance = new Paths();
        private Paths() {}

        public string Folders {
            get { return AppDataFile("folders.xml"); }
        }

        public string History {
            get { return AppDataFile("history.xml"); }
        }

        public string ExecutedCommands {
            get { return AppDataFile("executedCommands.xml"); }
        }

        public string Exception {
            get { return AppDataFile("exception.txt"); }
        }

        private static string AppDataFile(string fileName) {
            return Combine(Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Azazel", fileName));
        }

        public static string Combine(params string[] paths) {
            string result = "";
            for (int i = 0; i < paths.Length; i++) result = Path.Combine(result, paths[i]);
            return result;
        }
    }
}