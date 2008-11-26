using System.Diagnostics;
using System.Windows;
using Azazel.FileSystem;

namespace Azazel {
    public partial class App {
        protected override void OnStartup(StartupEventArgs e) {
            var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            if (processes.Length != 1) Shutdown();
            var command = new MainWindowCommand(Shutdown);
            command.Execute();
            command.Collapse();
            DispatcherUnhandledException += (sender, args) => File.WriteAllText(Paths.Instance.Exception, args.Exception.ToString());
        }
    }
}