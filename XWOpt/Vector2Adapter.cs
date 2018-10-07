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
    class Vector2Adapter<TVector2> : VectorAdapter
    {
        // TODO: Call constructor if ref type.
        ConstructorInfo vector3Cotr;
        readonly FieldInfo X, Y;

        public Vector2Adapter()
        {
            vector3Cotr = typeof(TVector2).GetConstructor(new Type[] { typeof(float), typeof(float) });

            X = typeof(TVector2).GetField("X", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            Y = typeof(TVector2).GetField("Y", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);

            if (null == X || null == Y)
            {
                throw new ArgumentException("Vector2 type must have x and y fields (case insensitive).");
            }
        }

        internal override object Read(OptReader reader)
        {
            return (TVector2)vector3Cotr.Invoke(new object[] { reader.ReadSingle(), reader.ReadSingle() });
        }

        internal override object ReadCollection(OptReader reader, int count)
        {
            var collection = new Collection<TVector2>();

            for (int i = 0; i < count; i++)
            {
                collection.Add((TVector2)Read(reader));
            }

            return collection;
        }

        internal void Write()
        {
            throw new NotImplementedException();
        }

        internal override object Zero()
        {
            return vector3Cotr.Invoke(new object[] { 0, 0 });
        }
    }
}
