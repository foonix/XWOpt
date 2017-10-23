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
using System.Collections.Generic;
using SchmooTech.XWOpt.OptNode;

namespace SchmooTech.XWOpt
{
    public class OptFile<TVector2, TVector3> : List<BaseNode>
    {
        // The number that is subtracted from the file's internal pointers to get the actual file position.
        public int globalOffset = 0xFF;
        public int version = 0;
        public short unknownWord = 0;

        public Action<string> logger;

        public OptFile()
        {
        }

        public void Read(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var reader = new OptReader(stream, logger);
                reader.Vector2T = typeof(TVector2);
                reader.Vector3T = typeof(TVector3);

                // Version is stored as negative int.
                reader.version = version = -reader.ReadInt32();

                // Sanity check file size.
                int size = reader.ReadInt32() + 8;
                if (size != reader.BaseStream.Length)
                {
                    logger(String.Format("File length expected is {0} but actual lenght is {1}.  File may be corrupt.", size, reader.BaseStream.Length));
                    throw new InvalidDataException();
                }

                // The bytes preceding this don't count when calculating the offset.
                reader.globalOffset = globalOffset = reader.ReadInt32() - 8;

                // Usually 2 in TIE98
                unknownWord = reader.ReadInt16();

                var partCount = reader.ReadInt32();
                var partListOffset = reader.ReadInt32();
                AddRange(reader.ReadChildren(partCount, partListOffset, this));
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

        public List<T> FindAll<T>()
            where T : BaseNode
        {
            var found = new List<T>();
            foreach (BaseNode child in this)
            {
                if(child.GetType() == typeof(T))
                {
                    found.Add((T)child);
                }
                found.AddRange(child.FindAll<T>());
            }
            return found;
        }
    }
}
