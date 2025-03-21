using System;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedProject1.AssistImpl;
using SharedProject1.Configurations;

namespace SharedProjectTests2
{
    public class TestableRocky : Rocky
    {
        public TestableRocky(Panel canvas) : base(canvas)
        {
        }
        public new void RegisterAnimations()
        {
            base.RegisterAnimations();
        }
    }

    [TestClass]
    public class RockyTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Canvas panel = new Canvas();
            Canvas panel2 = new Canvas();
            var subject = new Genius(panel, panel2);

            Assert.IsTrue(subject.AllAnimationNames.Count > 0);
        }

        [TestMethod]
        public void TestRegisterAnimations_ReturnCorrectAnimCount()
        {
            Canvas panel = new Canvas();
            var subject = new TestableRocky(panel);

            subject.RegisterAnimations();
            Assert.AreEqual(46, subject.AllAnimationNames.Count, "Animation Count is  {0}", subject.AllAnimationNames.Count);
        }
    }
}