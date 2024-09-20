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
    /// <summary>
    /// Center of rotation and axises of rotation.
    /// </summary>
    /// <typeparam name="TVector3"></typeparam>
    public class RotationInfo<TVector3> : BaseNode
    {
        /// <summary>
        /// For a pivoting structure, this is the point it pivots around.
        /// </summary>
        public TVector3 Offset { get; set; }
        /// <summary>
        /// The vector the object yaws clockwise around IE downward facing vector
        /// </summary>
        public TVector3 YawAxis { get; set; }
        /// <summary>
        /// The vector the object rolls around (clockwise = roll right) IE forward facting vector
        /// </summary>
        public TVector3 RollAxis { get; set; }
        /// <summary>
        /// The vector the object pitches around (clockwise = pitch up)
        /// </summary>
        public TVector3 PitchAxis { get; set; }

        internal RotationInfo(OptReader reader, NodeHeader nodeHeader) : base(reader, nodeHeader)
        {
            reader.Seek(nodeHeader.DataAddress);

            Offset = reader.ReadVector<TVector3>();
            YawAxis = reader.ReadVector<TVector3>();
            RollAxis = reader.ReadVector<TVector3>();
            PitchAxis = reader.ReadVector<TVector3>();
        }
    }
}
