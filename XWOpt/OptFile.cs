using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace SchmooTech.XWOpt
{
    public class OptFile : List<OptNode.BaseNode>
    {
        // The number that is subtracted from the file's internal pointers to get the actual file position.
        public int globalOffset = 0xFF;
        public int version = 0;

        public Action<string> logger;

        public OptFile(string fileName, Action<string> logger = null)
        {
            if (null != logger)
            {
                this.logger = logger;
            }

            using (var reader = new OptReader(File.OpenRead(fileName), logger))
            {
                reader.ReadHeader();
                globalOffset = reader.globalOffset;
                version = reader.version;

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
