using Azazel;
using NUnit.Framework;

namespace Venus.Browser {
    [TestFixture]
    public class DirectBrowsePluginTest {
        [Test]
        public void DetectsUrl() {
            new DirectBrowsePlugin().IsValidFor("http://gmail.com").ShouldBeTrue();
            new DirectBrowsePlugin().IsValidFor("https://www.thoughtworks.com").ShouldBeTrue();
            new DirectBrowsePlugin().IsValidFor("https://www.").ShouldBeTrue();
            new DirectBrowsePlugin().IsValidFor("www.thoug").ShouldBeTrue();
            new DirectBrowsePlugin().IsValidFor("thoughtworks.com").ShouldBeTrue();
            new DirectBrowsePlugin().IsValidFor("thoughtworks.org").ShouldBeTrue();
            new DirectBrowsePlugin().IsValidFor("thoughtworks.org").ShouldBeTrue();
            new DirectBrowsePlugin().IsValidFor("thoughtworks.co.in").ShouldBeTrue();
            new DirectBrowsePlugin().IsValidFor("sssiii").ShouldBeFalse();
            new DirectBrowsePlugin().IsValidFor("internet").ShouldBeFalse();
        }
    }
}