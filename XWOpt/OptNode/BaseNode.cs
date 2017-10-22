using System.Collections.Generic;

namespace SchmooTech.XWOpt.OptNode
{
    public class BaseNode : List<BaseNode>
    {
        // For debugging the read process
        public long offsetInFile = 0;

        internal BaseNode(OptReader opt)
        {
            offsetInFile = opt.BaseStream.Position;
        }

        public List<T> FindAll<T>()
            where T : BaseNode
        {
            var found = new List<T>();
            foreach (BaseNode child in this)
            {
                if (child.GetType() == typeof(T))
                {
                    found.Add((T)child);
                }
                found.AddRange(child.FindAll<T>());
            }
            return found;
        }
    }
}
