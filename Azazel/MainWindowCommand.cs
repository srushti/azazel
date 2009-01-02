using System.Windows;
using System.Windows.Forms;
using Azazel.KeyHookup;
using Azazel.PluggingIn;

namespace Azazel {
    public class MainWindowCommand {
        private readonly Hotkey displayHotkey = new Hotkey(Modifiers.Windows, Keys.Space);
        private readonly VoidDelegate killApplication;
        private readonly Hotkey killHotkey = new Hotkey(Modifiers.Shift | Modifiers.Alt | Modifiers.Control, Keys.F4);
        private WPFHotkeyManager hotkeyManager;
        private readonly MainWindowController controller;
        private MainWindow window;

        public MainWindowCommand(VoidDelegate killApplication) {
            var loader = new PluginLoader();
            controller = new MainWindowController(loader.LaunchablePlugins, loader.CharacterPlugins);
            this.killApplication = killApplication;
        }

        public void Execute() {
            window = new MainWindow(controller);
            window.Show();
            window.Activate();
            RegisterEvents();
        }

        public void Collapse() {
            window.Collapse();
        }

        private void RegisterEvents() {
            hotkeyManager = new WPFHotkeyManager(window);
            hotkeyManager.Register(displayHotkey);
            hotkeyManager.Register(killHotkey);
            hotkeyManager.HotkeyPressed += (window1, hotkey) => HandleHotkey(hotkey);
        }

        private void HandleHotkey(Hotkey hotkey) {
            if (window.Visibility != Visibility.Visible) {
                window.Close();
                hotkeyManager.Unregister(displayHotkey);
                hotkeyManager.Unregister(killHotkey);
                if (hotkey.Equals(displayHotkey)) Execute();
                else if (hotkey.Equals(killHotkey)) killApplication();
            }
            if (hotkey.Equals(displayHotkey)) window.Activate();
        }
    }

    public delegate void VoidDelegate();
}