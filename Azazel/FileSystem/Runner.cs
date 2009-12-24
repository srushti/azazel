using System;
using System.Diagnostics;
using Azazel.Logging;

namespace Azazel.FileSystem {
    public class Runner {
        private readonly System.Action action;
        private readonly ProcessStartInfo startInfo;

        public Runner(ProcessStartInfo startInfo) {
            this.startInfo = startInfo;
            startInfo.UseShellExecute = true;
        }

        public Runner(System.Action action) {
            this.action = action;
        }

        public Runner(string fileName, string arguments) : this(new ProcessStartInfo(fileName, arguments)) {}

        public static Runner ExplorerRunner(Folder folder) {
            return new Runner("explorer", folder.FullName);
        }

        public void AsyncStart(System.Action callback) {
            if (action == null) {
                startInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                new System.Action(delegate {
                                     try {
                                         Process.Start(startInfo);
                                         callback();
                                     }
                                     catch (Exception e) {
                                         LogManager.WriteLog(e);
                                     }
                                 }).BeginInvoke(null, null);
            }
            else {
                try {
                    action();
                    callback();
                }
                catch (Exception e) {
                    LogManager.WriteLog(e);
                }
            }
        }

        public void AsyncStart() {
            AsyncStart(() => { });
        }
    }

    public static class StaticRunner {
        public static void AsyncStart(this ProcessStartInfo processStartInfo) {
            new Runner(processStartInfo).AsyncStart();
        }
    }
}