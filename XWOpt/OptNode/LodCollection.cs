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
    public class LodCollection : NodeCollection
    {
        private Collection<float> maxRenderDistance = new Collection<float>();
        public Collection<float> MaxRenderDistance { get => maxRenderDistance; }

        internal LodCollection(OptReader reader) : base()
        {
            int lodChildCount = reader.ReadInt32();
            int lodChildOffset = reader.ReadInt32();
            int lodThresholdCount = reader.ReadInt32();
            int lodThresholdOffset = reader.ReadInt32();

            // No idea why this would happen, but my understanding of this block is wrong if it does.
            if (lodChildCount != lodThresholdCount)
            {
                reader.logger(String.Format(CultureInfo.CurrentCulture, "Not the same number of LOD meshes ({0}) as LOD offsets ({1}) at {2:X}", lodChildCount, lodThresholdCount, reader.BaseStream.Position));
            }

            reader.Seek(lodThresholdOffset);
            maxRenderDistance.Clear();
            for (int i = 0; i < lodChildCount; i++)
            {
                maxRenderDistance.Add(reader.ReadSingle());
            }

            ReadChildren(reader, lodChildCount, lodChildOffset);
        }
    }
}
