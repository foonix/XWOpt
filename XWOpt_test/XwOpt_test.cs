using SchmooTech.XWOpt;
using NUnit.Framework;

namespace XWOpt_test
{
    [TestFixture]
    public class XWOpt_test
    {
        OptFile opt;
        string fileName = @"C:\GOG Games\Star Wars - TIE Fighter (1998)\IVFILES\TIEFTR.OPT";

        [SetUp]
        protected void SetUp()
        {

        }

        [Test]
        public void XWOpt_Read()
        {
            opt = new OptFile() {
                logger = msg => TestContext.Out.WriteLine(msg),
                Vector3Type = typeof(System.Numerics.Vector3)
            };
            opt.Read(fileName);
        }
    }
}