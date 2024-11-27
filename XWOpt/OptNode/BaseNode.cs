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
using SchmooTech.XWOpt.OptNode.Types;

namespace SchmooTech.XWOpt.OptNode
{
    /// <summary>
    /// Common type for all types of nodes in an OPT file.
    /// </summary>
    public class BaseNode : IEnumerable<BaseNode>
    {
        /// <summary>
        /// Offset at which this node was read from the file.  Use for read debugging.
        /// </summary>
        public long OffsetInFile { get; }
        public string Name { get; }
        public NodeType NodeType { get; }

        public Collection<BaseNode> Children { get; } = new Collection<BaseNode>();
        public BaseNode Parent { get; private set; }
        internal BaseNode(string name, NodeType nodeType)
        {
            Name = name;
            NodeType = nodeType;
        }

        internal BaseNode(OptReader opt, NodeHeader header)
        {
            OffsetInFile = opt.BaseStream.Position;
            Name = header.Name;
            NodeType = header.NodeType;
            Parent = header.Parent;

            for (var i = 0; i < header.ChildCount; i++)
            {
                opt.Seek(header.ChildAddressTable + i * 4);
                var childAddress = opt.ReadInt32();

                if (childAddress != 0)
                {
                    var node = opt.ReadNodeAt(childAddress, this, this);
                    Children.Add(node);
                }
            }
        }

        public IEnumerator<BaseNode> GetEnumerator()
        {
            yield return this;

            foreach (var node in Children)
            {
                // Recursive walk down the hierarchy
                foreach (var subNode in node)
                {
                    yield return subNode;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddChild(BaseNode node)
        {
            Children.Add(node);
            node.Parent = this;
        }

        public bool RemoveChild(BaseNode node)
        {
            return Children.Remove(node);
        }
    }
}
