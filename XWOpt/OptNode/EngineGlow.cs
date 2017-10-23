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

namespace SchmooTech.XWOpt.OptNode
{
    class EngineGlow<TVector3> : BaseNode
    {
        // RGBA colors
        private long innerColor, outerColor;
        private TVector3 center, x, y, z;

        public long InnerColor { get => innerColor; set => innerColor = value; }
        public long OuterColor { get => outerColor; set => outerColor = value; }
        public TVector3 Center { get => Center; set => Center = value; }

        // Unknown use vectors.
        public TVector3 X { get => x; set => x = value; }
        public TVector3 Y { get => y; set => y = value; }
        public TVector3 Z { get => z; set => z = value; }

        static Vector3Adapter<TVector3> v3Adapter = new Vector3Adapter<TVector3>();

        internal EngineGlow(OptReader reader) : base(reader)
        {
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(1, this);
            reader.FollowPointerToNextByte(this);
            reader.ReadUnknownUseValue(0, this);

            innerColor = reader.ReadInt32();
            outerColor = reader.ReadInt32();
            v3Adapter.Read(reader, ref center);

            // Cargo culting the order.
            v3Adapter.Read(reader, ref y);
            v3Adapter.Read(reader, ref z);
            v3Adapter.Read(reader, ref x);
        }
    }
}
