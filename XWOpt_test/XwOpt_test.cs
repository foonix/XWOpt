/*
 * Copyright 2017 Jason McNew
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies
 * or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
 * OR OTHER DEALINGS IN THE SOFTWARE.
 */

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
            Assert.That(hardpoint.location, Is.EqualTo(new Vector3(-17f, -70f, -32f)));

            Assert.That(opt.FindAll<Hardpoint<Vector3>>().Count(), Is.EqualTo(2));
        }

        [Test, TestCaseSource("GetTestCases")]
        public void XWOpt_Read(string fileName)
        {
            opt.Read(fileName);
        }
    }
}