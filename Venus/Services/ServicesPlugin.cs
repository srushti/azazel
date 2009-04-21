using System.ServiceProcess;
using System.Windows.Media;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Services {
    public class ServicesPlugin : LaunchablePlugin {
        public bool IsAvailable {
            get { return true; }
        }

        public Launchables Launchables() {
            var services = new Launchables();
            foreach (var service in ServiceController.GetServices()) services.Add(new Service(service));
            return services;
        }

        public event PluginChangedDelegate Changed = delegate { };
    }

    public class Service : Launchable {
        private readonly ServiceController serviceController;

        public Service(ServiceController serviceController) {
            this.serviceController = serviceController;
        }

        public string Name {
            get { return serviceController.DisplayName; }
        }

        public ImageSource Icon {
            get { return null; }
        }

        public Actions Actions {
            get {
                var actions = new Actions();
                serviceController.Refresh();
                if (CanStop) actions.Add(new ServiceStop(serviceController));
                if (CanStart) actions.Add(new ServiceStart(serviceController));
                actions.Add(new ServiceStatus(serviceController));
                return actions;
            }
        }

        private bool CanStart {
            get { return serviceController.Status == ServiceControllerStatus.Stopped || serviceController.Status == ServiceControllerStatus.Paused; }
        }

        private bool CanStop {
            get { return serviceController.CanStop && serviceController.Status == ServiceControllerStatus.Running; }
        }

        private class ServiceStatus : Action {
            private readonly ServiceController controller;

            public ServiceStatus(ServiceController controller) {
                this.controller = controller;
            }

            public void Act() {
                //no-op
            }

            public string Name {
                get { return "Status : " + controller.Status; }
            }
        }

        private class ServiceStart : Action {
            private readonly ServiceController controller;

            public ServiceStart(ServiceController controller) {
                this.controller = controller;
            }

            public void Act() {
                new Runner(controller.Start).AsyncStart();
            }

            public string Name {
                get { return "Start Service"; }
            }
        }

        private class ServiceStop : Action {
            private readonly ServiceController controller;

            public ServiceStop(ServiceController controller) {
                this.controller = controller;
            }

            public void Act() {
                new Runner(controller.Stop).AsyncStart();
            }

            public string Name {
                get { return "Stop Service"; }
            }
        }

        public void Launch(string arguments) {}

        public bool ShouldStoreHistory {
            get { return false; }
        }
    }
}