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

using SchmooTech.XWOpt.OptNode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Collections;

namespace SchmooTech.XWOpt
{
    public delegate TVector3 CoordinateSystemConverter<TVector3>(TVector3 vector);

    public class OptFile<TVector2, TVector3> : IEnumerable<BaseNode>
    {
        /// <summary>
        ///  This number determines where the rendering engine loads the OPT file into memory.
        ///  After reading this number, the rest of the file is copied into the given address.
        ///  To determine the actual location in a file that a pointer points to, we subtract
        ///  GlobalOffset and the lenght of the data before GlobalOffset from the pointer value.
        /// </summary>
        public int GlobalOffset { get; set; } = 0xFF;
        public int Version { get; set; } = 0;
        public short UnknownWord { get; set; } = 0;
        public Action<string> Logger { get; set; }
        public Collection<BaseNode> RootNodes { get; private set; }
        public CoordinateSystemConverter<TVector3> RotateFromOptSpace { get; set; }
        public CoordinateSystemConverter<TVector3> RotateToOptSpace { get; set; }

        int preGlobalOffsetHeaderLength = 8;

        public OptFile()
        {
        }

        public void Read(string fileName)
        {
            FileStream stream = null;
            try
            {
                stream = File.OpenRead(fileName);

                Read(stream);
            }
            catch (FileNotFoundException e)
            {
                Logger("Invalid file name " + fileName);
                throw new FileNotFoundException(e.Message, fileName);
            }
            finally
            {
                if (null != stream)
                {
                    stream.Close();
                }
            }
        }

        public void Read(Stream stream)
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

            // Version is stored as negative int, or is omitted if version is 0.
            var version = reader.ReadInt32();
            int size;
            if (version < 0)
            {
                reader.version = Version = Math.Abs(version);
                size = reader.ReadInt32() + preGlobalOffsetHeaderLength;
            }
            else
            {
                size = version;
                version = 0;
            }

            // Sanity check file size.
            if (size != reader.BaseStream.Length)
            {
                var msg = String.Format(CultureInfo.CurrentCulture, "File length expected is {0} but actual length is {1}.  File may be corrupt.", size, reader.BaseStream.Length);
                Logger(msg);
                throw new InvalidDataException(msg);
            }

            // The bytes preceding this don't count when calculating the offset.
            preGlobalOffsetHeaderLength = (int)reader.BaseStream.Position;
            reader.globalOffset = GlobalOffset = reader.ReadInt32() - preGlobalOffsetHeaderLength;

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

        /// <summary>
        /// Save OPT data to file.
        /// </summary>
        /// <param name="fileName">Name of file</param>
        public void SaveAs(string fileName)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<BaseNode> GetEnumerator()
        {
            foreach (var node in RootNodes)
            {
                yield return node;

                if (node is NodeCollection branch)
                {
                    foreach (var subNode in branch)
                    {
                        yield return subNode;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
