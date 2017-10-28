using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchmooTech.XWOpt
{
    public struct CoordinateReferenceTuple : IEquatable<CoordinateReferenceTuple>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "A")]
        public int A { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "B")]
        public int B { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "C")]
        public int C { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "D")]
        public int D { get; set; }

        internal CoordinateReferenceTuple(OptReader reader)
        {
            A = reader.ReadInt32();
            B = reader.ReadInt32();
            C = reader.ReadInt32();
            D = reader.ReadInt32();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CoordinateReferenceTuple))
                return false;

            return Equals((CoordinateReferenceTuple)obj);
        }

        public bool Equals(CoordinateReferenceTuple other)
        {
            return (A == other.A && B == other.B && C == other.C && D == other.D);
        }

        public override int GetHashCode()
        {
            return A^B^C^D;
        }

        public static bool operator ==(CoordinateReferenceTuple left, CoordinateReferenceTuple right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CoordinateReferenceTuple left, CoordinateReferenceTuple right)
        {
            return !left.Equals(right);
        }
    }
}
