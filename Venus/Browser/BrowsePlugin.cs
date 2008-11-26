using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Browser {
    public class BrowsePlugin : LaunchablePlugin {
        public bool IsAvailable {
            get { return true; }
        }

        public Launchables Launchables() {
            return new Launchables(new Browse());
        }

        public class Browse : Launchable {
            public string Name {
                get { return "Browse"; }
            }

            public ImageSource Icon {
                get { return new BitmapImage(); }
            }

            public Actions Actions {
                get { return new Actions(); }
            }

            public void Launch(string arguments) {
                new Runner(UrlLauncher.CommandToExecute(arguments, "")).AsyncStart();
            }

            public bool ShouldStoreHistory {
                get { return true; }
            }
        }
    }
}