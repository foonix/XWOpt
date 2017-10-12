using System;
using System.Collections.Generic;
using System.IO;

namespace SchmooTech.XWOpt.OptNode
{
    public class BaseNode : List<BaseNode>
    {
        // For debugging the read process
        long offsetInFile = 0;

        public BaseNode(OptFile opt)
        {
            offsetInFile = opt.BaseStream.Position;
        }

        // Instantiates the correct node type based on IDs at the current file offset.
        public static BaseNode ReadNode(OptFile opt)
        {
            int preHeaderOffset = 0;

            int majorId = opt.ReadInt32();
            int minorId = opt.ReadInt32();

            // Edge case: one block type doesn't start with major/minor type id and actually start with another offset.
            // So peek ahead one more long and shuffle numbers where they go.
            // This may not work if globalOffset is 0.
            // Should be a pointer to an offset containing string "Tex00000" or similar.
            int peek = opt.ReadInt32();
            if (majorId > opt.globalOffset && minorId == (long)Types.Major.textrue)
            {
                preHeaderOffset = majorId;
                majorId = minorId;
                minorId = peek;
            }
            else
            {
                opt.BaseStream.Seek(-4, SeekOrigin.Current);
            }

            // Figure out the type of node and build appropriate object.
            switch (majorId)
            {
                case (int)Types.Major.generic:
                    switch (minorId)
                    {
                        case (int)Types.GenericMinor.generic:
                            return new BaseNode(opt);
                        case (int)Types.GenericMinor.meshVertex:
                            return new MeshVerticies(opt) as BaseNode;
                        case (int)Types.GenericMinor.textureVertex:
                            return new VertexUV(opt) as BaseNode;
                        case (int)Types.GenericMinor.textureReferenceByName:
                            return new TextureReferenceByName(opt) as BaseNode;
                        case (int)Types.GenericMinor.vertexNormal:
                            return new VertexNormals(opt) as BaseNode;
                        case (int)Types.GenericMinor.hardpoint:
                            return new Hardpoint(opt) as BaseNode;
                        case (int)Types.GenericMinor.transform:
                            return new Transform(opt) as BaseNode;
                        case (int)Types.GenericMinor.meshLOD:
                            return new MeshLOD(opt) as BaseNode;
                        case (int)Types.GenericMinor.faceList:
                            return new FaceList(opt) as BaseNode;
                        case (int)Types.GenericMinor.skinSelector:
                            return new SkinSelector(opt) as BaseNode;
                        case (int)Types.GenericMinor.meshDescriptor:
                            return new PartDescriptor(opt) as BaseNode;
                        default:
                            opt.logger("Found unknown node type " + majorId + " " + minorId + " at " + opt.BaseStream.Position);
                            return new BaseNode(opt);
                    }

                case (int)Types.Major.textrue:
                    switch (minorId)
                    {
                        case (int)Types.TextureMinor.texture:
                            return new Texture(opt, preHeaderOffset);
                        case (int)Types.TextureMinor.textureWithAlpha:
                            return new BaseNode(opt);
                        default:
                            opt.logger("Found unknown node type " + majorId + " " + minorId + " at " + opt.BaseStream.Position);
                            return new Texture(opt, preHeaderOffset) as BaseNode;
                    }
                default:
                    return new BaseNode(opt);
            }
        }
    }
}
