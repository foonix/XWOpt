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
using System.Reflection;

namespace SchmooTech.XWOpt
{
    internal class Vector3Adapter<TVector3> : VectorAdapter
    {
        ConstructorInfo vector3Cotr;
        readonly FieldInfo X, Y, Z;
        internal CoordinateSystemConverter<TVector3> RotateFromOptSpace { get; set; }
        internal CoordinateSystemConverter<TVector3> RotateToOptSpace { get; set; }

        public Vector3Adapter()
        {
            vector3Cotr = typeof(TVector3).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float) });

            X = typeof(TVector3).GetField("X", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            Y = typeof(TVector3).GetField("Y", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            Z = typeof(TVector3).GetField("Z", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);

            if (null == X || null == Y || null == Z)
            {
                throw new ArgumentException("Vector3 type must have x, y, and z fields (case insensitive).");
            }
        }

        internal override object Read(OptReader reader)
        {
            var v = vector3Cotr.Invoke(new object[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() });

            if (null != RotateFromOptSpace)
            {
                v = RotateFromOptSpace((TVector3)v);
            }

            return v;
        }

        internal override object ReadCollection(OptReader reader, int count)
        {
            var collection = new Collection<TVector3>();

            for (int i = 0; i < count; i++)
            {
                collection.Add((TVector3)Read(reader));
            }

            return collection;
        }

        internal void Write()
        {
            throw new NotImplementedException();
        }

        internal override object Zero()
        {
            return vector3Cotr.Invoke(new object[] { 0, 0, 0 });
        }
    }
}
