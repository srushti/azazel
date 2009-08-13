using Azazel.KeyHookup;

namespace Azazel {
    public class AppSettings {
        public readonly Hotkey DisplayHotKey;
        public readonly Hotkey KillHotKey;

        public AppSettings() {}

        public AppSettings(Hotkey displayHotKey, Hotkey killHotKey) {
            DisplayHotKey = displayHotKey;
            KillHotKey = killHotKey;
        }
    }
}