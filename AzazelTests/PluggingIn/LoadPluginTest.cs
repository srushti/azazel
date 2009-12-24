using System.Configuration;
using NUnit.Framework;

namespace Azazel.PluggingIn {
    [TestFixture]
    public class LoadPluginTest {
        [SetUp]
        public void SetUp() {
            ConfigurationManager.AppSettings[""] = "";
        }

        [Test]
        public void LoadsPluginsDynamically() {
            Assert.AreNotEqual(null, new PluginLoader(new SelfPlugin()).LaunchablePlugins);
        }
    }
}