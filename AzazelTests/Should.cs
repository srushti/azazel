using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Azazel {
    public static class Should {
        public static void ShouldBe<T>(this T actual, T expected) {
            Assert.That(actual, Is.EqualTo(expected));
        }

        public static void ShouldBeLessThan<T>(this T actual, T expected) where T : IComparable<T> {
            Assert.That(actual.CompareTo(expected), Is.LessThan(0));
        }

        public static void ShouldBeGreaterThan<T>(this T actual, T expected) where T : IComparable<T> {
            Assert.That(actual.CompareTo(expected), Is.GreaterThan(0));
        }

        public static void ShouldNotBe<T>(this T actual, T expected) {
            Assert.That(actual, Is.Not.EqualTo(expected));
        }

        public static void ShouldBeTrue(this bool actual) {
            Assert.That(actual);
        }

        public static void ShouldBeFalse(this bool actual) {
            Assert.That(actual, Is.False);
        }

        public static void ShouldBeNull(this object actual) {
            Assert.That(actual, Is.Null);
        }

        public static void ShouldNotBeNull(this object actual) {
            Assert.That(actual, Is.Not.Null);
        }

        public static void ShouldHaveCount(this ICollection actual, int count) {
            Assert.That(actual, Has.Count(count));
        }

        public static void ShouldContain<T>(this ICollection<T> actual, T expected) {
            Assert.That(actual, Has.Member(expected));
        }
    }
}