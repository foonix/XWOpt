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
using SchmooTech.XWOpt.OptNode.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace SchmooTech.XWOpt
{
    /// <summary>
    /// Helper class for navigating Opt's file structure.
    /// </summary>
    internal class OptReader : BinaryReader
    {
        public int globalOffset = 0;
        public int version = 0;

        // Making this a generic type parameter results in a generic parameter explosion where every node type 
        // needs TVector3 and TVector2 type parameters to use the reader wether they use those types or not.
        internal Type Vector2T { get; set; }
        internal Type Vector3T { get; set; }

        internal VectorAdapter V2Adapter { get; set; }
        internal VectorAdapter V3Adapter { get; set; }

        internal Action<string> logger;

        private Dictionary<int, BaseNode> nodeCache = new Dictionary<int, BaseNode>();
        private Dictionary<int, TexturePallet> paletteCache = new Dictionary<int, TexturePallet>();

        /// <summary>
        /// Helper for reading Opt file strcutures.  Readers initial top level headers required for reading the rest of the file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="logger"></param>
        internal OptReader(Stream stream, Action<string> logger) : base(stream)
        {
            this.logger = logger;
        }

        internal List<BaseNode> ReadChildren(int count, int jumpListOffset, object context)
        {
            var nodes = new List<BaseNode>();

            for (int i = 0; i < count; i++)
            {
                Seek(jumpListOffset + 4 * i);
                int nextNode = ReadInt32();
                if (nextNode != 0)
                {
                    nodes.Add(ReadNodeAt(nextNode, context, null));
                }
            }

            return nodes;
        }

        // Instantiates the correct node type based on IDs found.
        public BaseNode ReadNodeAt(int offset, object context, BaseNode parent)
        {
            if (nodeCache.ContainsKey(offset))
            {
                return nodeCache[offset];
            }

            Seek(offset);

            var nodeHeader = new NodeHeader(this, parent);

            // Figure out the type of node and build appropriate object.
            BaseNode node;

            switch(nodeHeader.NodeType)
            {
                case NodeType.Separator:
                    node = new SeparatorNode(this, nodeHeader);
                    break;
                case NodeType.IndexedFaceSet:
                    node = MakeGenericNode(typeof(FaceList<>), new Type[] { Vector3T }, nodeHeader);
                    break;
                case NodeType.VertexPosition:
                    node = MakeGenericNode(typeof(MeshVertices<>), new Type[] { Vector3T }, nodeHeader);
                    break;
                case NodeType.Translation:
                    node = MakeGenericNode(typeof(Translation<>), new Type[] { Vector3T }, nodeHeader);
                    break;
                case NodeType.UseTexture:
                    node = new TextureReferenceByName(this, nodeHeader);
                    break;
                case NodeType.VertexNormal:
                    node = MakeGenericNode(typeof(VertexNormals<>), new Type[] { Vector3T }, nodeHeader);
                    break;
                case NodeType.TextureVertex:
                    node = MakeGenericNode(typeof(VertexUV<>), new Type[] { Vector2T }, nodeHeader);
                    break;
                case NodeType.Texture:
                    node = new Texture(this, nodeHeader);
                    break;
                case NodeType.MeshLod:
                    node = new LodCollection(this, nodeHeader);
                    break;
                case NodeType.Hardpoint:
                    node = MakeGenericNode(typeof(Hardpoint<>), new Type[] { Vector3T }, nodeHeader);
                    break;
                case NodeType.Pivot:
                    node = MakeGenericNode(typeof(RotationInfo<>), new Type[] { Vector3T }, nodeHeader);
                    break;
                case NodeType.CamoSwitch:
                    node = new SkinCollection(this, nodeHeader);
                    break;
                case NodeType.ComponentInfo:
                    node = MakeGenericNode(typeof(PartDescriptor<>), new Type[] { Vector3T }, nodeHeader);
                    break;
                default:
                    logger?.Invoke("Found unknown node type " + nodeHeader.NodeType + " " + nodeHeader.Name + " at " + BaseStream.Position + " context:" + context);
                    node = new BaseNode(this, nodeHeader);
                    break;

            }

            nodeCache[offset] = node;

            return node;
        }

        /// <summary>
        /// Calculate actual file address based on a pointer stored in the file.
        /// </summary>
        /// <param name="offset">Position read from file</param>
        /// <returns>Physical file address</returns>
        internal long RealOffset(int offset)
        {
            return offset - globalOffset;
        }

        /// <summary>
        /// Calculates the pointer address that would be stored in a file based on actual file address.
        /// </summary>
        /// <param name="offset">Real position in file</param>
        /// <returns>Position to be stored.</returns>
        internal long FakeOffset(int offset)
        {
            return offset + globalOffset;
        }

        /// <summary>Follow offset address in file. Warn if seek does not lead to next byte after address's location.</summary>
        /// <remarks>This is useful for detecting unexpected data in the file.</remarks>
        internal void FollowPointerToNextByte(object caller)
        {
            int offset = ReadInt32();
            Seek(offset);
            if (RealOffset(offset) != BaseStream.Position)
            {
                logger?.Invoke(String.Format("Warning: skipping unexpected {0} bytes at {1:X} in a {2}", RealOffset(offset) - BaseStream.Position, BaseStream.Position, caller.ToString()));
            }
        }

        /// <summary>Set stream position accounting for global offset mechanism</summary>
        internal void Seek(int offset)
        {
            BaseStream.Seek(RealOffset(offset), SeekOrigin.Begin);
        }

        internal void SeekShouldPointHere(int offset, object caller)
        {
            if (RealOffset(offset) != BaseStream.Position)
            {
                logger?.Invoke(String.Format(CultureInfo.CurrentCulture, "Warning: Skipping unkown data near {0:X} ({1} bytes) in a {2}", BaseStream.Position, RealOffset(offset) - BaseStream.Position, caller.ToString()));
                Seek(offset);
            }
        }

        /// <summary>
        /// Reads an int for which we don't know what the value is supposed to be used for.
        /// Logs a warning if it does not contain the value we normally see.
        /// </summary>
        /// <param name="expected">The expected value</param>
        internal void ReadUnknownUseValue(int expected, object context)
        {
            int found = ReadInt32();
            if (found != expected)
            {
                logger?.Invoke(String.Format(CultureInfo.CurrentCulture, "Unknown use field normally containing {0:X} contains {1:X} at {2:X} in a {3}", expected, found, BaseStream.Position, context.ToString()));
            }
        }

        // Create OptNodes with reflection avoids generic parameter explosion.
        BaseNode MakeGenericNode(Type nodeType, Type[] GenericParams, NodeHeader nodeHeader)
        {
            var closedGeneric = nodeType.MakeGenericType(GenericParams);

            var ctor = closedGeneric.GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, System.Reflection.CallingConventions.HasThis, new Type[] { this.GetType(), typeof(NodeHeader) }, null);
            return ctor.Invoke(new object[] { this, nodeHeader }) as BaseNode;
        }

        internal string ReadString(int maxLen)
        {
            var start = BaseStream.Position;

            // Text could be null terminated or fill the entire length.
            int i = 0;
            while (i < maxLen && ReadByte() > 0)
                i++;

            byte[] buffer = new byte[i];

            BaseStream.Seek(start, SeekOrigin.Begin);
            BaseStream.Read(buffer, 0, i);

            BaseStream.Seek(maxLen - i, SeekOrigin.Current);

            return Encoding.ASCII.GetString(buffer);
        }

        internal TVector ReadVector<TVector>()
        {
            if (typeof(TVector) == Vector2T)
            {
                return (TVector)V2Adapter.Read(this);
            }
            else if (typeof(TVector) == Vector3T)
            {
                return (TVector)V3Adapter.Read(this);
            }
            throw new ArgumentException("Unknown vector type.");
        }

        internal Collection<TVector> ReadVectorCollection<TVector>(int count)
        {
            if (typeof(TVector) == Vector2T)
            {
                return (Collection<TVector>)V2Adapter.ReadCollection(this, count);
            }
            else if (typeof(TVector) == Vector3T)
            {
                return (Collection<TVector>)V3Adapter.ReadCollection(this, count);
            }
            throw new ArgumentException("Unknown vector type.");
        }

        internal TexturePallet ReadPalette(int offset)
        {
            if (paletteCache.ContainsKey(offset))
            {
                return paletteCache[offset];
            }

            Seek(offset);

            var palette = new TexturePallet(this);
            paletteCache[offset] = palette;
            return palette;
        }
    }
}

