using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Azazel.FileSystem;
using xstream;
using xstream.Converters;
using Action=Azazel.FileSystem.Action;

namespace Azazel.PluggingIn {
    public class SelfPlugin : LaunchablePlugin {
        public static readonly SelfPlugin INSTANCE = new SelfPlugin();
        private readonly Operations operations = new Operations();
        public readonly Recent Recent = new Recent();
        public static readonly XStream xstream;

        static SelfPlugin() {
            xstream = new XStream();
            xstream.AddConverter(new FoldersConverter());
            xstream.AddConverter(new OperationsConverter());
            xstream.AddConverter(new Recent.RecentConverter());
            xstream.AddConverter(Folder.Converter);
            xstream.AddConverter(File.Converter);
            xstream.Load(new PluginLoader().Assemblies);
        }

        public SelfPlugin() {
            operations.RefreshRequested += (() => RefreshRequested());
            operations.AddFolder += (() => AddAFolder());
            operations.ChangeShortcut += (() => ChangeShortcut());
        }

        public Launchables Launchables() {
            return new Launchables(operations, Recent);
        }

        public bool IsAvailable {
            get { return false; }
        }

        public event VoidDelegate RefreshRequested = delegate { };

        public event VoidDelegate AddAFolder = delegate { };

        public event VoidDelegate ChangeShortcut = delegate { };

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) return true;
            return (obj as SelfPlugin) != null;
        }

        public override int GetHashCode() {
            return 0;
        }

        internal class Operations : Launchable {
            private readonly BaseAction reparseFolders = new BaseAction("Refresh Folders");
            private readonly BaseAction addAFolder = new BaseAction("Add a folder");
            private readonly BaseAction changeShortcut = new BaseAction("Change Keyboard Shortcut");

            public override bool Equals(object obj) {
                return obj as Operations != null;
            }

            public override int GetHashCode() {
                return 0;
            }

            public Operations() {
                reparseFolders.Invoked += (() => RefreshRequested());
                addAFolder.Invoked += (() => AddFolder());
                changeShortcut.Invoked += (() => ChangeShortcut());
            }

            public string Name {
                get { return "Azazel"; }
            }

            public ImageSource Icon {
                get { return new BitmapImage(); }
            }

            public void Launch(string arguments) {}

            public bool ShouldStoreHistory {
                get { return true; }
            }

            public Actions Actions {
                get { return new Actions(reparseFolders, addAFolder, changeShortcut); }
            }

            public event VoidDelegate RefreshRequested = delegate { };
            public event VoidDelegate AddFolder = delegate { };
            public event VoidDelegate ChangeShortcut = delegate { };

            internal class BaseAction : Action {
                private readonly string name;

                public BaseAction(string name) {
                    this.name = name;
                }

                public string Name {
                    get { return name; }
                }

                public void Act() {
                    Invoked();
                }

                public event VoidDelegate Invoked = delegate { };
            }
        }

        public class OperationsConverter : Converter {
            public bool CanConvert(Type type) {
                return typeof (Operations).Equals(type);
            }

            public void ToXml(object value, XStreamWriter writer, MarshallingContext context) {}

            public object FromXml(XStreamReader reader, UnmarshallingContext context) {
                return INSTANCE.operations;
            }
        }
    }
}