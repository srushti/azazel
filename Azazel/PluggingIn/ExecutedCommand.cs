using System;
using Azazel.FileSystem;
using Action=Azazel.FileSystem.Action;

namespace Azazel.PluggingIn {
    public class ExecutedCommand : Action {
        private readonly Launchable launchable;
        private readonly string arguments = String.Empty;
        private readonly Action action;
        private ExecutedCommand() {}

        public ExecutedCommand(Launchable launchable, string arguments) : this() {
            this.launchable = launchable;
            this.arguments = arguments;
        }

        public ExecutedCommand(Launchable launchable, Action action) : this() {
            this.launchable = launchable;
            this.action = action;
        }

        public void Act() {
            if (action != null) action.Act();
            else launchable.Launch(arguments);
        }

        public string Name {
            get {
                return launchable.Name +
                       (string.IsNullOrEmpty(arguments) && action == null ? "" : " -> " + (!string.IsNullOrEmpty(arguments) ? arguments : action.Name));
            }
        }

        private bool Equals(ExecutedCommand obj) {
            return Equals(obj.launchable, launchable) && Equals(obj.arguments, arguments) && Equals(obj.action, action);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ExecutedCommand)) return false;
            return Equals((ExecutedCommand) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int result = (launchable != null ? launchable.GetHashCode() : 0);
                result = (result*397) ^ (arguments != null ? arguments.GetHashCode() : 0);
                result = (result*397) ^ (action != null ? action.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString() {
            return Name;
        }
    }
}