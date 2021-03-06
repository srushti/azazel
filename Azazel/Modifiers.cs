using System.Windows.Input;
using Azazel.Extensions;

namespace Azazel {
    public class Modifiers {
        private readonly ModifierKeys modifierKeys;
        public static readonly Modifiers Windows = new Modifiers(ModifierKeys.Windows);
        public static readonly Modifiers Alt = new Modifiers(ModifierKeys.Alt);
        public static readonly Modifiers Shift = new Modifiers(ModifierKeys.Shift);
        public static readonly Modifiers Control = new Modifiers(ModifierKeys.Control);

        public Modifiers(ModifierKeys modifierKeys) {
            this.modifierKeys = modifierKeys;
        }

        public Modifiers(params Modifiers[] modifiers) {
            if (!modifiers.IsEmpty())
                foreach (var modifier in modifiers) modifierKeys |= modifier.modifierKeys;
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

        public ModifierKeys ModifierKeys {
            get { return modifierKeys; }
        }

        private bool HasModifier(ModifierKeys modifier) {
            return (modifierKeys & modifier) == modifier;
        }

        public static Modifiers operator |(Modifiers first, Modifiers second) {
            return new Modifiers(first.modifierKeys | second.modifierKeys);
        }

        public override string ToString() {
            return modifierKeys.ToString();
        }
    }
}