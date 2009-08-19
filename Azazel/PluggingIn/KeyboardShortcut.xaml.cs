using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Azazel.FileSystem;
using Azazel.KeyHookup;

namespace Azazel.PluggingIn {
    public partial class KeyboardShortcut {
        private readonly AppSettings settings;
        private readonly PersistanceHelper persistanceHelper;
        private Hotkey newHotkey;

        public KeyboardShortcut(AppSettings settings, PersistanceHelper persistanceHelper) {
            this.settings = settings;
            this.persistanceHelper = persistanceHelper;
            InitializeComponent();
            HookEvents();
        }

        private void HookEvents() {
            Activated += delegate { new WindowHider(this).Hide(); };
            KeyUp += ((sender, e) => KeyPressed(e.Key, Modifiers.Current));
            useWindowsKey.Checked += delegate { OnUseWindowsCheckedChanged(); };
        }

        private void OnUseWindowsCheckedChanged() {
            KeyPressed((Key) Enum.Parse(typeof (Key), newHotkey.key.ToString()), newHotkey.modifiers);
        }

        private void KeyPressed(Key key, Modifiers modifiers) {
            if (useWindowsKey.IsChecked ?? false) modifiers = new Modifiers(modifiers, Modifiers.Windows);
            if (key == Key.Enter) SaveAsKeyboardShortcut();
            else if (key == Key.Escape) Close();
            else {
                var keyboardCombo = new StringBuilder();
                if (modifiers.HasCtrl) keyboardCombo.Append("Ctrl + ");
                if (modifiers.HasAlt) keyboardCombo.Append("Alt + ");
                if (modifiers.HasShift) keyboardCombo.Append("Shift + ");
                if (useWindowsKey.IsChecked ?? false) keyboardCombo.Append("Win + ");
                keyboardCombo.Append(key);
                if (
                    !new List<Key> {Key.LeftAlt, Key.LeftCtrl, Key.LeftShift, Key.RightAlt, Key.RightCtrl, Key.RightShift, Key.LWin, Key.RWin, Key.System}.
                         Contains(key)) {
                    currentlySelected.Content = keyboardCombo.ToString();
                    newHotkey = new Hotkey(modifiers, (Keys) Enum.Parse(typeof (Keys), key.ToString()));
                }
            }
        }

        private void SaveAsKeyboardShortcut() {
            settings.AppHotkeys = persistanceHelper.SaveAndLoad(Paths.Instance.AppHotkeys, new Hotkeys(newHotkey));
            Close();
        }
    }
}