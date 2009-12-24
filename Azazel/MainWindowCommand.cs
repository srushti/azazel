using System.Windows;
using System.Windows.Forms;
using Azazel.FileSystem;
using Azazel.KeyHookup;
using Azazel.PluggingIn;
using Action=System.Action;

namespace Azazel {
    public class MainWindowCommand {
        private readonly Action killApplication;
        private Hotkey displayHotkey;
        private readonly Hotkey killHotkey = new Hotkey(Modifiers.Alt | Modifiers.Control | Modifiers.Shift, Keys.F4);
        private WPFHotkeyManager hotkeyManager;
        private readonly MainWindowController controller;
        private readonly MainWindow window;
        private readonly Hotkey unchangeableDisplayHotkey = new Hotkey(Modifiers.Alt | Modifiers.Control | Modifiers.Shift, Keys.Space);

        public MainWindowCommand(Action killApplication, AppSettings appSettings) {
            var selfPlugin = new SelfPlugin();
            var loader = new PluginLoader(selfPlugin);
            var persistanceHelper = new PersistanceHelper(selfPlugin.XStream);
            appSettings.AppHotkeys = persistanceHelper.LoadOrSaveAndLoad(Paths.Instance.AppHotkeys,
                                                                         new Hotkeys(new Hotkey(Modifiers.Alt, Keys.F2)));
            Hotkeys hotkeys = appSettings.AppHotkeys;
            displayHotkey = hotkeys.DisplayHotKey;
            controller = new MainWindowController(loader.LaunchablePlugins, loader.CharacterPlugins, loader.LaunchableHandlers, selfPlugin, persistanceHelper,
                                                  appSettings);
            this.killApplication = killApplication;
            appSettings.HotkeysChanged += HandleHotkeysChanged;
            window = new MainWindow(controller);
        }

        private void HandleHotkeysChanged(Hotkeys hotkeys) {
            UnregisterHotkeys();
            displayHotkey = hotkeys.DisplayHotKey;
            RegisterEvents();
        }

        public void Execute() {
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
            hotkeyManager.Register(unchangeableDisplayHotkey);
            hotkeyManager.HotkeyPressed += (window1, hotkey) => HandleHotkey(hotkey);
        }

        private void HandleHotkey(Hotkey hotkey) {
            if (window.Visibility != Visibility.Visible) {
                window.Collapse();
//                UnregisterHotkeys();
                if (hotkey.Equals(displayHotkey)) Execute();
                else if (hotkey.Equals(killHotkey)) killApplication();
            }
            if (hotkey.Equals(displayHotkey) || hotkey.Equals(unchangeableDisplayHotkey)) window.Activate();
        }

        private void UnregisterHotkeys() {
            hotkeyManager.Unregister(displayHotkey);
            hotkeyManager.Unregister(killHotkey);
            hotkeyManager.Unregister(unchangeableDisplayHotkey);
        }
    }
}