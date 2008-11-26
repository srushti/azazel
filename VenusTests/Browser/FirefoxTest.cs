using System.Collections.Generic;
using Azazel.FileSystem;
using NUnit.Framework;

namespace Venus.Browser {
    [TestFixture]
    public class FirefoxTest {
        [Test]
        public void RegularExpression() {
            var files = new Firefox().Launchables();
            AssertContainsLaunchable(files, "wp", "http://en.wikipedia.org/wiki/Special:Search?search=%s");
            AssertContainsLaunchable(files, "Wikipedia", "http://en.wikipedia.org/wiki/Special:Search?search=%s");
        }

        private static void AssertContainsLaunchable(IEnumerable<Launchable> launchables, string name, string url) {
            var count = 0;
            foreach (Bookmark launchable in launchables) {
                if (launchable.Name.Equals(name)) {
                    Assert.AreEqual(url, launchable.FullName);
                    ++count;
                }
            }
            Assert.AreEqual(2, count, "Didn't find " + name);
        }
    }
}