using System.Windows.Input;

namespace Azazel {
    public class Modifiers {
        private readonly ModifierKeys modifierKeys;

        public Modifiers(ModifierKeys modifierKeys) {
            this.modifierKeys = modifierKeys;
        }

        public bool HasShift {
            get { return HasModifier(ModifierKeys.Shift); }
        }

        public bool HasCtrl {
            get { return HasModifier(ModifierKeys.Control); }
        }

        public static Modifiers Current {
            get { return new Modifiers(Keyboard.Modifiers); }
        }

        public bool HasWin {
            get { return HasModifier(ModifierKeys.Windows); }
        }

        public bool HasAlt {
            get { return HasModifier(ModifierKeys.Alt); }
        }

        private bool HasModifier(ModifierKeys modifier) {
            return (modifierKeys & modifier) == modifier;
        }
    }
}
