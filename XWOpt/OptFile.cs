using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

namespace SchmooTech.XWOpt
{
    public class OptFile : List<OptNode.BaseNode>
    {
        // The number that is subtracted from the file's internal pointers to get the actual file position.
        public int globalOffset = 0xFF;
        public int version = 0;

        public Action<string> logger;

        private Type vector3Type;
        internal ConstructorInfo vector3Cotr;
        public Type Vector3Type
        {
            get => vector3Type;
            set
            {
                vector3Type = value;
                vector3Cotr = vector3Type.GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float) });
            }
        }

        public class Vector3
        {
            public float x, y, z;
            public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        }

        public OptFile() {
            Vector3Type = typeof(Vector3);
        }

        public void Read(string fileName)
        {
            using (var reader = new OptReader(File.OpenRead(fileName), this, logger))
            {
                // Version is stored as negative int.
                version = -reader.ReadInt32();

                // Sanity check file size.
                int size = reader.ReadInt32() + 8;
                if (size != reader.BaseStream.Length)
                {
                    logger(String.Format("File length expected is {0} but actual lenght is {1}.  File may be corrupt.", size, reader.BaseStream.Length));
                }

                // The bytes preceding this don't count when calculating the offset.
                globalOffset = reader.ReadInt32() - 8;

                // Always 2 in TIE98
                Debug.Assert(reader.ReadInt16() == 2);

                AddRange(reader.ReadChildren());
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
    }
}
