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
                if (CanStop) actions.Add(new ServiceStopAction(serviceController));
                if (CanStart) actions.Add(new ServiceStartAction(serviceController));
                actions.Add(new ServiceStatusAction(serviceController));
                return actions;
            }
        }

        private bool CanStart {
            get { return serviceController.Status == ServiceControllerStatus.Stopped || serviceController.Status == ServiceControllerStatus.Paused; }
        }

        private bool CanStop {
            get { return serviceController.CanStop && serviceController.Status == ServiceControllerStatus.Running; }
        }

        private class ServiceStatusAction : Action {
            private readonly ServiceController controller;

            public ServiceStatusAction(ServiceController controller) {
                this.controller = controller;
            }

            public void Act() {
                //no-op
            }

            public string Name {
                get { return "Status : " + controller.Status; }
            }
        }

        private class ServiceStartAction : Action {
            private readonly ServiceController controller;

            public ServiceStartAction(ServiceController controller) {
                this.controller = controller;
            }

            public void Act() {
                new Runner(controller.Start).AsyncStart();
            }

            public string Name {
                get { return "Start Service"; }
            }
        }

        private class ServiceStopAction : Action {
            private readonly ServiceController controller;

            public ServiceStopAction(ServiceController controller) {
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