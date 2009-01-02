using System.Collections.Generic;
using Azazel.FileSystem;
using NUnit.Framework;

namespace Venus.Browser {
    [TestFixture]
    public class FirefoxTest {
        [Test]
        public void RegularExpression() {
            var files = new Firefox().Launchables();
            var wikipediaUrl = "https://secure.wikimedia.org/wikipedia/en/wiki/Special:Search?go=Go&search=%s";
            AssertContainsLaunchable(files, "wp", wikipediaUrl);
            AssertContainsLaunchable(files, "Wikipedia", wikipediaUrl);
        }

        private static void AssertContainsLaunchable(IEnumerable<Launchable> launchables, string name, string url) {
            foreach (Bookmark launchable in launchables) if (launchable.Name.Equals(name) && launchable.FullName.Equals(url)) return;
            Assert.Fail("Didn't find " + name);
        }
    }
}