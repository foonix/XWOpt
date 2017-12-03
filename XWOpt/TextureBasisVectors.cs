﻿/*
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

namespace SchmooTech.XWOpt
{
    public struct TextureBasisVectors<TVector3>
    {
        private TVector3 accrossTop;
        private TVector3 downSide;

        public TVector3 AcrossTop { get => accrossTop; set => accrossTop = value; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DownSide")]
        public TVector3 DownSide { get => downSide; set => downSide = value; }

        internal TextureBasisVectors(OptReader reader)
        {
            accrossTop = reader.ReadVector<TVector3>();
            downSide = reader.ReadVector<TVector3>();
        }

        public override bool Equals(object obj)
        {
            switch(obj) {
                case TextureBasisVectors<TVector3> tbv:
                    return Equals(tbv);
                default:
                    return false;
            }
        }

        public bool Equals(TextureBasisVectors<TVector3> other)
        {
            return (accrossTop.Equals(other.AcrossTop) && downSide.Equals(other.DownSide));
        }

        public override int GetHashCode()
        {
            return accrossTop.GetHashCode() | downSide.GetHashCode();
        }

        public static bool operator ==(TextureBasisVectors<TVector3> left, TextureBasisVectors<TVector3> right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(TextureBasisVectors<TVector3> left, TextureBasisVectors<TVector3> right)
        {
            if (ReferenceEquals(left, null))
            {
                return !ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }
    }
}
