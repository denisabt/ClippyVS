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

            Assert.IsTrue(subject.AllAnimations.Count > 0);
        }

        [TestMethod]
        public void TestRegisterAnimations_ReturnCorrectAnimCount()
        {
            Canvas panel = new Canvas();
            var subject = new TestableRocky(panel);

            subject.RegisterAnimations();
            Assert.AreEqual(46, subject.AllAnimations.Count, "Animation Count is  {0}", subject.AllAnimations.Count);
        }

        //[TestMethod]
            //public void TestAnimations_ContainAllAnims()
            //{
            //    Canvas panel = new Canvas();
            //    var subject = new TestableRocky(panel);

            //    subject.RegisterAnimations();
            //    bool missing = false;

            //    foreach (var anim in Enum.GetValues(typeof(RockyAnimations)))
            //    {
            //        subject.AllAnimations
            //        var animation = RockyGeniusBase.Ani
            //        if (animation == null)
            //            missing = true;
            //    }

            //    Assert.IsFalse(missing);
            //}
        }
    }