using System;
using System.Diagnostics;
using Azazel.Logging;

namespace Azazel.FileSystem {
    public class Runner {
        private readonly VoidDelegate voidDelegate;
        private readonly ProcessStartInfo startInfo;

        public Runner(ProcessStartInfo startInfo) {
            this.startInfo = startInfo;
            startInfo.UseShellExecute = true;
        }

        public Runner(VoidDelegate voidDelegate) {
            this.voidDelegate = voidDelegate;
        }

        public Runner(string fileName, string arguments) : this(new ProcessStartInfo(fileName, arguments)) {}

        public static Runner ExplorerRunner(Folder folder) {
            return new Runner("explorer", folder.FullName);
        }

        public void AsyncStart(VoidDelegate callback) {
            if (voidDelegate == null) {
                startInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                new VoidDelegate(delegate {
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
                    voidDelegate();
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