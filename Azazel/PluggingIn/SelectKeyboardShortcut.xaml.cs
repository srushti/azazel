using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Azazel.KeyHookup;

namespace Azazel.PluggingIn {
    public partial class SelectKeyboardShortcut {
        public SelectKeyboardShortcut() {
            InitializeComponent();
            HookEvents();
        }

        private void HookEvents() {
            Activated += delegate { new WindowHider(this).Hide(); };
            KeyUp += ((sender, e) => KeyPressed(e.Key, Modifiers.Current));
        }

        private void KeyPressed(Key key, Modifiers modifiers) {
            if (key == Key.Enter) SaveAsKeyboardShortcut();
            else if (key == Key.Escape && modifiers.IsNone) Close();
            else {
                var keyboardCombo = new StringBuilder();
                if (modifiers.HasCtrl) keyboardCombo.Append("Ctrl + ");
                if (modifiers.HasAlt) keyboardCombo.Append("Alt + ");
                if (modifiers.HasShift) keyboardCombo.Append("Shift + ");
                if (modifiers.HasWin) keyboardCombo.Append("Win + ");
                keyboardCombo.Append(key);
                if (
                    !new List<Key> {Key.LeftAlt, Key.LeftCtrl, Key.LeftShift, Key.RightAlt, Key.RightCtrl, Key.RightShift, Key.LWin, Key.RWin, Key.System}.
                         Contains(key))
                    currentlySelected.Text = keyboardCombo.ToString();
                var newHotkey = new Hotkey(modifiers, (Keys) Enum. Parse(typeof (Keys), key.ToString()));
            }
        }

        private void SaveAsKeyboardShortcut() {
//            PersistanceHelper.SaveOrLoadAndSave<Hotkey>()
        }
    }
}