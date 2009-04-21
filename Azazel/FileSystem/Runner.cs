using System;
using System.Diagnostics;
using Azazel.Logging;

namespace Azazel.FileSystem {
    public class Runner {
        private readonly VoidDelegate voidDelegate;
        private readonly ProcessStartInfo startInfo;

        public Runner(ProcessStartInfo startInfo) : this(startInfo, true) {
        }

        public Runner(ProcessStartInfo startInfo, bool useShellExecute) {
            this.startInfo = startInfo;
            startInfo.UseShellExecute = useShellExecute;
        }

        public Runner(VoidDelegate voidDelegate) {
            this.voidDelegate = voidDelegate;
        }

        public Runner(string fileName, string arguments) : this(new ProcessStartInfo(fileName, arguments)) {}

        public static Runner ExplorerRunner(Folder folder) {
            return new Runner("explorer", folder.FullName);
        }

        public void AsyncStart() {
            if (voidDelegate == null) {
                startInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                new VoidDelegate(delegate {
                                     try {
                                         Process.Start(startInfo);
                                     }
                                     catch (Exception e) {
                                         LogManager.WriteLog(e);
                                     }
                                 }).BeginInvoke(null, null);
            }
            else {
                try {
                    voidDelegate();
                }
                catch (Exception e) {
                    LogManager.WriteLog(e);
                }
            }
        }
    }
}