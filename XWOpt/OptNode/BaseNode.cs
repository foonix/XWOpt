using System;
using System.Collections.Generic;
using System.IO;

namespace SchmooTech.XWOpt.OptNode
{
    public class BaseNode : List<BaseNode>
    {
        // For debugging the read process
        long offsetInFile = 0;

        internal BaseNode(OptReader opt)
        {
            offsetInFile = opt.BaseStream.Position;
        }


    }
}
