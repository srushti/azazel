using System;
using System.Configuration;
using Microsoft.Win32;
using NUnit.Framework;

namespace Azazel.PluggingIn {
    [TestFixture]
    public class LoadPluginTest {
        [SetUp]
        public void SetUp() {
            ConfigurationManager.AppSettings[""] = "";
        }

        [Test]
        public void MethodName() {
            var registryKey = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command", false);
            var browserCommand = ((string) registryKey.GetValue(null, null));
            Console.WriteLine(browserCommand);
        }

        [Test]
        public void LoadsPluginsDynamically() {
            Assert.AreNotEqual(null, new PluginLoader().LaunchablePlugins);
        }
    }
}