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

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SchmooTech.XWOpt.OptNode
{
    public class NodeCollection : BaseNode, IEnumerable<BaseNode>

    {
        Collection<BaseNode> children = new Collection<BaseNode>();

        public Collection<BaseNode> Children
        {
            get { return children; }
        }

        internal NodeCollection() : base() { }
        internal NodeCollection(OptReader reader) : base(reader)
        {
            ReadChildren(reader);
        }

        internal void ReadChildren(OptReader reader)
        {
            foreach (var child in reader.ReadChildren(this))
            {
                Children.Add(child);
            }
        }

        internal void ReadChildren(OptReader reader, int count, int jumpListOffset)
        {
            foreach (var child in reader.ReadChildren(count, jumpListOffset, this))
            {
                Children.Add(child);
            }
        }

        public IEnumerator<BaseNode> GetEnumerator()
        {
            foreach (var node in Children)
            {
                yield return node;

                if(node is NodeCollection branch) {
                    foreach (var subNode in branch) {
                        yield return subNode;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
