using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchmooTech.XWOpt
{
    abstract class VectorAdapter
    {
        internal abstract object Read(OptReader reader);
        internal abstract object ReadCollection(OptReader reader, int count);
        internal abstract object Zero();
    }
}
