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

using System.IO;

namespace SchmooTech.XWOpt.OptNode
{
    public class FaceList<TVector3> : BaseNode
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        public int[,] vertexRef, edgeRef, UVRef, vertexNormalRef;

        public TVector3[] faceNormals;
        public TVector3[] acrossTop;
        public TVector3[] downSide;

        public int count, edgeCount;

        static Vector3Adapter<TVector3> v3Adapter = new Vector3Adapter<TVector3>();

        internal FaceList(OptReader reader) : base(reader)
        {
            // unknown zeros
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(0, this);

            count = reader.ReadInt32();
            reader.FollowPointerToNextByte(this);
            edgeCount = reader.ReadInt32();

            vertexRef = new int[count, 4];
            edgeRef = new int[count, 4];
            UVRef = new int[count, 4];
            vertexNormalRef = new int[count, 4];

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    vertexRef[i, j] = reader.ReadInt32();
                }

                for (int j = 0; j < 4; j++)
                {
                    edgeRef[i, j] = reader.ReadInt32();
                }

                for (int j = 0; j < 4; j++)
                {
                    UVRef[i, j] = reader.ReadInt32();
                }

                for (int j = 0; j < 4; j++)
                {
                    vertexNormalRef[i, j] = reader.ReadInt32();
                }
            }

            faceNormals = v3Adapter.ReadArray(reader, count);


            // Not sure these are actually useful in unity.
            acrossTop = new TVector3[count];
            downSide = new TVector3[count];
            for (int i = 0; i < count; i++)
            {
                v3Adapter.Read(reader, ref acrossTop[i]);
                try
                {
                    v3Adapter.Read(reader, ref downSide[i]);
                }
                catch (EndOfStreamException e)
                {
                    // Edge case: TIE98 CORVTA.OPT is missing a float at EOF.
                    reader.logger(e.Message);
                }
            }
        }
    }
}
