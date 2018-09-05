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

using SchmooTech.XWOpt.OptNode;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    class LodFactory
    {
        Mesh _mesh;
        NodeCollection _lodNode;
        int _index;
        float _threshold;
        PartFactory Part { get; set; }

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

        internal LodFactory(PartFactory part, NodeCollection lodNode, int index, float threshold)
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

            _mesh = GatherSubmeshes();
        }

        Mesh GatherSubmeshes()
        {
            List<FaceList<Vector3>> faceLists = new List<FaceList<Vector3>>();

            // All meshes inside of the LOD node, recursively
            foreach (var child in _lodNode)
            {
                switch (child)
                {
                    case FaceList<Vector3> faces:
                        faceLists.Add(faces);
                        break;
                }
            }

            return MakeMesh(faceLists, Part.verts, Part.vertNormals, Part.vertUV);
        }

        static Mesh MakeMesh(List<FaceList<Vector3>> faceLists, MeshVertices<Vector3> verts, VertexNormals<Vector3> vertNormals, VertexUV<Vector2> vertUV)
        {
            var mesh = new Mesh();

            var submeshList = new List<List<int>>();

            // Must remain the same length.
            var meshVerts = new List<Vector3>();
            var meshUV = new List<Vector2>();
            var meshNorms = new List<Vector3>();

            // Unity can only have one normal and UV per vertex.
            // All sub-meshes (triangle lists) in the same mesh have to share the same vertex list.
            // This data uses normal and UV data per face, per vertex.
            // So we have to make a new vertex any time a polygon references a different normal
            // or UV than another polygon.
            var usedVertLookup = new Dictionary<VertexSplitTuple, int>();

            // Build the vert/normal/UV lists
            foreach (FaceList<Vector3> faceList in faceLists)
            {
                var triangles = new List<int>();
                for (int i = 0; i < faceList.Count; i++)
                {
                    var newVertRefs = new int[4];

                    // check each point for need to generate new vertex
                    for (int j = 0; j < 4; j++)
                    {
                        VertexSplitTuple vt;
                        vt.vId = faceList.VertexRef[i][j];
                        vt.uvId = faceList.UVRef[i][j];
                        vt.normId = faceList.VertexNormalRef[i][j];

                        // Some faces are triangles instead of quads.
                        if (vt.vId == -1 || vt.uvId == -1 || vt.normId == -1)
                        {
                            newVertRefs[j] = -1;
                            continue;
                        }

                        if (usedVertLookup.ContainsKey(vt))
                        {
                            // reuse the vertex
                            newVertRefs[j] = usedVertLookup[vt];
                        }
                        else
                        {
                            // make a new one
                            if (vt.vId > verts.Vertices.Count - 1)
                            {
                                Debug.LogError(string.Format(CultureInfo.CurrentCulture, "Vert {0}/{4} out of bound {1:X} {2} {3} ", vt.vId, faceList.OffsetInFile, i, j, verts.Vertices.Count));
                            }
                            if (vt.normId > vertNormals.Normals.Count - 1)
                            {
                                Debug.LogError(string.Format(CultureInfo.CurrentCulture, "Normal {0}/{4} out of bound {1:X} {2} {3} ", vt.normId, faceList.OffsetInFile, i, j, vertNormals.Normals.Count));
                            }
                            if (vt.uvId > vertUV.Vertices.Count - 1)
                            {
                                Debug.LogError(string.Format(CultureInfo.CurrentCulture, "UV {0}/{4} out of bound {1:X} {2} {3} ", vt.uvId, faceList.OffsetInFile, i, j, vertUV.Vertices.Count));
                            }
                            meshVerts.Add(verts.Vertices[vt.vId]);
                            meshUV.Add(vertUV.Vertices[vt.uvId]);
                            meshNorms.Add(vertNormals.Normals[vt.normId]);

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
                submeshList.Add(triangles);
            }

            mesh.SetVertices(meshVerts);
            mesh.SetUVs(0, meshUV);
            mesh.SetNormals(meshNorms);

            mesh.subMeshCount = submeshList.Count;
            for (int i = 0; i < submeshList.Count; i++)
            {
                mesh.SetTriangles(submeshList[i], i);
            }

            return mesh;
        }

        internal LOD MakeLOD(GameObject parent, int skin)
        {
            GameObject lodObj = new GameObject(parent.name + "_LOD" + _index);
            lodObj.AddComponent<MeshFilter>();
            lodObj.AddComponent<MeshRenderer>();
            Helpers.AttachTransform(parent, lodObj);

            var matsUsed = new List<string>();

            // It seems there is no direct connection between meshes and the textures that go on
            // them besides that the texture preceeds the mesh in this list.
            // So keep track of the last mesh or mesh reference we've seen and apply it to the next mesh.
            // If there is more than one texture preceding a mesh, the last one must be used.
            string previousTexture = null;

            // Workaround for lambda shuttle.  Fuselage parts have a weird sub-part wrapper.
            var SearchedNodes = new List<BaseNode>();
            foreach (var child in _lodNode.Children)
            {
                switch (child)
                {
                    case NameNode named:
                        SearchedNodes.AddRange((named.Children[0] as NodeCollection).Children);
                        break;
                    default:
                        SearchedNodes.Add(child);
                        break;
                }
            }

            foreach (var child in SearchedNodes)
            {
                switch (child)
                {
                    case XWOpt.OptNode.Texture t:
                        previousTexture = t.Name;
                        break;
                    case TextureReferenceByName t:
                        previousTexture = t.Name;
                        break;
                    case SkinCollection selector:
                        switch (selector.Children[skin])
                        {
                            case XWOpt.OptNode.Texture t:
                                previousTexture = t.Name;
                                break;
                            case TextureReferenceByName t:
                                previousTexture = t.Name;
                                break;
                        }
                        break;
                    case FaceList<Vector3> _:
                        // Some meshes are not preceeded by a texture.  In this case use a global default texture.
                        if (null == previousTexture)
                        {
                            matsUsed.Add("Tex00000");
                        }
                        else
                        {
                            matsUsed.Add(previousTexture);
                        }
                        previousTexture = null;
                        break;
                }
            }

            // Look up materials used by name to get references to the actual materials.
            Material[] mats = new Material[matsUsed.Count];
            for (int i = 0; i < matsUsed.Count; i++)
            {
                mats[i] = Part.Craft.materials[matsUsed[i]];
            }
            lodObj.GetComponent<MeshRenderer>().materials = mats;

            lodObj.GetComponent<MeshFilter>().mesh = _mesh;

            return new LOD(_threshold, new Renderer[] { lodObj.GetComponent<MeshRenderer>() });
        }
    }
}
