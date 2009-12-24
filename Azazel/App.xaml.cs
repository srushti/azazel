using System.Diagnostics;
using System.Windows;
using Azazel.FileSystem;
using Azazel.Logging;

namespace Azazel {
    public partial class App {
        protected override void OnStartup(StartupEventArgs e) {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            foreach (var process in processes) {
                if (process.Id != currentProcess.Id) {
                    Shutdown();
                    return;
                }
            }
            var command = new MainWindowCommand(Shutdown, new AppSettings());
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