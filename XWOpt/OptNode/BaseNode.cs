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

using System.Collections.Generic;

namespace SchmooTech.XWOpt.OptNode
{
    public class BaseNode : List<BaseNode>
    {
        // For debugging the read process
        public readonly long offsetInFile = 0;
        public object parent;

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
