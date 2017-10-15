using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchmooTech.XWOpt.OptNode
{
    public class BranchNode : BaseNode
    {
        internal BranchNode(OptReader opt) : base(opt)
        {
            AddRange(opt.ReadChildren());
        }
    }
}
