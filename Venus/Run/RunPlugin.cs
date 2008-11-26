using System.Windows.Media;
using System.Windows.Media.Imaging;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Run {
    public class RunPlugin : CharacterPlugin {
        public bool IsAvailable {
            get { return true; }
        }

        public bool IsValidFor(string searchString) {
            return searchString.StartsWith("!") || searchString.StartsWith("'");
        }

        public Launchable Launchable(string searchString) {
            return new RunLaunchable(searchString.Substring(1));
        }

        public class RunLaunchable : Launchable {
            private readonly string s;

            public RunLaunchable(string s) {
                this.s = s;
            }

            public string Name {
                get { return "Run: " + s; }
            }

            public ImageSource Icon {
                get { return new BitmapImage(); }
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
        }
    }
}