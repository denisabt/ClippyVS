using Microsoft.VisualStudio.TestTools.UnitTesting;
using Recoding.ClippyVSPackage;
using System.Windows.Controls;
using Recoding.ClippyVSPackage.Configurations.Legacy;
using SharedProject1.AssistImpl;

namespace SharedProjectTests2
{
    [TestClass]
    public class AssistantBaseTests
    {
        [TestMethod]
        public void TestInit()
        {
            Canvas panel = new Canvas();
            var subj = new TestableAssistantBase();
            subj.InitAssistant(panel);
        }
    }

    public class TestableAssistantBase : AssistantBase
    {
        public new void InitAssistant(Panel canvas)
        {
            base.InitAssistant(canvas);
        }


    }

    public class TestableRockyGeniusBase : RockyGeniusBase
    {
        public new string AnimationsResourceUri
        {
            get => base.AnimationsResourceUri;
            set => base.AnimationsResourceUri = value;
        }

        public new void InitAssistant(Panel canvas)
        {
            base.InitAssistant(canvas);
        }

        public new LayeredAnimation ParseWeblikeAnimation(WeblikeSingleAnimation animation,
            double timeOffset, int frameIndex)
        {
            return base.ParseWeblikeAnimation(animation, timeOffset, frameIndex);
        }
    }
}
