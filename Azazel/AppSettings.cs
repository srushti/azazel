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
        public readonly Hotkey DisplayHotKey;

        protected Hotkeys() {}

        public Hotkeys(Hotkey displayHotKey) {
            DisplayHotKey = displayHotKey;
        }
    }
}