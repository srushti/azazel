using System;
using System.Diagnostics;
using System.Windows.Media;
using Azazel.FileSystem;
using Azazel.PluggingIn;
using Action=Azazel.FileSystem.Action;

namespace Venus.ProcessKilller {
    public class ProcessKillerPlugin : LaunchablePlugin {
        public bool IsAvailable {
            get { return true; }
        }

        public Launchables Launchables() {
            return new Launchables(new ProcessKiller());
        }

        public event PluginChangedDelegate Changed = delegate {};


        public class ProcessKiller : Launchable
        {
            public string Name
            {
                get { return "Kill"; }
            }

            public ImageSource Icon
            {
                get { return null; }
            }

            public Actions Actions
            {
                get {
                    var actions = new Actions();
                    foreach (var process in Process.GetProcesses()) {
                        actions.Add(new ProcessAction(process));
                    }
                    return actions;
                }
            }

            public class ProcessAction : Action {
                private readonly Process process;

                public ProcessAction(Process process) {
                    this.process = process;
                }

                public void Act() {
                    process.Kill();
                }

                public string Name {
                    get { return string.Format("{0} ({1})", process.ProcessName, process.Id); }
                }
            }

            public void Launch(string arguments)
            {
                
            }

            public bool ShouldStoreHistory
            {
                get { return false; }
            }
        }

    }

}