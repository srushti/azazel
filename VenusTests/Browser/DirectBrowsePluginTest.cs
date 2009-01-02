using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Venus.Browser {
    [TestFixture]
    public class DirectBrowsePluginTest {
        [Test]
        public void DetectsUrl() {
            Assert.That(new DirectBrowsePlugin().IsValidFor("http://gmail.com"));
            Assert.That(new DirectBrowsePlugin().IsValidFor("https://www.thoughtworks.com"));
            Assert.That(new DirectBrowsePlugin().IsValidFor("https://www."));
            Assert.That(new DirectBrowsePlugin().IsValidFor("www.thoug"));
            Assert.That(new DirectBrowsePlugin().IsValidFor("thoughtworks.com"));
            Assert.That(new DirectBrowsePlugin().IsValidFor("thoughtworks.org"));
            Assert.That(new DirectBrowsePlugin().IsValidFor("thoughtworks.org"));
            Assert.That(new DirectBrowsePlugin().IsValidFor("thoughtworks.co.in"));
            Assert.That(new DirectBrowsePlugin().IsValidFor("sssiii"), Is.False);
        }

        [Test]
        public void MethodName() {
            Assert.AreEqual(false, new DirectBrowsePlugin().IsValidFor("mail."));
        }
    }
}