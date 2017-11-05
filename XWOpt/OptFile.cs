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

using System;
using System.IO;
using SchmooTech.XWOpt.OptNode;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SchmooTech.XWOpt
{

    public delegate TVector3 CoordinateSystemConverter<TVector3>(TVector3 vector);

    public class OptFile<TVector2, TVector3>
    {
        // The number that is subtracted from the file's internal pointers to get the actual file position
        public int GlobalOffset { get; set; } = 0xFF;
        public int Version { get; set; } = 0;
        public short UnknownWord { get; set; } = 0;
        public Action<string> Logger { get; set; }
        public Collection<BaseNode> RootNodes { get; private set; }
        public CoordinateSystemConverter<TVector3> RotateFromOptSpace { get; set; }
        public CoordinateSystemConverter<TVector3> RotateToOptSpace { get; set; }

        public OptFile()
        {
        }

        public void Read(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var reader = new OptReader(stream, Logger)
                {
                    Vector2T = typeof(TVector2),
                    Vector3T = typeof(TVector3),
                    V2Adapter = new Vector2Adapter<TVector2>(),
                    V3Adapter = new Vector3Adapter<TVector3>()
                    {
                        RotateFromOptSpace = RotateFromOptSpace,
                    }
                };

                // Version is stored as negative int.
                reader.version = Version = -reader.ReadInt32();

                // Sanity check file size.
                int size = reader.ReadInt32() + 8;
                if (size != reader.BaseStream.Length)
                {
                    Logger(String.Format(CultureInfo.CurrentCulture, "File length expected is {0} but actual lenght is {1}.  File may be corrupt.", size, reader.BaseStream.Length));
                    throw new InvalidDataException();
                }

                // The bytes preceding this don't count when calculating the offset.
                reader.globalOffset = GlobalOffset = reader.ReadInt32() - 8;

                // Usually 2 in TIE98
                UnknownWord = reader.ReadInt16();

                var partCount = reader.ReadInt32();
                var partListOffset = reader.ReadInt32();
                RootNodes = new Collection<BaseNode>();
                foreach (var child in reader.ReadChildren(partCount, partListOffset, this))
                {
                    RootNodes.Add(child);
                }
            }
        }

        /// <summary>
        /// Save OPT data to file.
        /// </summary>
        /// <param name="fileName">Name of file</param>
        public void SaveAs(string fileName)
        {
            throw new NotImplementedException();
        }

        public Collection<T> FindAll<T>()
            where T : BaseNode
        {
            var found = new Collection<T>();
            foreach (BaseNode child in RootNodes)
            {
                if (child is T)
                {
                    found.Add((T)child);
                }

                if (child is BranchNode branch)
                {
                    foreach (var node in branch.FindAll<T>())
                    {
                        found.Add(node);
                    }
                }
            }
            return found;
        }
    }
}
