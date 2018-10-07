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
using System.Reflection;
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

        internal List<BaseNode> ReadChildren(object context)
        {
            var count = ReadInt32();
            if (0 == count)
            {
                return new List<BaseNode>();
            }

            var offset = ReadInt32();

            // Reverse jump count and offset?
            if (version <= 2)
            {
                ReadUnknownUseValue(1, context);
            }
            else
            {
                ReadUnknownUseValue(0, context);
            }
            ReadInt32();  // skip reverse jump pointer

            // warn if caller is skipping anything else
            SeekShouldPointHere(offset, context);
            return ReadChildren(count, offset, context);
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
                    nodes.Add(ReadNodeAt(nextNode, context));
                }
            }

            return nodes;
        }

        // Instantiates the correct node type based on IDs found.
        public BaseNode ReadNodeAt(int offset, object context)
        {
            int preHeaderOffset = 0;

            if (nodeCache.ContainsKey(offset))
            {
                logger?.Invoke("Recycling node at " + offset + " of type " + nodeCache[offset].ToString());
                return nodeCache[offset];
            }

            Seek(offset);
            int majorId = ReadInt32();
            int minorId = ReadInt32();

            // Edge case: one block type doesn't start with major/minor type id and actually start with another offset.
            // So peek ahead one more long and shuffle numbers where they go.
            // This may not work if globalOffset is 0.
            // Should be a pointer to an offset containing string "Tex00000" or similar.
            int peek = ReadInt32();
            if (majorId > globalOffset && minorId == (long)Major.Texture)
            {
                preHeaderOffset = majorId;
                majorId = minorId;
                minorId = peek;
            }
            else if (majorId > globalOffset && minorId == 0 && peek == 1)
            {
                // This is a weird subtype found in SHUTTLE.OPT
                return new NameNode(this, majorId);
            }
            else
            {
                BaseStream.Seek(-4, SeekOrigin.Current);
            }

            // Figure out the type of node and build appropriate object.
            BaseNode node;
            switch (majorId)
            {
                case (int)Major.Generic:
                    switch (minorId)
                    {
                        case (int)GenericMinor.Branch:
                            node = new NodeCollection(this) as BaseNode;
                            break;
                        case (int)GenericMinor.MeshVertex:
                            node = MakeGenericNode(typeof(MeshVertices<>), new Type[] { Vector3T });
                            break;
                        case (int)GenericMinor.TextureVertex:
                            node = MakeGenericNode(typeof(VertexUV<>), new Type[] { Vector2T });
                            break;
                        case (int)GenericMinor.TextureReferenceByName:
                            node = new TextureReferenceByName(this) as BaseNode;
                            break;
                        case (int)GenericMinor.VertexNormal:
                            node = MakeGenericNode(typeof(VertexNormals<>), new Type[] { Vector3T });
                            break;
                        case (int)GenericMinor.Hardpoint:
                            node = MakeGenericNode(typeof(Hardpoint<>), new Type[] { Vector3T });
                            break;
                        case (int)GenericMinor.Transform:
                            node = MakeGenericNode(typeof(RotationInfo<>), new Type[] { Vector3T });
                            break;
                        case (int)GenericMinor.MeshLod:
                            node = new LodCollection(this) as BaseNode;
                            break;
                        case (int)GenericMinor.FaceList:
                            node = MakeGenericNode(typeof(FaceList<>), new Type[] { Vector3T });
                            break;
                        case (int)GenericMinor.SkinSelector:
                            node = new SkinCollection(this) as BaseNode;
                            break;
                        case (int)GenericMinor.MeshDescriptor:
                            node = MakeGenericNode(typeof(PartDescriptor<>), new Type[] { Vector3T });
                            break;
                        case (int)GenericMinor.EngineGlow:
                            node = MakeGenericNode(typeof(EngineGlow<>), new Type[] { Vector3T });
                            break;
                        default:
                            logger?.Invoke("Found unknown node type " + majorId + " " + minorId + " at " + BaseStream.Position + " context:" + context);
                            node = new BaseNode(this);
                            break;
                    }
                    break;

                case (int)Major.Texture:
                    switch (minorId)
                    {
                        case (int)TextureMinor.Texture:
                            node = new Texture(this, preHeaderOffset);
                            break;
                        case (int)TextureMinor.TextureWithAlpha:
                            node = new Texture(this, preHeaderOffset);
                            break;
                        default:
                            logger?.Invoke("Found unknown node type " + majorId + " " + minorId + " at " + BaseStream.Position + " context:" + context);
                            node = new Texture(this, preHeaderOffset);
                            break;
                    }
                    break;
                default:
                    node = new BaseNode(this);
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

        /// <summary>Follow a pointer read from the file.</summary>
        internal void FollowPointer()
        {
            Seek(ReadInt32());
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
        BaseNode MakeGenericNode(Type nodeType, Type[] GenericParams, object constructorArgs = null)
        {
            var closedGeneric = nodeType.MakeGenericType(GenericParams);

            if (null != constructorArgs)
            {
                return closedGeneric.GetConstructor(new Type[] { this.GetType(), constructorArgs.GetType() }).Invoke(new object[] { this, constructorArgs }) as BaseNode;
            }
            else
            {
                var cotr = closedGeneric.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { this.GetType() }, null);
                return cotr.Invoke(new object[] { this }) as BaseNode;
            }
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

