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

namespace SchmooTech.XWOpt.OptNode.Types
{
    public enum NodeType
    {
        Separator = 0,
        IndexedFaceSet = 1,
        Transform = 2,
        VertexPosition = 3,
        Translation = 4,
        Rotation = 5,
        Scale = 6,
        UseTexture = 7,
        Def = 8,
        Material = 9,
        MaterialBinding = 10,
        VertexNormal = 11,
        VertexNormalBinding = 12,
        TextureVertex = 13,
        TextureVertexBinding = 14,
        QuadMesh = 15,
        FaceSet = 16,
        TriangleStripSet = 17,
        Group = 18,
        BaseColour = 19,
        Texture = 20,
        MeshLod = 21,
        Hardpoint = 22,
        Pivot = 23,
        CamoSwitch = 24,
        ComponentInfo = 25,
        EngineGlow = 28,
        Unknown = 0xff,
    }
}
