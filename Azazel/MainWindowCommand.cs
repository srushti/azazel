using System.Windows;
using System.Windows.Forms;
using Azazel.FileSystem;
using Azazel.KeyHookup;
using Azazel.PluggingIn;

namespace Azazel {
    public class MainWindowCommand {
        private readonly VoidDelegate killApplication;
        private readonly Hotkey displayHotkey;
        private readonly Hotkey killHotkey;
        private WPFHotkeyManager hotkeyManager;
        private readonly MainWindowController controller;
        private MainWindow window;
        private readonly Hotkey unchangeableDisplayHotkey = new Hotkey(Modifiers.Alt | Modifiers.Control | Modifiers.Shift, Keys.Space);

        public MainWindowCommand(VoidDelegate killApplication) {
            var selfPlugin = new SelfPlugin();
            var loader = new PluginLoader(selfPlugin);
            AppSettings appSettings = PersistanceHelper.LoadOrSaveAndLoad(selfPlugin.XStream, Paths.Instance.AppSettings,
                                                                          new AppSettings(new Hotkey(Modifiers.Alt, Keys.F2),
                                                                                          new Hotkey(Modifiers.Alt | Modifiers.Control | Modifiers.Shift,
                                                                                                     Keys.F4)));
            displayHotkey = appSettings.DisplayHotKey;
            killHotkey = appSettings.KillHotKey;
            controller = new MainWindowController(loader.LaunchablePlugins, loader.CharacterPlugins, loader.LaunchableHandlers, selfPlugin);
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
            hotkeyManager.Register(unchangeableDisplayHotkey);
            hotkeyManager.HotkeyPressed += (window1, hotkey) => HandleHotkey(hotkey);
        }

        private void HandleHotkey(Hotkey hotkey) {
            if (window.Visibility != Visibility.Visible) {
                window.Close();
                hotkeyManager.Unregister(displayHotkey);
                hotkeyManager.Unregister(killHotkey);
                hotkeyManager.Unregister(unchangeableDisplayHotkey);
                if (hotkey.Equals(displayHotkey)) Execute();
                else if (hotkey.Equals(killHotkey)) killApplication();
            }
            if (hotkey.Equals(displayHotkey) || hotkey.Equals(unchangeableDisplayHotkey)) window.Activate();
        }
    }

    public delegate void VoidDelegate();
}