using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Azazel.FileSystem;
using Microsoft.Win32;

namespace Venus.Browser {
    public static class UrlLauncher {
        public static ImageSource BrowserIcon;

        static UrlLauncher() {
            var processStartInfo = CommandToExecute("", "");
            var fileName = processStartInfo.FileName;
            if (fileName[0] == '\"') fileName = fileName.Split('\"')[1];
            BrowserIcon = IconExtractor.Instance.Extract(new File(fileName), IconSize.Large);
        }

        public static ProcessStartInfo CommandToExecute(string url, string arguments) {
            var registryKey = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command", false);
            var browserCommand = ((string) registryKey.GetValue(null, null));
            var match = Regex.Match(browserCommand, "(\".*\") (.*)$");
            var urlToLaunch = string.IsNullOrEmpty(arguments) ? url : url.Replace("%s", Uri.EscapeDataString(arguments));
            return new ProcessStartInfo(match.Groups[1].Value, match.Groups[2].Value.Replace("%1", urlToLaunch)) {UseShellExecute = true};
        }
    }
}