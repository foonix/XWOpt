using System.IO;

namespace SchmooTech.XWOpt.OptNode
{
    public class FaceList<Vector3T> : BaseNode
    {
        public int[,] vertexRef, edgeRef, UVRef, vertexNormalRef;

        public Vector3T[] faceNormals;
        public Vector3T[] accrossTop;
        public Vector3T[] downSide;

        public int count, edgeCount;

        static Vector3Adapter<Vector3T> v3Adapter = new Vector3Adapter<Vector3T>();

        internal FaceList(OptReader reader) : base(reader)
        {
            // unknown zeros
            reader.ReadUnknownUseValue(0);
            reader.ReadUnknownUseValue(0);

            count = reader.ReadInt32();
            reader.FollowPointerToNextByte();
            edgeCount = reader.ReadInt32();

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

            faceNormals = v3Adapter.ReadArray(reader, count);


            // Not sure these are actually useful in unity.
            accrossTop = new Vector3T[count];
            downSide = new Vector3T[count];
            for (int i = 0; i < count; i++)
            {
                v3Adapter.Read(reader, ref accrossTop[i]);
                try
                {
                    v3Adapter.Read(reader, ref downSide[i]);
                }
                catch (EndOfStreamException e)
                {
                    // Edge case: TIE98 CORVTA.OPT is missing a float at EOF.
                    reader.logger(e.Message);
                }
            }
        }
    }
}
