﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchmooTech.XWOpt
{
    public class CoordinateReferenceTuple : IEquatable<CoordinateReferenceTuple>
    {
        int[] values = new int[4];

        public int this[int which]
        {
            get { return GetIndex(which); }
            set { SetIndex(which, value); }
        }

        internal CoordinateReferenceTuple(OptReader reader)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = reader.ReadInt32();
            }
        }

        public int GetIndex(int which)
        {
            return values[which];
        }

        public void SetIndex(int which, int value)
        {
            values[which] = value;
        }

        public override bool Equals(object obj)
        {
            var crt = obj as CoordinateReferenceTuple;
            if (null == crt)
            {
                return false;
            }

            return Equals(crt);
        }

        public bool Equals(CoordinateReferenceTuple other)
        {
            if(null == other) { return false; }

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != other.values[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return values[0] ^ values[1] ^ values[2] ^ values[3];
        }

        public static bool operator ==(CoordinateReferenceTuple left, CoordinateReferenceTuple right)
        {
            if(null == left || null == right)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(CoordinateReferenceTuple left, CoordinateReferenceTuple right)
        {
            if (null == left || null == right)
            {
                return false;
            }
            return !left.Equals(right);
        }
    }
}