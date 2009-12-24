using System.Collections.Generic;
using Azazel.FileSystem;

namespace Azazel.PluggingIn {
    public interface LaunchableHandler : Plugin {
        bool Handles(Launchable launchable);
        IEnumerable<Action> ActionsFor(Launchable launchable);
    }
}