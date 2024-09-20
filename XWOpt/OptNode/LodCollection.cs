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

using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SchmooTech.XWOpt.OptNode
{
    public class LodCollection : BaseNode
    {
        /// <summary>
        /// Render cutoff distances associated with each LOD, in same order as Children.  (This may be in no logical order.)
        /// </summary>
        public Collection<float> MaxRenderDistance { get; } = new Collection<float>();

        internal LodCollection(OptReader reader, NodeHeader nodeHeader) : base(reader, nodeHeader)
        {
            // No idea why this would happen, but my understanding of this block is wrong if it does.
            if (nodeHeader.ChildCount != nodeHeader.DataCount)
            {
                reader.logger?.Invoke(String.Format(CultureInfo.CurrentCulture, "Not the same number of LOD meshes ({0}) as LOD offsets ({1}) at {2:X}", nodeHeader.ChildCount, nodeHeader.DataCount, reader.BaseStream.Position));
            }

            reader.Seek(nodeHeader.DataAddress);
            MaxRenderDistance.Clear();
            for (int i = 0; i < nodeHeader.DataCount; i++)
            {
                float distance = reader.ReadSingle();
                // A distance of 0 represents infinite draw distance
                // Converting to PositiveInfinity sorts it correctly.
                distance = distance == 0 ? float.PositiveInfinity : distance;
                MaxRenderDistance.Add(distance);
            }
        }
    }
}
