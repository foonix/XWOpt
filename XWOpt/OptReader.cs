using System;
using System.Collections.Generic;
using System.IO;
using SchmooTech.XWOpt.OptNode;
using SchmooTech.XWOpt.OptNode.Types;

namespace SchmooTech.XWOpt
{
    /// <summary>
    /// Helper class for navigating Opt's file structure.
    /// </summary>
    internal class OptReader : BinaryReader
    {
        // OptFile tracks this, but OptReader also needs to know it.
        public int globalOffset = 0;
        public int version = 0;
        Action<string> logger;

        /// <summary>
        /// Helper for reading Opt file strcutures.  Readers initial top level headers required for reading the rest of the file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="logger"></param>
        internal OptReader(Stream stream, Action<string> logger) : base(stream)
        {
            this.logger = logger;
        }

        internal void ReadHeader()
        {
            // Version is stored as negative int.
            version = -ReadInt32();

            // Sanity check file size.
            int size = ReadInt32() + 8;
            if (size != BaseStream.Length)
            {
                logger(String.Format("File length expected is {0} but actual lenght is {1}.  File may be corrupt.", size, BaseStream.Length));
            }

            // The bytes preceding this don't count when calculating the offset.
            globalOffset = ReadInt32() - 8;
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
                            return new MeshVerticies(this) as BaseNode;
                        case (int)GenericMinor.textureVertex:
                            return new VertexUV(this) as BaseNode;
                        case (int)GenericMinor.textureReferenceByName:
                            return new TextureReferenceByName(this) as BaseNode;
                        case (int)GenericMinor.vertexNormal:
                            return new VertexNormals(this) as BaseNode;
                        case (int)GenericMinor.hardpoint:
                            return new Hardpoint(this) as BaseNode;
                        case (int)GenericMinor.transform:
                            return new Transform(this) as BaseNode;
                        case (int)GenericMinor.meshLOD:
                            return new MeshLOD(this) as BaseNode;
                        case (int)GenericMinor.faceList:
                            return new FaceList(this) as BaseNode;
                        case (int)GenericMinor.skinSelector:
                            return new SkinSelector(this) as BaseNode;
                        case (int)GenericMinor.meshDescriptor:
                            return new PartDescriptor(this) as BaseNode;
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
                            return new BaseNode(this);
                        default:
                            logger("Found unknown node type " + majorId + " " + minorId + " at " + BaseStream.Position);
                            return new Texture(this, preHeaderOffset) as BaseNode;
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
    }
}

