using System;

namespace SchmooTech.XWOpt.OptNode
{
    public class MeshLOD : BaseNode
    {
        public float[] LODThresholds;

        internal MeshLOD(OptReader reader) : base(reader)
        {
            int lodChildCount = reader.ReadInt32();
            int lodChildOffset = reader.ReadInt32();
            int lodThresholdCount = reader.ReadInt32();
            int lodThresholdOffset = reader.ReadInt32();

            // No idea why this would happen, but my understanding of this block is wrong if it does.
            if (lodChildCount != lodThresholdCount)
            {
                reader.logger(String.Format("Not the same number of LOD meshes ({0}) as LOD offsets ({1}) at {2:X}", lodChildCount, lodThresholdCount, reader.BaseStream.Position));
            }

            LODThresholds = new float[lodChildCount];
            reader.Seek(lodThresholdOffset);
            for (int i = 0; i < lodChildCount; i++)
            {
                LODThresholds[i] = reader.ReadSingle();
            }

            AddRange(reader.ReadChildren(lodChildCount, lodChildOffset));
        }
    }
}
