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
using UnityEngine;
using SchmooTech.XWOpt.OptNode;

namespace SchmooTech.XWOptUnity
{
    internal class PartFactory
    {
        internal BranchNode ShipPart { get; set; }

        internal PartDescriptor<Vector3> descriptor;
        internal MeshVertices<Vector3> verts;
        internal VertexUV<Vector2> vertUV;
        internal VertexNormals<Vector3> vertNormals;

        List<LodFactory> _lods;
        internal CraftFactory _craft;
        HardpointFactory hardpointFactory;
        TargetPointFactory targetPointFactory;

        internal PartFactory(CraftFactory craft, BranchNode shipPart)
        {
            _craft = craft;
            ShipPart = shipPart;
            hardpointFactory = new HardpointFactory(this);
            targetPointFactory = new TargetPointFactory(this);

            // Fetch ship part top level data
            descriptor = ShipPart.Children.OfType<PartDescriptor<Vector3>>().First();
            verts = ShipPart.FindAll<MeshVertices<Vector3>>().First();
            vertUV = ShipPart.FindAll<VertexUV<Vector2>>().First();
            vertNormals = ShipPart.FindAll<VertexNormals<Vector3>>().First();

            // All meshes are contained inside of a MeshLod
            // There is only one MeshLod per part.
            // Each LOD is BranchNode containing a collection of meshes and textures it uses.
            var lodNode = ShipPart.FindAll<MeshLod>().First<MeshLod>();
            int lodIndex = 0;

            // TODO: sort by threshold order.
            _lods = new List<LodFactory>();
            foreach (var lodLevel in lodNode.Children.OfType<BranchNode>())
            {
                _lods.Add(new LodFactory(this, lodLevel, lodIndex, lodNode.MaxRenderDistance[lodIndex]));
                lodIndex++;
            }
        }

        internal GameObject CreatePart(int skin)
        {
            var partObj = Object.Instantiate(_craft.PartBase) as GameObject;

            if (null != descriptor)
            {
                // Give the part object a useful name in the Unity GUI
                partObj.name = descriptor.PartType.ToString();

                // Create an object used for targeting.
                targetPointFactory.CreateTargetPoint(partObj, descriptor.CenterPoint, descriptor);
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
            lodGroup.RecalculateBounds();

            // Generate hardpoints
            foreach (var hardpoint in ShipPart.FindAll<Hardpoint<Vector3>>())
            {
                hardpointFactory.MakeHardpoint(partObj, hardpoint, descriptor);
            }

            _craft.ProcessPart(partObj, descriptor);

            return partObj;
        }
    }
}
