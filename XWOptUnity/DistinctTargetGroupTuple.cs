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

using SchmooTech.XWOpt.OptNode;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{

    internal struct DistinctTargetGroupTuple
    {
        internal int id;
        internal PartType type;
        internal Vector3 location;

        internal DistinctTargetGroupTuple(PartDescriptor<Vector3> descriptor)
        {
            id = descriptor.TargetGroupId;
            type = descriptor.PartType;
            // You'd think this would be TargetPoint, but from my testing it seems to be HitBoxCenter.
            // TODO: Figure out what TargetPoint is actually for.
            location = descriptor.HitboxCenterPoint;
        }

        internal bool Equals(DistinctTargetGroupTuple other)
        {
            if (other.GetType() != this.GetType())
            {
                return false;
            }

            return this.id == other.id && this.type == other.type && this.location == other.location;
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case DistinctTargetGroupTuple o:
                    return this.id == o.id && this.type == o.type && this.location == o.location;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DistinctTargetGroupTuple left, DistinctTargetGroupTuple right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(DistinctTargetGroupTuple left, DistinctTargetGroupTuple right)
        {
            if (ReferenceEquals(left, null))
            {
                return !ReferenceEquals(right, null);
            }

            return !left.Equals(right);
        }
    }
}
