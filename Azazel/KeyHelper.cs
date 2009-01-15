using System.Windows.Input;

namespace Azazel {
    public static class KeyHelper {
        public static bool IsCharOrDigit(this Key keyToCompare, Modifiers modifiers) {
            return IsLetter(keyToCompare) || IsDigit(keyToCompare, modifiers);
        }

        private static bool IsDigit(this Key keyToCompare, Modifiers modifiers) {
            return (keyToCompare >= Key.NumPad0 && keyToCompare <= Key.NumPad9) || (keyToCompare >= Key.D0 && keyToCompare <= Key.D9 && !modifiers.HasShift);
        }

        private static bool IsLetter(this Key keyToCompare) {
            return (keyToCompare >= Key.A && keyToCompare <= Key.Z);
        }

        public static string ActualChar(this Key key, Modifiers modifiers) {
            string keyString = key.ToString();
            if (IsLetter(key)) return keyString;
            if (IsDigit(key, modifiers)) return keyString.Substring(keyString.Length - 1, 1);
            return "";
        }

        public static bool IsBackspace(this Key key) {
            return key == Key.Back;
        }
    }
}