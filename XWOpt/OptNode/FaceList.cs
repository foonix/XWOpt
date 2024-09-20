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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SchmooTech.XWOpt.OptNode
{
    public class FaceList<TVector3> : BaseNode
    {
        private Collection<CoordinateReferenceTuple> vertexNormalRef;

        private Collection<TVector3> faceNormals;
        private Collection<TextureBasisVectors<TVector3>> basisVectors;

        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        private Collection<CoordinateReferenceTuple> vertexRef;
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        private Collection<CoordinateReferenceTuple> edgeRef;
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        private Collection<CoordinateReferenceTuple> uVRef;

        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        public Collection<CoordinateReferenceTuple> VertexRef { get => vertexRef; }
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        public Collection<CoordinateReferenceTuple> EdgeRef { get => edgeRef; }
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        public Collection<CoordinateReferenceTuple> UVRef { get => uVRef; }
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        public Collection<CoordinateReferenceTuple> VertexNormalRef { get => vertexNormalRef; }
        public Collection<TVector3> FaceNormals { get => faceNormals; }
        public Collection<TextureBasisVectors<TVector3>> BasisVectors { get => basisVectors; }
        public int Count { get; }
        public int EdgeCount { get; }
        public int FaceListSize { get; }

        internal FaceList(OptReader reader, NodeHeader nodeHeader) : base(reader, nodeHeader)
        {
            reader.Seek(nodeHeader.DataAddress);
            Count = nodeHeader.DataCount;

            // read the IFS Size
            FaceListSize = reader.ReadInt32();

            // Next up is the DataCount * Faces, size 64
            vertexRef = new Collection<CoordinateReferenceTuple>();
            edgeRef = new Collection<CoordinateReferenceTuple>();
            uVRef = new Collection<CoordinateReferenceTuple>();
            vertexNormalRef = new Collection<CoordinateReferenceTuple>();

            for (int i = 0; i < nodeHeader.DataCount; i++)
            {
                vertexRef.Add(new CoordinateReferenceTuple(reader));
                edgeRef.Add(new CoordinateReferenceTuple(reader));
                UVRef.Add(new CoordinateReferenceTuple(reader));
                vertexNormalRef.Add(new CoordinateReferenceTuple(reader));
            }

            // Next up is the DataCount * Face Normals, size 12
            faceNormals = reader.ReadVectorCollection<TVector3>(nodeHeader.DataCount);

            // Next up is the DataCount * Texture Basis Vectors, size 24
            basisVectors = new Collection<TextureBasisVectors<TVector3>>();
            for (int i = 0; i < nodeHeader.DataCount; i++)
            {
                // Edge case: TIE98 CORVTA.OPT is missing a float at EOF.
                try
                {
                    BasisVectors.Add(new TextureBasisVectors<TVector3>(reader));
                }
                catch (EndOfStreamException e)
                {
                    reader.logger?.Invoke(e.Message);
                    var bv = new TextureBasisVectors<TVector3>
                    {
                        AcrossTop = (TVector3)reader.V3Adapter.Zero()
                    };
                    bv.AcrossTop = (TVector3)reader.V3Adapter.Zero();
                    BasisVectors.Add(bv);
                }
            }
        }
    }
}
