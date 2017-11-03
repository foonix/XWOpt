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
        public long InnerColor { get; set; }
        public long OuterColor { get; set; }
        public TVector3 Center { get; set; }

        // Unknown use vectors.
        public TVector3 X { get; set; }
        public TVector3 Y { get; set; }
        public TVector3 Z { get; set; }

        internal EngineGlow(OptReader reader) : base(reader)
        {
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(1, this);
            reader.FollowPointerToNextByte(this);
            reader.ReadUnknownUseValue(0, this);

            InnerColor = reader.ReadInt32();
            OuterColor = reader.ReadInt32();
            Center = reader.ReadVector<TVector3>();

            // Cargo culting the order.
            Y = reader.ReadVector<TVector3>();
            Z = reader.ReadVector<TVector3>();
            X = reader.ReadVector<TVector3>();
        }
    }
}
