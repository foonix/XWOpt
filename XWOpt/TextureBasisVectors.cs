using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchmooTech.XWOpt
{
    public struct TextureBasisVectors<TVector3>
    {
        private TVector3 accrossTop;
        private TVector3 downSide;

        static Vector3Adapter<TVector3> v3adapter = new Vector3Adapter<TVector3>();

        public TVector3 AcrossTop { get => accrossTop; set => accrossTop = value; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DownSide")]
        public TVector3 DownSide { get => downSide; set => downSide = value; }

        internal TextureBasisVectors(OptReader reader)
        {
            accrossTop = v3adapter.Read(reader);
            downSide = v3adapter.Read(reader);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TextureBasisVectors<TVector3>))
                return false;

            return Equals((TextureBasisVectors<TVector3>)obj);
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
            return left.Equals(right);
        }

        public static bool operator !=(TextureBasisVectors<TVector3> left, TextureBasisVectors<TVector3> right)
        {
            return !left.Equals(right);
        }
    }
}
