using SchmooTech.XWOpt;
using SchmooTech.XWOpt.OptNode;
using NUnit.Framework;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.IO;

namespace XWOpt_test
{
    [TestFixture]
    public class XWOpt_test
    {
        static string TieFighter = @"C:\GOG Games\Star Wars - TIE Fighter (1998)\IVFILES\TIEFTR.OPT";
        static string[] optDirs = {
            @"C:\GOG Games\Star Wars - XvT\ivfiles",
            @"C:\GOG Games\Star Wars - TIE Fighter (1998)\IVFILES",
            @"C:\GOG Games\Star Wars - X-Wing (1998)\IVFiles",
            @"C:\GOG Games\Star Wars - X-Wing Alliance\FLIGHTMODELS",
        };

        OptFile<Vector2, Vector3> opt;

        [SetUp]
        protected void SetUp()
        {
            opt = new OptFile<Vector2, Vector3>()
            {
                logger = msg => TestContext.Out.WriteLine(msg),
            };
        }

        private static List<string> GetTestCases()
        {
            var files = new List<string>();

            foreach (string dir in optDirs)
            {
                files.AddRange(Directory.GetFiles(dir, "*.OPT"));
            }

            return files;
        }


        [Test]
        public void XWOpt_Read_TieFighter()
        {
            opt.Read(TieFighter);

            var hardpoint = (Hardpoint<Vector3>)opt[0][4];
            Assert.That(hardpoint, Is.InstanceOf(typeof(Hardpoint<Vector3>)));
            Assert.That(hardpoint.coords, Is.EqualTo(new Vector3(-17f, -70f, -32f)));

            Assert.That(opt.FindAll<Hardpoint<Vector3>>().Count(), Is.EqualTo(2));
        }

        [Test, TestCaseSource("GetTestCases")]
        public void XWOpt_Read(string fileName)
        {
            opt.Read(fileName);
        }
    }
}