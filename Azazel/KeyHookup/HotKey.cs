using System;
using System.Text;
using System.Windows.Forms;

namespace Azazel.KeyHookup {
    [Serializable]
    public class Hotkey {
        internal int id;
        public readonly Keys key;
        public readonly Modifiers modifiers;

        public Hotkey(Modifiers modifiers, Keys key) {
            this.modifiers = modifiers;
            this.key = key;
            id = -1;
        }

        public override string ToString() {
            var hotKeyString = new StringBuilder();
            if (modifiers.HasWin)
                hotKeyString.Append("WIN + ");
            if (modifiers.HasCtrl)
                hotKeyString.Append("CTRL + ");
            if (modifiers.HasAlt)
                hotKeyString.Append("ALT + ");
            if (modifiers.HasShift)
                hotKeyString.Append("SHIFT + ");
            return hotKeyString.Append(key).ToString();
        }

        public bool Equals(Hotkey hotkey) {
            return Equals(key, hotkey.key) && Equals(modifiers, hotkey.modifiers);
        }

        public override bool Equals(object obj) {
            if (!(obj is Hotkey)) return false;
            return Equals((Hotkey) obj);
        }

        public override int GetHashCode() {
            return key.GetHashCode() + 29*modifiers.GetHashCode();
        }
    }
}