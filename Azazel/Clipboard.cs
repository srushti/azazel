namespace Azazel {
    public class Clipboard {
        public static void AddString(string s) {
            System.Windows.Clipboard.SetText(s);
        }
    }
}