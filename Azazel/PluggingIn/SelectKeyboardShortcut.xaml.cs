using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

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
            else if (key != Key.Escape) {
                var keyboardCombo = new StringBuilder();
                if (modifiers.HasCtrl) keyboardCombo.Append("Ctrl + ");
                if (modifiers.HasAlt) keyboardCombo.Append("Alt + ");
                if (modifiers.HasShift) keyboardCombo.Append("Shift + ");
                if (modifiers.HasWin) keyboardCombo.Append("Ctrl + Win");
                keyboardCombo.Append(key);
                if (!new List<Key> {Key.LeftAlt, Key.LeftCtrl, Key.LeftShift, Key.RightAlt, Key.RightCtrl, Key.RightShift, Key.LWin, Key.RWin}.Contains(key))
                    currentlySelected.Text = keyboardCombo.ToString();
            }
            else Close();
        }

        private void SaveAsKeyboardShortcut() {}
    }
}