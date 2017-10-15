using System;
using System.Collections.Generic;
using System.Text;

namespace SchmooTech.XWOpt.OptNode.Types
{
    public enum GenericMinor
    {
        branch = 0,
        faceList = 1,
        mainJump = 2,
        meshVertex = 3,
        info = 4,
        textureReferenceByName = 7,
        vertexNormal = 11,
        textureVertex = 13,
        textureHeader = 20,
        meshLOD = 21,
        hardpoint = 22,
        transform = 23,
        skinSelector = 24,
        meshDescriptor = 25,
        unknown = 0xff,
    }
}
