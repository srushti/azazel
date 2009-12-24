using Azazel.PluggingIn;

namespace Azazel {
    internal class KeyboardShortcutChangeCommand {
        private readonly AppSettings settings;

        public KeyboardShortcutChangeCommand(AppSettings settings) {
            this.settings = settings;
        }

        public Hotkeys Execute() {
            var hotkeys = new Hotkeys(null);
            var window = new KeyboardShortcut(hotkeys);
            window.ShowDialog();
            return hotkeys;
        }
    }
}