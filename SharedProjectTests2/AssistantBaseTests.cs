using Microsoft.VisualStudio.TestTools.UnitTesting;
using Recoding.ClippyVSPackage;
using System.Windows.Controls;

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
            subj.InitAssistant(panel, "");
            
        }
    }

    public class TestableAssistantBase : AssistantBase
    {
        public new void InitAssistant(Panel canvas, string spriteResourceUri)
        {
            //var rmMock = Mock.Of<ResourceManager>(MockBehavior.Strict);
            base.InitAssistant(canvas, "testAssistant", "testAssistantMapFilename");
        }
    }
}
