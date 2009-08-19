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
            foreach (var service in ServiceController.GetServices()) {
                ImageSource icon = new PluginIconLoader().Png("services");
                services.Add(new Service(service, icon));
            }
            return services;
        }

        public event PluginChangedDelegate Changed = delegate { };
    }

    public class Service : Launchable {
        private readonly ServiceController serviceController;
        private readonly ImageSource icon;

        public Service(ServiceController serviceController, ImageSource icon) {
            this.serviceController = serviceController;
            this.icon = icon;
        }

        public string Name {
            get { return serviceController.DisplayName; }
        }

        public ImageSource Icon {
            get { return icon; }
        }

        public Actions Actions {
            get {
                var actions = new Actions();
                serviceController.Refresh();
                if (CanStop) {
                    actions.Add(new ServiceStopAction(serviceController));
                    actions.Add(new ServiceRestartAction(serviceController));
                }
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

        private class ServiceRestartAction : Action {
            private readonly ServiceController controller;

            public ServiceRestartAction(ServiceController controller) {
                this.controller = controller;
            }

            public void Act() {
                new Runner(controller.Stop).AsyncStart(controller.Start);
            }

            public string Name {
                get { return "Restart Service"; }
            }
        }

        public void Launch(string arguments) {}

        public bool ShouldStoreHistory {
            get { return false; }
        }
    }
}