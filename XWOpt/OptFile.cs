using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace SchmooTech.XWOpt
{
    public class OptFile : BinaryReader, IDisposable
    {
        public int globalOffset; // The number that is subtracted from the file's internal pointers to get the actual file position.
        public int version;

        public Action<string> logger;

        public List<OptNode.BaseNode> rootNodes;

        public OptFile(string fileName, Action<string> logger = null) : base(File.OpenRead(fileName))
        {
            if (null != logger)
            {
                this.logger = logger;
            }

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

        /// <summary>
        /// Save OPT data to file.
        /// </summary>
        /// <param name="fileName">Name of file</param>
        public void SaveAs(string fileName)
        {
            throw new NotImplementedException();
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
                logger(String.Format("Warning: skipping unexpected {0} bytes at {X:1}", RealOffset(offset) - BaseStream.Position, BaseStream.Position));
            }

            // Always 2 in TIE98
            Debug.Assert(ReadInt16() == 2);


        }

        /// <summary>Set stream position accounting for global offset mechanism</summary>
        internal void Seek(int offset)
        {
            base.BaseStream.Seek(RealOffset(offset), SeekOrigin.Begin);
        }
    }
}
