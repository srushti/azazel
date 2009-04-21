using System;
using System.Collections.Generic;
using Azazel.FileSystem;
using Action=Azazel.FileSystem.Action;

namespace Azazel.PluggingIn {
    public interface LaunchableHandler : Plugin {
        bool Handles(Launchable launchable);
        IEnumerable<Action> ActionsFor(Launchable launchable);
    }
}