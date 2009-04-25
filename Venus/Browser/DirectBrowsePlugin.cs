using System.Text.RegularExpressions;
using System.Windows.Media;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Browser {
    public class DirectBrowsePlugin : CharacterPlugin {
        private static readonly Regex emailRegex = new Regex(@"[a-zA-Z][a-zA-Z0-9_\.]*@[a-zA-Z0-9_\.]+\.[a-zA-Z0-9_\.]+");
        private static readonly Regex startingUrlRegex = new Regex(@"^(https?://)|(www\.)");
        private static readonly Regex endingUrlRegex = new Regex(@"\.(com)|(net)|(org)|(co\.\w\w)");

        public bool IsAvailable {
            get { return true; }
        }

        public bool IsValidFor(string searchString) {
            return IsUrl(searchString) || emailRegex.IsMatch(searchString);
        }

        private static bool IsUrl(string searchString) {
            return startingUrlRegex.IsMatch(searchString) || endingUrlRegex.IsMatch(searchString);
        }

        public Launchable Launchable(string searchString) {
            if (emailRegex.IsMatch(searchString)) return new EmailLaunchable(searchString);
            return new UrlLaunchable(searchString);
        }

        private class EmailLaunchable : DirectBrowseLaunchable {
            public EmailLaunchable(string url) : base(url) {}

            public override string Name {
                get { return "Email: " + url; }
            }

            public override ImageSource Icon {
                get { return new PluginIconLoader().Icon("email"); }
            }
        }

        private class UrlLaunchable : DirectBrowseLaunchable {
            public UrlLaunchable(string url) : base(url) {}

            public override string Name {
                get { return "Open: " + url; }
            }

            public override ImageSource Icon {
                get { return new PluginIconLoader().Png("internet"); }
            }
        }

        public abstract class DirectBrowseLaunchable : Launchable {
            protected readonly string url;

            protected DirectBrowseLaunchable(string url) {
                this.url = url;
            }

            public abstract string Name { get; }
            public abstract ImageSource Icon { get; }

            public Actions Actions {
                get { return new Actions(); }
            }

            public void Launch(string arguments) {
                new Runner(UrlLauncher.CommandToExecute(url, arguments)).AsyncStart();
            }

            public bool ShouldStoreHistory {
                get { return false; }
            }
        }
    }
}