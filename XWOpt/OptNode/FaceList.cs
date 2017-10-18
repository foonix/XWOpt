using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SchmooTech.XWOpt.OptNode
{
    public class FaceList : BaseNode
    {
        public int[,] vertexRef, edgeRef, UVRef, vertexNormalRef;

        public object[] faceNormals;
        public object[] accrossTop;
        public object[] downSide;

        public long count, edgeCount;

        internal FaceList(OptReader reader) : base(reader)
        {
            // unknown zeros
            reader.ReadUnknownUseValue(0);
            reader.ReadUnknownUseValue(0);

            count = reader.ReadUInt32();
            reader.FollowPointerToNextByte();
            edgeCount = reader.ReadUInt32();

            vertexRef = new int[count, 4];
            edgeRef = new int[count, 4];
            UVRef = new int[count, 4];
            vertexNormalRef = new int[count, 4];

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    vertexRef[i, j] = reader.ReadInt32();
                }

                for (int j = 0; j < 4; j++)
                {
                    edgeRef[i, j] = reader.ReadInt32();
                }

                for (int j = 0; j < 4; j++)
                {
                    UVRef[i, j] = reader.ReadInt32();
                }

                for (int j = 0; j < 4; j++)
                {
                    vertexNormalRef[i, j] = reader.ReadInt32();
                }
            }

            faceNormals = new object[count];
            for (int i = 0; i < count; i++)
            {
                faceNormals[i] = reader.opt.vector3Cotr.Invoke(new object[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() });
            }

            // Not sure these are actually useful in unity.
            accrossTop = new object[count];
            downSide = new object[count];
            for (int i = 0; i < count; i++)
            {
                accrossTop[i] = reader.opt.vector3Cotr.Invoke(new object[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() });

                downSide[i] = reader.opt.vector3Cotr.Invoke(new object[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() });
            }
        }
    }
}
