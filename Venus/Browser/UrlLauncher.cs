using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Venus.Browser {
    public static class UrlLauncher {
        public static ProcessStartInfo CommandToExecute(string url, string arguments) {
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command", false);
            var browserCommand = ((string) registryKey.GetValue(null, null));
            Match match = Regex.Match(browserCommand, "(\".*\") (.*)$");
            string urlToLaunch = string.IsNullOrEmpty(arguments) ? url : url.Replace("%s", Uri.EscapeDataString(arguments));
            return new ProcessStartInfo(match.Groups[1].Value, match.Groups[2].Value.Replace("%1", urlToLaunch)) {UseShellExecute = true};
        }
    }
}