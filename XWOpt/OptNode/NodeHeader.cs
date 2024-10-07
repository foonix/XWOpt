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

using SchmooTech.XWOpt.OptNode.Types;

namespace SchmooTech.XWOpt.OptNode
{
    public class NodeHeader
    {
        public int NameOffset { get; }
        public NodeType NodeType { get; }

        public int ChildCount { get; }
        public int ChildAddressTable { get; }

        public int DataCount { get; }
        public int DataAddress { get; }

        public string Name { get; }
        public BaseNode Parent { get; }

        internal NodeHeader(OptReader reader, BaseNode parent) 
        {
            Parent = parent;

            NameOffset = reader.ReadInt32();
            NodeType = (NodeType)reader.ReadInt32();

            ChildCount = reader.ReadInt32();
            ChildAddressTable = reader.ReadInt32();

            DataCount = reader.ReadInt32();
            DataAddress = reader.ReadInt32();

            if (NameOffset != 0)
            {
                reader.Seek(NameOffset);
                Name = reader.ReadString(100);
            }
            else
            {
                Name = string.Empty;
            }
        }
    }
}
