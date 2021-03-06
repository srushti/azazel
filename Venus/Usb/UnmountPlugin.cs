using System.Windows.Media;
using Azazel.Extensions;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Usb {
    public class UnmountPlugin : LaunchablePlugin {
        public bool IsAvailable {
            get { return true; }
        }

        public Launchables Launchables() {
            return new Launchables {new UnmountLaunchable()};
        }

        public event PluginChangedDelegate Changed = delegate { };
    }

    internal class UnmountLaunchable : Launchable {
        public string Name {
            get { return "Unmount: "; }
        }

        public ImageSource Icon {
            get { return new PluginIconLoader().Icon("usb"); }
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof (UnmountLaunchable);
        }

        public override int GetHashCode() {
            return 0;
        }

        public Actions Actions {
            get {
                var diskDevice = new DiskDeviceClass();
                var devices = diskDevice.Devices.FindAll(device => device.IsUsb);
                if (devices.IsEmpty()) return new Actions {UnmountAction.NoDevices};
                var actions = new Actions();
                foreach (var device in devices) actions.Add(new UnmountAction(device));
                return actions;
            }
        }

        public void Launch(string arguments) {}

        public bool ShouldStoreHistory {
            get { return true; }
        }
    }

    internal class UnmountAction : Action {
        private readonly Device device;
        public static readonly Action NoDevices = new UnmountAction();

        private UnmountAction() {}

        public UnmountAction(Device device) {
            this.device = device;
        }

        public void Act() {
            if (device != null) new Runner(() => device.Eject(true)).AsyncStart();
        }

        public string Name {
            get {
                if (device == null) return "No devices to unmount";
                return device.FriendlyName;
            }
        }
    }
}