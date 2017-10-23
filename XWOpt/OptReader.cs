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
using System.Collections.Generic;
using System.IO;
using SchmooTech.XWOpt.OptNode;
using SchmooTech.XWOpt.OptNode.Types;
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
        private Type tVector3;
        private Type tVector2;
        public Type Vector3T { get => tVector3; set => tVector3 = value; }
        public Type Vector2T { get => tVector2; set => tVector2 = value; }

        internal Action<string> logger;

        /// <summary>
        /// Helper for reading Opt file strcutures.  Readers initial top level headers required for reading the rest of the file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="logger"></param>
        internal OptReader(Stream stream, Action<string> logger) : base(stream)
        {
            this.logger = logger;
        }

        internal List<BaseNode> ReadChildren(object parent)
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
                ReadUnknownUseValue(1, parent);
            }
            else
            {
                ReadUnknownUseValue(0, parent);
            }
            ReadInt32();  // skip reverse jump pointer

            // warn if caller is skipping anything else
            SeekShouldPointHere(offset, parent);
            return ReadChildren(count, offset, parent);
        }

        internal List<BaseNode> ReadChildren(int count, int jumpListOffset, object parent)
        {
            var nodes = new List<BaseNode>();

            for (int i = 0; i < count; i++)
            {
                Seek(jumpListOffset + 4 * i);
                int nextNode = ReadInt32();
                if (nextNode != 0)
                {
                    nodes.Add(ReadNodeAt(nextNode, parent));
                }
            }

            return nodes;
        }

        // Instantiates the correct node type based on IDs found.
        public BaseNode ReadNodeAt(int offset, object parent)
        {
            int preHeaderOffset = 0;

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
            else
            {
                BaseStream.Seek(-4, SeekOrigin.Current);
            }

            // Figure out the type of node and build appropriate object.
            switch (majorId)
            {
                case (int)Major.Generic:
                    switch (minorId)
                    {
                        // TODO: set parents.
                        case (int)GenericMinor.Branch:
                            return new BranchNode(this) { parent = parent } as BaseNode;
                        case (int)GenericMinor.MeshVertex:
                            return MakeGenericNode(typeof(MeshVertices<>), new Type[] { Vector3T });
                        case (int)GenericMinor.TextureVertex:
                            return MakeGenericNode(typeof(VertexUV<>), new Type[] { Vector2T });
                        case (int)GenericMinor.TextureReferenceByName:
                            return new TextureReferenceByName(this) as BaseNode;
                        case (int)GenericMinor.VertexNormal:
                            return MakeGenericNode(typeof(VertexNormals<>), new Type[] { Vector3T });
                        case (int)GenericMinor.Hardpoint:
                            return MakeGenericNode(typeof(Hardpoint<>), new Type[] { Vector3T });
                        case (int)GenericMinor.Transform:
                            return MakeGenericNode(typeof(RotationInfo<>), new Type[] { Vector3T });
                        case (int)GenericMinor.MeshLod:
                            return new MeshLOD(this) as BaseNode;
                        case (int)GenericMinor.FaceList:
                            return MakeGenericNode(typeof(FaceList<>), new Type[] { Vector3T });
                        case (int)GenericMinor.SkinSelector:
                            return new SkinSelector(this) as BaseNode;
                        case (int)GenericMinor.MeshDescriptor:
                            return MakeGenericNode(typeof(PartDescriptor<>), new Type[] { Vector3T });
                        case (int)GenericMinor.EngineGlow:
                            return MakeGenericNode(typeof(EngineGlow<>), new Type[] { Vector3T });
                        default:
                            logger("Found unknown node type " + majorId + " " + minorId + " at " + BaseStream.Position);
                            return new BaseNode(this);
                    }

                case (int)Major.Texture:
                    switch (minorId)
                    {
                        case (int)TextureMinor.Texture:
                            return new Texture(this, preHeaderOffset);
                        case (int)TextureMinor.TextureWithAlpha:
                            return new Texture(this, preHeaderOffset);
                        default:
                            logger("Found unknown node type " + majorId + " " + minorId + " at " + BaseStream.Position);
                            return new Texture(this, preHeaderOffset);
                    }
                default:
                    return new BaseNode(this);
            }
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
                logger(String.Format("Warning: skipping unexpected {0} bytes at {1:X} in a {2}", RealOffset(offset) - BaseStream.Position, BaseStream.Position, caller.ToString()));
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
                logger(String.Format("Warning: Skipping unkown data near {0:X} ({1} bytes) in a {2}", BaseStream.Position, RealOffset(offset) - BaseStream.Position, caller.ToString()));
                Seek(offset);
            }
        }

        /// <summary>
        /// Reads an int for which we don't know what the value is supposed to be used for.
        /// Logs a warning if it does not contain the value we normally see.
        /// </summary>
        /// <param name="expected">The expected value</param>
        internal void ReadUnknownUseValue(int expected, object parent)
        {
            int found = ReadInt32();
            if (found != expected)
            {
                logger(String.Format("Unknown use field normally containing {0:X} contains {1:X} at {2:X} in a {3}", expected, found, BaseStream.Position, parent.ToString()));
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
    }
}

