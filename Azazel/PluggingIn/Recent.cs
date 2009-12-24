using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Azazel.Extensions;
using Azazel.FileSystem;
using xstream;
using xstream.Converters;
using Action=Azazel.FileSystem.Action;

namespace Azazel.PluggingIn {
    public class Recent : Launchable {
        private readonly Actions executedCommands = new Actions();
        private const int MAX_HISTORY_SIZE = 10;

        public string Name {
            get { return "Recent Commands" + (executedCommands.IsEmpty() ? "" : " : " + executedCommands[0].Name); }
        }

        public ImageSource Icon {
            get { return new BitmapImage(); }
        }

        public Actions Actions {
            get { return executedCommands; }
        }

        public void Launch(string arguments) {
            if (!executedCommands.IsEmpty()) executedCommands[0].Act();
        }

        public bool ShouldStoreHistory {
            get { return true; }
        }

        public void AddExecutedCommand(Launchable launchable, Action action) {
            if (typeof (ExecutedCommand).IsAssignableFrom(action.GetType())) AddExecutedCommand((ExecutedCommand) action);
            else AddExecutedCommand(new ExecutedCommand(launchable, action));
        }

        private void AddExecutedCommand(ExecutedCommand executedCommand) {
            if (executedCommands.Count == MAX_HISTORY_SIZE) executedCommands.RemoveAt(MAX_HISTORY_SIZE - 1);
            if (executedCommands.Contains(executedCommand)) executedCommands.Remove(executedCommand);
            executedCommands.Insert(0, executedCommand);
        }

        public void AddExecutedCommand(Launchable launchable, string arguments) {
            if (!typeof (Recent).IsAssignableFrom(launchable.GetType()))
                AddExecutedCommand(new ExecutedCommand(launchable, arguments));
        }

        private static bool Equals(Recent obj) {
            return !ReferenceEquals(null, obj);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Recent)) return false;
            return Equals((Recent) obj);
        }

        public override int GetHashCode() {
            return 0;
        }

        public class RecentConverter : Converter {
            private readonly SelfPlugin selfPlugin;

            public RecentConverter(SelfPlugin selfPlugin) {
                this.selfPlugin = selfPlugin;
            }

            public bool CanConvert(Type type) {
                return typeof (Recent).Equals(type);
            }

            public void ToXml(object value, XStreamWriter writer, MarshallingContext context) {}

            public object FromXml(XStreamReader reader, UnmarshallingContext context) {
                return selfPlugin.Recent;
            }
        }
    }
}