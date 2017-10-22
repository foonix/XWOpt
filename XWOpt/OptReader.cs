using System;
using System.Collections.Generic;
using System.IO;
using SchmooTech.XWOpt.OptNode;
using SchmooTech.XWOpt.OptNode.Types;
using System.Reflection;

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
        // needs Vector3T and Vector2T type parameters to use the reader wether they use those types or not.
        private Type vector3T;
        private Type vector2T;
        public Type Vector3T { get => vector3T; set => vector3T = value; }
        public Type Vector2T { get => vector2T; set => vector2T = value; }

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

        internal List<BaseNode> ReadChildren()
        {
            return ReadChildren(ReadInt32(), ReadInt32());
        }

        internal List<BaseNode> ReadChildren(int count, int jumpListOffset)
        {
            var nodes = new List<BaseNode>();

            for (int i = 0; i < count; i++)
            {
                Seek(jumpListOffset + 4 * i);
                int nextNode = ReadInt32();
                if (nextNode != 0)
                {
                    nodes.Add(ReadNodeAt(nextNode));
                }
            }

            return nodes;
        }

        // Instantiates the correct node type based on IDs found.
        public BaseNode ReadNodeAt(int offset)
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
            if (majorId > globalOffset && minorId == (long)Major.textrue)
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
                case (int)Major.generic:
                    switch (minorId)
                    {
                        case (int)GenericMinor.branch:
                            return new BranchNode(this) as BaseNode;
                        case (int)GenericMinor.meshVertex:
                            return MakeGenericNode(typeof(MeshVerticies<>), new Type[] { Vector3T });
                        case (int)GenericMinor.textureVertex:
                            return MakeGenericNode(typeof(VertexUV<>), new Type[] { Vector2T });
                        case (int)GenericMinor.textureReferenceByName:
                            return new TextureReferenceByName(this) as BaseNode;
                        case (int)GenericMinor.vertexNormal:
                            return MakeGenericNode(typeof(VertexNormals<>), new Type[] { Vector3T });
                        case (int)GenericMinor.hardpoint:
                            return MakeGenericNode(typeof(Hardpoint<>), new Type[] { Vector3T });
                        case (int)GenericMinor.transform:
                            return MakeGenericNode(typeof(Transform<>), new Type[] { Vector3T });
                        case (int)GenericMinor.meshLOD:
                            return new MeshLOD(this) as BaseNode;
                        case (int)GenericMinor.faceList:
                            return MakeGenericNode(typeof(FaceList<>), new Type[] { Vector3T });
                        case (int)GenericMinor.skinSelector:
                            return new SkinSelector(this) as BaseNode;
                        case (int)GenericMinor.meshDescriptor:
                            return MakeGenericNode(typeof(PartDescriptor<>), new Type[] { Vector3T });
                        default:
                            logger("Found unknown node type " + majorId + " " + minorId + " at " + BaseStream.Position);
                            return new BaseNode(this);
                    }

                case (int)Major.textrue:
                    switch (minorId)
                    {
                        case (int)TextureMinor.texture:
                            return new Texture(this, preHeaderOffset);
                        case (int)TextureMinor.textureWithAlpha:
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
        internal void FollowPointerToNextByte()
        {
            int offset = ReadInt32();
            Seek(offset);
            if (RealOffset(offset) != BaseStream.Position)
            {
                logger(String.Format("Warning: skipping unexpected {0} bytes at {1:X}", RealOffset(offset) - BaseStream.Position, BaseStream.Position));
            }
        }

        /// <summary>Set stream position accounting for global offset mechanism</summary>
        internal void Seek(int offset)
        {
            BaseStream.Seek(RealOffset(offset), SeekOrigin.Begin);
        }

        internal void SeekShouldPointHere(int offset)
        {
            if (RealOffset(offset) != BaseStream.Position)
            {
                logger(String.Format("Warning: Skipping unkown data near {0:X}", BaseStream.Position));
                Seek(offset);
            }
        }

        /// <summary>
        /// Reads an int for which we don't know what the value is supposed to be used for.
        /// Logs a warning if it does not contain the value we normally see.
        /// </summary>
        /// <param name="expected">The expected value</param>
        internal void ReadUnknownUseValue(int expected)
        {
            int found = ReadInt32();
            if (found != expected)
            {
                logger(String.Format("Unknown use field normally containing {0:X} contains {1:X} at {2:X}", found, expected, BaseStream.Position));
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
                var cotrs = closedGeneric.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                var cotr = closedGeneric.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { this.GetType() }, null);
                return cotr.Invoke(new object[] { this }) as BaseNode;
            }
        }
    }
}

