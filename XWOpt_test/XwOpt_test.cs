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

using NUnit.Framework;
using SchmooTech.XWOpt;
using SchmooTech.XWOpt.OptNode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;

namespace XWOpt_test
{
    [TestFixture]
    public class XWOpt_test
    {
        static readonly string TieFighter = @"C:\GOG Games\Star Wars - TIE Fighter (1998)\IVFILES\TIEFTR.OPT";
        static readonly string[] optDirs = {
            @"C:\GOG Games\Star Wars - XvT\ivfiles",
            @"C:\GOG Games\Star Wars - XvT\BalanceOfPower\IVFILES",
            @"C:\GOG Games\Star Wars - TIE Fighter (1998)\IVFILES",
            @"C:\GOG Games\Star Wars - X-Wing (1998)\IVFiles",
            @"C:\GOG Games\Star Wars - X-Wing Alliance\FLIGHTMODELS",
        };

        OptFile<Vector2, Vector3> opt;


        // XvT engine -> Unity engine
        // unity: forward is +z, right is +x,    up is +y
        // XvT:   forward is -y, right is +x(?), up is +z
        static readonly Matrix4x4 CoordinateConverter = new Matrix4x4(
            1, 0, 0, 0,
            0, 0, -1, 0,
            0, 1, 0, 0,
            0, 0, 0, 1
        );

        [Serializable]
        public struct LogEntry
        {
            public string file;
            public string message;
            public LogEntry(string file, string message) { this.file = file; this.message = message; }
        }

        static readonly List<LogEntry> messages = new List<LogEntry>();

        Vector3 RotateIntoUnitySpace(Vector3 v)
        {
            return Vector3.Transform(v, CoordinateConverter);
        }

        [SetUp]
        protected void SetUp()
        {
            opt = new OptFile<Vector2, Vector3>();
        }

        [TearDown]
        protected void TearDown()
        {
            var serializer = new XmlSerializer(messages.GetType());
            var writer = new StreamWriter(@"c:\temp\opt-test-output.xml");
            serializer.Serialize(writer, messages);
            writer.Close();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static List<string> GetTestCases()
        {
            var files = new List<string>();

            foreach (string dir in optDirs)
            {
                files.AddRange(Directory.GetFiles(dir, "*.OPT"));
                files.AddRange(Directory.GetFiles(dir, "*.OP1"));
            }

            return files;
        }


        [Test]
        public void XWOpt_Read_TieFighter()
        {
            opt.Read(TieFighter);

            Hardpoint<Vector3> hardpoint = opt.OfType<Hardpoint<Vector3>>().First();
            Assert.That(hardpoint, Is.InstanceOf(typeof(Hardpoint<Vector3>)));
            Assert.That(hardpoint.Location, Is.EqualTo(new Vector3(-17f, -70f, -32f)));

            Assert.That(opt.OfType<Hardpoint<Vector3>>().Count(), Is.EqualTo(2));

            var convertedOpt = new OptFile<Vector2, Vector3>()
            {
                RotateFromOptSpace = new CoordinateSystemConverter<Vector3>(RotateIntoUnitySpace),
                Logger = msg => TestContext.Out.WriteLine(msg),
            };

            convertedOpt.Read(TieFighter);
            var rotatedHardpoint = convertedOpt.OfType<Hardpoint<Vector3>>().First();
            Assert.That(rotatedHardpoint, Is.InstanceOf(typeof(Hardpoint<Vector3>)));
            Assert.That(rotatedHardpoint.Location, Is.EqualTo(new Vector3(-17f, -32f, 70f)));
        }

        [Test, TestCaseSource("GetTestCases")]
        public void XWOpt_Read(string fileName)
        {
            opt.Logger = msg =>
            {
                TestContext.Out.WriteLine(msg);
                messages.Add(new LogEntry(fileName, msg));
            };

            opt.Read(fileName);
        }
    }
}