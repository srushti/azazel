using Azazel.FileSystem;
using Azazel.PluggingIn;
using NUnit.Framework;

namespace Azazel {
    [TestFixture]
    public class AppFinderTest {
        private AppFinder finder;

        [SetUp]
        public void SetUp() {
            finder = new AppFinder(new LaunchablePlugins(), new CharacterPlugins());
        }

        [Test]
        public void FindsApp() {
            Assert.AreEqual(Paths.Combine(Constants.QuickLaunch, "Shows Desktop.lnk"), ((File) finder.FindFiles("Show Desktop")[0]).FullName);
        }

        [Test]
        public void DoesntCrashOnSpecialCharacter() {
            Assert.AreNotEqual(0, finder.FindFiles("+").Count);
        }
    }
}