using Azazel.KeyHookup;

namespace Azazel {
    public class AppSettings {
        private Hotkeys appHotkeys;
        public HotkeysChangedHandler HotkeysChanged = hotkeys => { };

        public Hotkeys AppHotkeys {
            get { return appHotkeys; }
            set {
                appHotkeys = value;
                HotkeysChanged(appHotkeys);
            }
        }
    }

    public delegate void HotkeysChangedHandler(Hotkeys hotkeys);

    public class Hotkeys {
        public Hotkey DisplayHotKey;

        private Hotkeys() {}

        public Hotkeys(Hotkey displayHotKey) : this() {
            DisplayHotKey = displayHotKey;
        }
    }
}