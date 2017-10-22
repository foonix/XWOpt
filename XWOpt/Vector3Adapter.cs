using System;
using System.Reflection;

namespace SchmooTech.XWOpt
{
    internal class Vector3Adapter<Vector3T>
    {
        ConstructorInfo vector3Cotr;
        FieldInfo X, Y, Z;

        public Vector3Adapter()
        {
            vector3Cotr = typeof(Vector3T).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float) });

            X = typeof(Vector3T).GetField("X", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            Y = typeof(Vector3T).GetField("Y", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            Z = typeof(Vector3T).GetField("Z", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);

            if (null == X || null == Y || null == Z)
            {
                throw new ArgumentException("Vector3 type must have x, y, and z fields (case insensitive).");
            }
        }

        // Using ref because Vector3T can't be initialized here.
        internal void Read(OptReader reader, ref Vector3T v)
        {
            TypedReference typedRef = __makeref(v);
            X.SetValueDirect(typedRef, reader.ReadSingle());
            Y.SetValueDirect(typedRef, reader.ReadSingle());
            Z.SetValueDirect(typedRef, reader.ReadSingle());
        }

        internal Vector3T[] ReadArray(OptReader reader, int count)
        {
            var v3Array = new Vector3T[count];

            for (int i = 0; i < count; i++)
            {
                Read(reader, ref v3Array[i]);
            }

            return v3Array;
        }


        internal void Write()
        {
            throw new NotImplementedException();
        }
    }
}
