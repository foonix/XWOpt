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

using System.Collections.Generic;
using System.Linq;
using SchmooTech.XWOpt.OptNode;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    internal class PartFactory
    {
        internal SeparatorNode ShipPart { get; set; }
        internal CraftFactory Craft { get; set; }
        internal int PartIndex { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal bool CreateChildTarget
        {
            get
            {
                return null == targetPoint;
            }
            set
            {
                if (true == value)
                {
                    targetPoint = new TargetPointFactory(Craft, new DistinctTargetGroupTuple(descriptor));
                }
                else
                {
                    targetPoint = null;
                }
            }
        }

        internal PartDescriptor<Vector3> descriptor;
        internal RotationInfo<Vector3> rotationInfo;
        internal MeshVertices<Vector3> verts;
        internal VertexUV<Vector2> vertUV;
        internal VertexNormals<Vector3> vertNormals;

        List<LodFactory> _lods;
        HardpointFactory hardpointFactory;
        TargetPointFactory targetPoint = null;

        internal PartFactory(CraftFactory craft, SeparatorNode shipPart, int partIndex)
        {
            Craft = craft;
            ShipPart = shipPart;
            PartIndex = partIndex;
            hardpointFactory = new HardpointFactory(craft);

            // Fetch ship part top level data
            descriptor = ShipPart.Children.OfType<PartDescriptor<Vector3>>().First();
            rotationInfo = ShipPart.Children.OfType<RotationInfo<Vector3>>().First();
            verts = ShipPart.OfType<MeshVertices<Vector3>>().First();
            vertUV = ShipPart.OfType<VertexUV<Vector2>>().First();
            vertNormals = ShipPart.OfType<VertexNormals<Vector3>>().First();

            // All meshes are contained inside of a MeshLod
            // There is only one MeshLod per part.
            // Each LOD is BranchNode containing a collection of meshes and textures it uses.
            var lodNode = ShipPart.OfType<LodCollection>().First();

            _lods = new List<LodFactory>();
            int newLodIndex = 0;
            for (int i = 0; i < lodNode.MaxRenderDistance.Count; i++)
            {
                float distance = lodNode.MaxRenderDistance[i];

                // Out of order LODs are probably broken.  See TIE98 CAL.OPT.
                // If this distance is greater than the previous distance (smaller number means greater render distance),
                // then this LOD is occluded by the previous LOD.
                if (distance > 0 && i > 0 && distance > lodNode.MaxRenderDistance[i - 1])
                {
                    continue;
                }

                if (lodNode.Children[i] is SeparatorNode branch)
                {
                    _lods.Add(new LodFactory(this, branch, newLodIndex, distance));
                    newLodIndex++;
                }
                else
                {
                    Debug.LogError("Skipping LOD" + newLodIndex + " as it is not a Separator Node");
                }
            }
        }

        internal GameObject CreatePart(GameObject parent, int skin)
        {
            var partObj = UnityEngine.Object.Instantiate(Craft.PartBase) as GameObject;

            if (null != descriptor)
            {
                // Give the part object a useful name in the Unity GUI
                partObj.name = descriptor.PartType.ToString() + " part";
            }

            var lods = new List<LOD>();
            foreach (var lod in _lods)
            {
                lods.Add(lod.MakeLOD(partObj, skin));
            }

            var lodGroup = partObj.AddComponent<LODGroup>();
            lodGroup.SetLODs(lods.ToArray());
            // Some low LOD meshes comletely replace meshes from other parts.
            // So ensure that all parts cutover at the same distance to avoid flickering.
            lodGroup.localReferencePoint = new Vector3(0, 0, 0);
            lodGroup.size = Craft.Size;

            // Generate hardpoints
            foreach (var hardpoint in ShipPart.OfType<Hardpoint<Vector3>>())
            {
                hardpointFactory.MakeHardpoint(partObj, hardpoint, descriptor, rotationInfo);
            }

            if (null != targetPoint)
            {
                Helpers.AttachTransform(partObj, targetPoint.CreateTargetPoint(), descriptor.HitboxCenterPoint);
            }

            Helpers.AttachTransform(parent, partObj, rotationInfo.Offset);

            Craft.ProcessPart?.Invoke(PartIndex, partObj, descriptor, rotationInfo);

            return partObj;
        }

        internal void ParallelizableBake(int? degreesOfParallelism)
        {
            foreach (var lod in _lods)
            {
                lod.ParallelizableBake(degreesOfParallelism);
            }
        }

        internal void MainThreadBake()
        {
            foreach(var lod in _lods)
            {
                lod.MainThreadBake();
            }
        }
    }
}
