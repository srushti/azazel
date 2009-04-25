using System.Windows.Media;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Run {
    public class RunPlugin : CharacterPlugin {
        private readonly ImageSource icon = new PluginIconLoader().Icon("run");

        public bool IsAvailable {
            get { return true; }
        }

        public bool IsValidFor(string searchString) {
            return searchString.StartsWith("!") || searchString.StartsWith("'");
        }

        public Launchable Launchable(string searchString) {
            return new RunLaunchable(searchString.Substring(1), icon);
        }

        private class RunLaunchable : Launchable {
            private readonly string s = string.Empty;
            private readonly ImageSource icon;

            public RunLaunchable(string s, ImageSource icon) {
                this.s = s;
                this.icon = icon;
            }

            public string Name {
                get { return "Run: " + s; }
            }

            public ImageSource Icon {
                get { return icon; }
            }

            public Actions Actions {
                get { return new Actions(); }
            }

            public void Launch(string arguments) {
                new Runner(s, arguments).AsyncStart();
            }

            public bool ShouldStoreHistory {
                get { return false; }
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (RunLaunchable)) return false;
                return Equals(((RunLaunchable) obj).s, s);
            }

            public override int GetHashCode() {
                return s.GetHashCode();
            }
        }
    }
}