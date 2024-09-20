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

using SchmooTech.XWOpt;
using SchmooTech.XWOpt.OptNode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    class LodFactory : IBakeable
    {
        SeparatorNode _lodNode;
        readonly int _index;
        readonly float _threshold;
        PartFactory Part { get; set; }
        List<Mesh> skinSpecificSubmeshes = new List<Mesh>();

        /// <summary>
        /// Fudge factor for increasing LOD cutover distance based on increased resolution and improved
        /// rendering technology on modern computers.
        ///
        /// 640x480 -> 1920x1080 = ~3x increased resolution
        ///
        /// Add additional subjective multipliers based on anti-aliasing (4x) and texture filtering (4x).
        ///
        /// 3 * (4+4)
        /// </summary>
        public const float DetailImprovementFudgeFactor = 24f;

        internal LodFactory(PartFactory part, SeparatorNode lodNode, int index, float threshold)
        {
            Part = part;
            _lodNode = lodNode;
            _index = index;

            if (threshold > 0 && threshold < float.PositiveInfinity)
            {
                // OPT LOD thresholds are based on distance. Unity is based on screen height.
                _threshold = (float)(1 / (DetailImprovementFudgeFactor * Math.Atan(1 / threshold)));
            }
            else
            {
                _threshold = 0;
            }
        }

        Mesh MakeMesh(FaceList<Vector3> faceList, string textureName)
        {
            // MeshVertices<Vector3> verts, VertexNormals<Vector3> vertNormals, VertexUV<Vector2> vertUV

            // Must remain the same length.
            var meshVerts = new List<Vector3>();
            var meshUV = new List<Vector2>();
            var meshNorms = new List<Vector3>();
            var meshUV2 = new List<Vector2>();  // bottom left corner of subtexture in atlas
            var meshUV3 = new List<Vector2>();  // top right corner of subtexture in atlas

            // References into part level mesh data
            var optVerts = Part.verts;
            var optNormals = Part.vertNormals;
            var optUV = Part.vertUV;

            // Unity can only have one normal and UV per vertex.
            // All sub-meshes (triangle lists) in the same mesh have to share the same vertex list.
            // This data uses normal and UV data per face, per vertex.
            // So we have to make a new vertex any time a polygon references a different normal
            // or UV than another polygon.
            var usedVertLookup = new Dictionary<VertexSplitTuple, int>();

            // Workaround for sat1.opt: no Tex00005 on LOD1.
            int texId = 0;
            Part.Craft.TextureAtlas.Layout.TextureId.TryGetValue(textureName, out texId);

            Rect atlasRect = Part.Craft.TextureAtlas.Layout.GetUvLocation(texId);

            // Build the vert/normal/UV lists
            var triangles = new List<int>();
            for (int i = 0; i < faceList.Count; i++)
            {
                var newVertRefs = new int[4];

                // Some normals in xwing98 tug, xwing98 bwing, xwa shuttle and possibly others, are garbage data.
                // Other normals in the vicinity might also garbage, so flate shade the entire face.
                bool rejectNormal = false;
                for (int j = 0; j < 4; j++)
                {
                    var id = faceList.VertexNormalRef[i][j];
                    if (id < 0 || id >= optNormals.Normals.Count || optNormals.Normals[id] == Vector3.zero)
                    {
                        rejectNormal = true;
                    }
                }

                // check each point for need to generate new vertex
                for (int j = 0; j < 4; j++)
                {
                    VertexSplitTuple vt;
                    vt.vId = faceList.VertexRef[i][j];
                    vt.uvId = faceList.UVRef[i][j];
                    vt.normId = faceList.VertexNormalRef[i][j];
                    vt.texId = texId;

                    // Some faces are triangles instead of quads.
                    if (vt.vId == -1 || vt.uvId == -1 || vt.normId == -1)
                    {
                        newVertRefs[j] = -1;
                        continue;
                    }

                    // Some normals in xwing98 tug, xwing98 bwing, xwa shuttle and possibly others, are garbage data.
                    // Zero normals can cause unity lighting problems.  Fallback on face normal.
                    Vector3 normal;
                    if (rejectNormal)
                    {
                        normal = GetFaceNormal(faceList.VertexRef[i], optVerts);
                    }
                    else
                    {
                        normal = optNormals.Normals[vt.normId];
                    }

                    if (usedVertLookup.ContainsKey(vt) && !rejectNormal)
                    {
                        // reuse the vertex
                        newVertRefs[j] = usedVertLookup[vt];
                    }
                    else
                    {
                        // make a new one
                        if (vt.vId > optVerts.Vertices.Count - 1)
                        {
                            Debug.LogError(string.Format(CultureInfo.CurrentCulture, "Vert {0}/{4} out of bound {1:X} {2} {3} ", vt.vId, faceList.OffsetInFile, i, j, optVerts.Vertices.Count));
                        }
                        if (vt.normId > optNormals.Normals.Count - 1)
                        {
                            Debug.LogError(string.Format(CultureInfo.CurrentCulture, "Normal {0}/{4} out of bound {1:X} {2} {3} ", vt.normId, faceList.OffsetInFile, i, j, optNormals.Normals.Count));
                        }
                        if (vt.uvId > optUV.Vertices.Count - 1)
                        {
                            Debug.LogError(string.Format(CultureInfo.CurrentCulture, "UV {0}/{4} out of bound {1:X} {2} {3} ", vt.uvId, faceList.OffsetInFile, i, j, optUV.Vertices.Count));
                        }
                        meshVerts.Add(optVerts.Vertices[vt.vId] - Part.rotationInfo.Offset);
                        meshNorms.Add(normal.normalized);

                        // translate uv to atlas space
                        Vector2 uv = optUV.Vertices[vt.uvId];
                        //uv.x = uv.x * atlasRect.width + atlasRect.xMin;
                        //uv.y = uv.y * atlasRect.height + atlasRect.yMin;
                        meshUV.Add(uv);

                        meshUV2.Add(new Vector2(atlasRect.x, atlasRect.y));
                        meshUV3.Add(new Vector2(atlasRect.width, atlasRect.height));

                        // Index it so we can find it later.
                        usedVertLookup[vt] = meshVerts.Count - 1;
                        newVertRefs[j] = usedVertLookup[vt];
                    }
                }

                // TODO: Less nieve quad split.
                // First triangle
                triangles.Add(newVertRefs[1]);
                triangles.Add(newVertRefs[0]);
                triangles.Add(newVertRefs[2]);

                // second triangle if a quad
                if (newVertRefs[3] != -1)
                {
                    triangles.Add(newVertRefs[3]);
                    triangles.Add(newVertRefs[2]);
                    triangles.Add(newVertRefs[0]);
                }
            }

            var mesh = new Mesh
            {
                vertices = meshVerts.ToArray(),
                normals = meshNorms.ToArray(),
                triangles = triangles.ToArray(),
                uv = meshUV.ToArray(),
                uv2 = meshUV2.ToArray(),
                uv3 = meshUV3.ToArray(),
            };

            return mesh;
        }

        public void ParallelizableBake(int? degreesOfParallelism)
        {

        }

        public void MainThreadBake()
        {
            // cache a separate mesh for each skin.
            int skinCount = 1;
            foreach (var collection in _lodNode.OfType<SkinCollection>())
            {
                if (collection.Children.Count > skinCount)
                {
                    skinCount = collection.Children.Count;
                }
            }

            for (int skin = 0; skin < skinCount; skin++)
            {
                var subMeshes = new List<CombineInstance>();
                foreach (var assoc in WalkTextureAssociations(skin))
                {
                    var combineInstance = new CombineInstance()
                    {
                        mesh = MakeMesh(assoc.faceList, assoc.textureName)
                    };

                    subMeshes.Add(combineInstance);
                }

                var mesh = new Mesh();
                mesh.CombineMeshes(subMeshes.ToArray(), true, false, false);
                mesh.RecalculateBounds();
                mesh.name = ToString() + "_skin" + skin;

                skinSpecificSubmeshes.Add(mesh);
            }
        }

        /// <summary>
        /// Calculate the flat surface normal for a given quad.
        /// </summary>
        /// <param name="coordinateReferenceTuple"></param>
        /// <param name="verts"></param>
        /// <returns></returns>
        private static Vector3 GetFaceNormal(CoordinateReferenceTuple coordinateReferenceTuple, MeshVertices<Vector3> verts)
        {
            // I'm not aware of any models that have non-planar quads, so just steal unity example code :)
            Vector3 a = verts.Vertices[coordinateReferenceTuple[1]];
            Vector3 b = verts.Vertices[coordinateReferenceTuple[0]];
            Vector3 c = verts.Vertices[coordinateReferenceTuple[2]];

            var side1 = b - a;
            var side2 = c - a;

            return Vector3.Cross(side1, side2).normalized;
        }

        internal LOD MakeLOD(GameObject parent, int skin)
        {
            GameObject lodObj = new GameObject(ToString());
            lodObj.AddComponent<MeshFilter>();
            lodObj.AddComponent<MeshRenderer>();
            Helpers.AttachTransform(parent, lodObj);

            var skinMeshSwitch = lodObj.AddComponent<SkinMeshSwitch>();
            skinMeshSwitch.Initialise(skinSpecificSubmeshes.ToArray());

            skinMeshSwitch.SwitchSkin(skin);
            lodObj.GetComponent<MeshRenderer>().sharedMaterial = Part.Craft.TextureAtlas.Material;

            return new LOD(_threshold, new Renderer[] { lodObj.GetComponent<MeshRenderer>() });
        }

        /// <summary>
        /// Which texture to use for which submesh
        /// </summary>
        class TextureMeshAssociation
        {
            public string textureName;
            public FaceList<Vector3> faceList;
        }

        IEnumerable<TextureMeshAssociation> WalkTextureAssociations(int skin)
        {
            // It seems there is no direct connection between meshes and the textures that go on
            // them besides that the texture preceeds the mesh in this list.
            // So keep track of the last mesh or mesh reference we've seen and apply it to the next mesh.
            // If there is more than one texture preceding a mesh, the last one must be used.
            var previousTexture = new TextureMeshAssociation
            {
                faceList = null,
                textureName = null
            };

            foreach (var child in _lodNode.Children)
            {
                foreach (var assoc in WalkTextureAssociations(child, skin, previousTexture))
                {
                    yield return assoc;
                }
            }
        }

        IEnumerable<TextureMeshAssociation> WalkTextureAssociations(BaseNode child, int skin, TextureMeshAssociation previousTexture)
        {
            switch (child)
            {
                case XWOpt.OptNode.Texture t:
                    previousTexture.textureName = t.Name;
                    break;
                case TextureReferenceByName t:
                    previousTexture.textureName = t.TextureName;
                    break;
                case SkinCollection selector:
                    // Workaround: Some training platform parts have varying number of skins.
                    int usedSkin = skin % selector.Children.Count;
                    switch (selector.Children[usedSkin])
                    {
                        case XWOpt.OptNode.Texture t:
                            previousTexture.textureName = t.Name;
                            break;
                        case TextureReferenceByName t:
                            previousTexture.textureName = t.TextureName;
                            break;
                    }
                    yield break;
                case FaceList<Vector3> f:
                    // Some meshes are not preceeded by a texture.  In this case use a global default texture.
                    if (null == previousTexture.textureName)
                    {
                        previousTexture.textureName = "Tex00000";
                    }

                    previousTexture.faceList = f;

                    yield return previousTexture;
                    previousTexture.textureName = null;

                    break;
            }

            foreach (var subNode in child.Children)
            {
                foreach (var assoc in WalkTextureAssociations(subNode, skin, previousTexture))
                {
                    yield return assoc;
                }
            }
        }

        public override string ToString()
        {
            string partName;
            if (Part.descriptor is null)
            {
                partName = "Unknown Part";
            }
            else
            {
                partName = Part.descriptor.PartType.ToString();
            }

            return partName + "_LOD" + _index;
        }
    }
}
