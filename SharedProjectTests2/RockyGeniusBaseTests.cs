using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Recoding.ClippyVSPackage.Configurations.Legacy;

namespace SharedProjectTests2
{
    [TestClass]
    public class RockyGeniusBaseTests
    {
        [TestMethod]
        public void TestParseWeblikeAnimation()
        {
            // Arrange
            var subj = new TestableRockyGeniusBase();
            subj.AnimationsResourceUri = "";
            var animation = new WeblikeSingleAnimation();
            double timeOffset = 0;
            int frameIndex = 0;
            // Act
            var result = subj.ParseWeblikeAnimation(animation, timeOffset, frameIndex);
            // Assert
            Assert.IsNotNull(result);
        }
    }
}
