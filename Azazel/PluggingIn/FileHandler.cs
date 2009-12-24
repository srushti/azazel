using System.Collections.Generic;
using System.IO;
using Azazel.FileSystem;
using File=Azazel.FileSystem.File;

namespace Azazel.PluggingIn {
    public abstract class FileHandler : LaunchableHandler {
        public abstract bool IsAvailable { get; }

        public bool Handles(Launchable launchable) {
            return launchable.GetType() == typeof (File);
        }

        public IEnumerable<Action> ActionsFor(Launchable launchable) {
            return ActionsFor(((File) launchable).FileSystemInfo);
        }

        protected abstract Actions ActionsFor(FileInfo fileInfo);
    }
}