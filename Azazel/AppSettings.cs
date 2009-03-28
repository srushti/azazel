using System.Windows.Forms;
using Azazel.KeyHookup;

namespace Azazel {
    public class AppSettings {
        public readonly Hotkey DisplayHotKey = new Hotkey(Modifiers.Windows, Keys.Space);
        public readonly Hotkey KillHotkey = new Hotkey(Modifiers.Shift | Modifiers.Alt | Modifiers.Control, Keys.F4);
    }
}