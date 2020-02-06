using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using NUnit.Framework;
using Playnite;
using Playnite.Common;

namespace Playnite.Tests
{
    [TestFixture]
    public class SizesTets
    {
        private static IEnumerable<TestCaseData> GetAspectRatioCases()
        {
            yield return new TestCaseData(new Rectangle(0, 0, 1920, 1080), new AspectRatio(16, 9));
            yield return new TestCaseData(new Rectangle(0, 0, 1280, 720), new AspectRatio(16, 9));
            yield return new TestCaseData(new Rectangle(0, 0, 1920, 1200), new AspectRatio(8, 5));
            yield return new TestCaseData(new Rectangle(0, 0, 1024, 768), new AspectRatio(4, 3));
            yield return new TestCaseData(new Rectangle(0, 0, 3840, 1080), new AspectRatio(32, 9));
            yield return new TestCaseData(new Rectangle(0, 0, 1280, 1024), new AspectRatio(5, 4));
        }

        [Test, TestCaseSource(nameof(GetAspectRatioCases))]
        public void GetAspectRatioTest(Rectangle input, AspectRatio output)
        {
            Assert.AreEqual(output, Sizes.GetAspectRatio(input));
        }

        private static IEnumerable<TestCaseData> GetWidthTestCases()
        {
            yield return new TestCaseData(720, 1280, new AspectRatio(16, 9));
            yield return new TestCaseData(1200, 1920, new AspectRatio(16, 10));
            yield return new TestCaseData(1080, 3840, new AspectRatio(32, 9));
        }

        [Test, TestCaseSource(nameof(GetWidthTestCases))]
        public void GetWidthTest(double height, double width, AspectRatio ratio)
        {
            Assert.AreEqual(width, ratio.GetWidth(height));
        }

        private static IEnumerable<TestCaseData> GetHeightTestCases()
        {
            yield return new TestCaseData(1280, 720, new AspectRatio(16, 9));
            yield return new TestCaseData(1920, 1200, new AspectRatio(16, 10));
            yield return new TestCaseData(3840, 1080, new AspectRatio(32, 9));
        }

        [Test, TestCaseSource(nameof(GetHeightTestCases))]
        public void GetHeightTest(int width, int height, AspectRatio ratio)
        {
            Assert.AreEqual(height, ratio.GetHeight(width));
        }

        [Test]
        public void GetMegapixelsFromResTest()
        {
            Assert.AreEqual(0.922, Sizes.GetMegapixelsFromRes(1280, 720));
            Assert.AreEqual(0.540, Sizes.GetMegapixelsFromRes(600, 900));
        }

        [Test]
        public void AspectRatioEquatableTest()
        {
            var asp1 = new AspectRatio(1, 2);
            var asp2 = new AspectRatio(1, 2);
            var asp3 = new AspectRatio(1, 1);

            Assert.IsTrue(asp1 == asp2);
            Assert.IsFalse(asp1 == asp3);
            Assert.AreEqual(asp1, asp2);
            Assert.AreNotEqual(asp1, asp3);
        }
    }
}
