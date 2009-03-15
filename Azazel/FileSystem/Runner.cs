using System;
using System.Diagnostics;

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

        public void AsyncStart() {
            if (voidDelegate == null) {
                startInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                new VoidDelegate(() => Process.Start(startInfo)).BeginInvoke(null, null);
            }
            else
                voidDelegate();
        }
    }
}