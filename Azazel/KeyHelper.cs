using System.Windows.Input;

namespace Azazel {
    public static class KeyHelper {
        public static bool IsCharOrDigit(Key keyToCompare, Modifiers modifiers) {
            return IsLetter(keyToCompare) || IsDigit(keyToCompare, modifiers);
        }

        private static bool IsDigit(Key keyToCompare, Modifiers modifiers) {
            return (keyToCompare >= Key.NumPad0 && keyToCompare <= Key.NumPad9) || (keyToCompare >= Key.D0 && keyToCompare <= Key.D9 && !modifiers.HasShift);
        }

        private static bool IsLetter(Key keyToCompare) {
            return (keyToCompare >= Key.A && keyToCompare <= Key.Z);
        }

        public static string ActualChar(Key key, Modifiers modifiers) {
            string keyString = key.ToString();
            if (IsLetter(key)) return keyString;
            if (IsDigit(key, modifiers)) return keyString.Substring(keyString.Length - 1, 1);
            return "";
        }

        public static bool IsBackspace(Key key) {
            return key == Key.Back;
        }

        public static string AllButLastCharacter(string s) {
            if (s.Length == 0) return s;
            return s.Substring(0, s.Length - 1);
        }
    }
}