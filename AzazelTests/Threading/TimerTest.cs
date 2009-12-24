using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Azazel.Threading {
    [TestFixture]
    public class TimerTest {
        private List<VoidDelegate> wrongThreadAssertions;

        [SetUp]
        public void SetUp() {
            wrongThreadAssertions = new List<VoidDelegate>();
        }

        [Test]
        public void RunsAfterHalfASecond() {
            var startTime = new DateTime();
            var autoResetEvent = new AutoResetEvent(false);
            var timer = new Timer<string>(a => {
                                              ThreadAssertion(DateTime.Now.Subtract(startTime), new GreaterThanConstraint(TimeSpan.FromSeconds(.5)));
                                              autoResetEvent.Set();
                                          });
            startTime = DateTime.Now;
            timer.Start(.5, "");
            Assert.AreEqual(true, autoResetEvent.WaitOne(TimeSpan.FromSeconds(10), false));
        }

        [Test]
        public void DoesntRunIfTimerIsStartedAgain() {
            var startTime = new DateTime();
            var autoResetEvent = new AutoResetEvent(false);
            var timer = new Timer<string>(a => {
                                              ThreadAssertion(DateTime.Now.Subtract(startTime), new GreaterThanOrEqualConstraint(TimeSpan.FromSeconds(.9)));
                                              autoResetEvent.Set();
                                          });
            startTime = DateTime.Now;
            timer.Start(.5, "");
            Thread.Sleep(TimeSpan.FromSeconds(.4));
            timer.Start(.5, "");
            Assert.AreEqual(true, autoResetEvent.WaitOne(TimeSpan.FromSeconds(10), false));
        }

        private void ThreadAssertion(object elapsed, Constraint constraint) {
            lock (((ICollection) wrongThreadAssertions).SyncRoot)
                wrongThreadAssertions.Add(() => Assert.That(elapsed, constraint));
        }

        [TearDown]
        public void TearDown() {
            foreach (var wrongThreadAssertion in wrongThreadAssertions) wrongThreadAssertion();
        }
    }
}