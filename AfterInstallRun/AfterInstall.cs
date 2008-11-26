using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;

namespace AfterInstallRun {
    [RunInstaller(true)]
    public partial class AfterInstall : Installer {
        public AfterInstall() {
            InitializeComponent();
            AfterInstall += delegate { RunApp(); };
        }

        private void RunApp() {
            Process.Start(Path.Combine(Context.Parameters["TargetDir"], "Azazel.exe"));
        }
    }
}