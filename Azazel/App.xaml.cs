using System.Diagnostics;
using System.Windows;
using Azazel.FileSystem;
using Azazel.Logging;

namespace Azazel {
    public partial class App {
        protected override void OnStartup(StartupEventArgs e) {
            Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            if (processes.Length != 1) {
                Shutdown();
                return;
            }
            var command = new MainWindowCommand(Shutdown);
            command.Execute();
            command.Collapse();
            DispatcherUnhandledException += (sender, args) => {
                                                LogManager.WriteLog(args.Exception);
                                                File.WriteAllText(Paths.Instance.Exception, args.Exception.ToString());
                                            };
            Exit += (sender, args) => LogManager.WriteLog("exiting");
        }
    }
}