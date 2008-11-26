using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Venus.Calculate {
    [TestFixture]
    public class CalculatePluginTest {
        [Test]
        public void PerformsAddition() {
            AssertCalculation("10 + 20", 30);
        }

        [Test]
        public void PerformsSubtraction() {
            AssertCalculation("20 - 10", 10);
        }

        [Test]
        public void PerformsMultiplication() {
            AssertCalculation(".99 * 10", 9.9);
        }

        [Test]
        public void PerformsDivision() {
            AssertCalculation("13 / 2", 6.5);
        }

        [Test]
        public void WorksWithBrackets() {
            AssertCalculation("(2+ 3)/2", 2.5);
        }

        [Test]
        public void HandlesUnaryNegative() {
            AssertCalculation("-50 +3.5", -46.5);
        }

        [Test]
        public void RandomStringsDontWork() {
            Assert.AreEqual(false, new CalculatePlugin().IsValidFor("mail."));
        }

        [Test]
        public void WrongEquationReturnsEmpty() {
            Assert.AreEqual(false, new CalculatePlugin().IsValidFor("4+4("));
        }

        private static void AssertCalculation(string equation, double result) {
            Assert.That(new CalculatePlugin().IsValidFor(equation));
            var launchable = new CalculatePlugin().Launchable(equation);
            Assert.That(double.Parse(launchable.Name), Is.EqualTo(result));
        }
    }
}
