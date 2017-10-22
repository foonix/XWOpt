using System;
using System.IO;
using System.Collections.Generic;
using SchmooTech.XWOpt.OptNode;

namespace SchmooTech.XWOpt
{
    public class OptFile<Vector2T, Vector3T> : List<BaseNode>
    {
        // The number that is subtracted from the file's internal pointers to get the actual file position.
        public int globalOffset = 0xFF;
        public int version = 0;
        public short unknownWord = 0;

        public Action<string> logger;

        public OptFile()
        {
        }

        public void Read(string fileName)
        {
            using (var reader = new OptReader(File.OpenRead(fileName), logger))
            {
                reader.Vector2T = typeof(Vector2T);
                reader.Vector3T = typeof(Vector3T);

                // Version is stored as negative int.
                reader.version = version = -reader.ReadInt32();

                // Sanity check file size.
                int size = reader.ReadInt32() + 8;
                if (size != reader.BaseStream.Length)
                {
                    logger(String.Format("File length expected is {0} but actual lenght is {1}.  File may be corrupt.", size, reader.BaseStream.Length));
                    throw new InvalidDataException();
                }

                // The bytes preceding this don't count when calculating the offset.
                reader.globalOffset = globalOffset = reader.ReadInt32() - 8;

                // Usually 2 in TIE98
                unknownWord = reader.ReadInt16();

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

        public List<T> FindAll<T>()
            where T : BaseNode
        {
            var found = new List<T>();
            foreach (BaseNode child in this)
            {
                if(child.GetType() == typeof(T))
                {
                    found.Add((T)child);
                }
                found.AddRange(child.FindAll<T>());
            }
            return found;
        }
    }
}
