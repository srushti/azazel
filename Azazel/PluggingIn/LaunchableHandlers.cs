using System.Collections.Generic;
using Azazel.FileSystem;

namespace Azazel.PluggingIn {
    public class LaunchableHandlers : List<LaunchableHandler> {
        public Actions ActionsFor(Launchable launchable) {
            var actions = new Actions();
            foreach (var launchableHandler in this) {
                if (launchableHandler.Handles(launchable))
                    actions.AddRange(launchableHandler.ActionsFor(launchable));
            }
            return actions;
        }
    }
}