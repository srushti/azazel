using System.Collections.Generic;
using Azazel.FileSystem;

namespace Azazel.PluggingIn {
    public interface CharacterPlugin : Plugin {
        bool IsValidFor(string searchString);
        Launchable Launchable(string searchString);
    }

    public class CharacterPlugins : List<CharacterPlugin> {}
}