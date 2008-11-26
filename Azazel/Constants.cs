using System;
using Azazel.FileSystem;

namespace Azazel {
    public static class Constants {
        public static string QuickLaunch = Paths.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                         "Microsoft\\Internet Explorer\\Quick Launch");

        public static string StartMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        public static string AllUsersStartMenu;

        static Constants() {
            string allUsersProfile = Environment.GetEnvironmentVariable("allusersprofile");
            AllUsersStartMenu = Paths.Combine(allUsersProfile, "Start Menu");
            if (Environment.OSVersion.Version.Major == 6) AllUsersStartMenu = Paths.Combine(allUsersProfile, @"Microsoft\Windows\Start Menu");
        }
    }
}