/*
 * Copyright 2017 Jason McNew
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies
 * or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
 * OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.ObjectModel;

namespace SchmooTech.XWOpt.OptNode
{
    public class BranchNode : BaseNode
    {
        Collection<BaseNode> children = new Collection<BaseNode>();

        internal BranchNode() : base() { }
        internal BranchNode(OptReader reader) : base(reader)
        {
            ReadChildren(reader);
        }

        internal void ReadChildren(OptReader reader)
        {
            foreach (var child in reader.ReadChildren(this))
            {
                children.Add(child);
            }
        }

        internal void ReadChildren(OptReader reader, int count, int jumpListOffset)
        {
            foreach (var child in reader.ReadChildren(count, jumpListOffset, this))
            {
                children.Add(child);
            }
        }

        public Collection<T> FindAll<T>()
            where T : BaseNode
        {
            var found = new Collection<T>();
            foreach (BaseNode child in children)
            {
                if (child is T)
                {
                    found.Add((T)child);
                }

                var branch = child as BranchNode;
                if (null != branch)
                {
                    foreach (var node in branch.FindAll<T>())
                    {
                        found.Add(node);
                    }
                }
            }
            return found;
        }
    }
}
