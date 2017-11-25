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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    /// <summary>
    /// Callback for game specific setup of part objects after instantiation.
    /// Use this to modify or filter parts based on data in the OPT file.
    /// </summary>
    /// <param name="part">The part object that has been instantiated</param>
    /// <param name="descriptor">The XWOpt part descriptor associated with the part</param>
    public delegate void ProcessPartHandler(GameObject part, PartDescriptor<Vector3> descriptor);
    /// <summary>
    /// Callback for game specific setup of part objects after instantiation.
    /// Use this to modify or filter hardpoints, eg connect them to the game's firing system, etc.
    /// </summary>
    /// <param name="parent">The unity objet for the ship part containing the hardpoint</param>
    /// <param name="descriptor">The XWOpt PartDescriptor for the part containing the hardpoint</param>
    /// <param name="hardpoint">The XWOpt Hardpoint causing the hardpoint in question to be created</param>
    public delegate void ProcessHardpointHandler(GameObject parent, PartDescriptor<Vector3> descriptor, Hardpoint<Vector3> hardpoint);

    /// <summary>
    /// Callback for game specific setup of TargetGroup objects.
    /// Parts with identical ID, type, and location are treated as the same part in game terms.
    /// </summary>
    /// <param name="targetGroup">The targeting group object.  All parts in the same target group are attached as children.</param>
    /// <param name="id">The ID of the targeting group.</param>
    /// <param name="type">The type of parts in the group.</param>
    /// <param name="location">The place on the model that is shown in the targeting window.</param>
    public delegate void ProcessTargetGroupHandler(GameObject targetGroup, int id, PartType type, Vector3 location);

    /// <summary>
    /// Reads OPT model and helps instantiate GameObjects based on useful data in the file.
    /// </summary>
    public class CraftFactory
    {
        public string FileName { get; private set; }

        /// <summary>
        /// The craft's root game object is cloned from this GameObject
        /// </summary>
        public GameObject CraftBase { get; set; }

        /// <summary>
        /// Part objects are cloned from this GameObject
        /// </summary>
        public GameObject PartBase { get; set; }

        /// <summary>
        /// Hardpoint objects are cloned from this GameObject
        /// </summary>
        public GameObject HardpointBase { get; set; }

        /// <summary>
        /// Targeting point objects are cloned from this GameObject
        /// </summary>
        public GameObject TargetPointBase { get; set; }

        /// <summary>
        /// If set, parts with the same target ID are grouped as children of this object.
        /// </summary>
        public GameObject TargetingGroupBase { get; set; }

        /// <summary>
        /// Callback for game specific setup of part objects after instantiation.
        /// </summary>
        public ProcessPartHandler ProcessPart { get; set; }

        /// <summary>
        /// Callback for game specific setup of hardpoint objects after instantiation.
        /// </summary>
        public ProcessHardpointHandler ProcessHardpoint { get; set; }

        /// <summary>
        /// Callback for game specific setup of targeting groups.
        /// </summary>
        public ProcessTargetGroupHandler ProcessTargetGroup { get; set; }

        /// <summary>
        /// The shader to use on the materials.  Default is Unity "Standard" shader.
        /// </summary>
        public Shader PartShader { get; set; } = Shader.Find("Standard");

        /// <summary>
        /// Overall size of the craft.  Used for LOD cutover.
        /// </summary>
        public float Size { get; private set; }

        OptFile<Vector2, Vector3> opt = new OptFile<Vector2, Vector3>();
        internal Dictionary<string, Material> materials;
        List<PartFactory> nonTargetGroupedParts = new List<PartFactory>();
        Dictionary<DistinctTargetGroupTuple, TargetGroupFactory> targetGroups = new Dictionary<DistinctTargetGroupTuple, TargetGroupFactory>();

        // XvT engine -> Unity engine
        // unity: forward is +z, right is +x,    up is +y
        // XvT:   forward is -y, right is +x(?), up is +z
        static readonly Matrix4x4 CoordinateConverter = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 0, -1, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 0, 1)
        );

        public CraftFactory(string fileName)
        {
            FileName = fileName;

            opt.RotateFromOptSpace = new CoordinateSystemConverter<Vector3>(RotateIntoUnitySpace);

            opt.Read(fileName);

            // Some models seem to share textures between parts by placing them at the top level.
            // So we need to gather all of the textures in the model
            // Making the assumption here that texture names are unique.
            materials = new Dictionary<string, Material>();
            foreach (var textureNode in opt.OfType<XWOpt.OptNode.Texture>())
            {
                materials.Add(
                    textureNode.Name,
                    new Material(PartShader)
                    {
                        name = textureNode.Name,
                        mainTexture = MakeUnityTexture(textureNode),
                    }
                );
            }

            // Determine total size of the craft.  Used for LOD size.
            bool foundDescriptor = false;
            Vector3 upperBound = new Vector3(); // upper bound
            Vector3 lowerBound = new Vector3(); // lower bound
            foreach (var descriptor in opt.OfType<PartDescriptor<Vector3>>())
            {
                var huc = descriptor.HitboxUpperCorner;
                var hlc = descriptor.HitboxLowerCorner;
                // In case origin is outside of all hitbox demensions,
                // set the bounds based on first hitbox found and expand from there.
                if (foundDescriptor)
                {
                    upperBound = Vector3.Max(upperBound, huc);
                    lowerBound = Vector3.Min(lowerBound, hlc);
                }
                else
                {
                    upperBound = new Vector3(huc.x, huc.y, huc.z);
                    lowerBound = new Vector3(hlc.x, hlc.y, hlc.z);
                    foundDescriptor = true;
                }
            }

            if (foundDescriptor)
            {
                Size = (upperBound - lowerBound).magnitude;
            }
            else
            {
                // TODO: Vertex based size calculation
                Size = float.PositiveInfinity;
            }

            foreach (NodeCollection shipPart in opt.RootNodes.OfType<NodeCollection>())
            {
                var factory = new PartFactory(this, shipPart);

                if (null == factory.descriptor || null == TargetingGroupBase)
                {
                    nonTargetGroupedParts.Add(factory);
                    factory.CreateChildTarget = true;
                }
                else
                {
                    var groupTuple = new DistinctTargetGroupTuple(factory.descriptor);

                    if (targetGroups.TryGetValue(groupTuple, out TargetGroupFactory group))
                    {
                        group.Add(factory);
                    }
                    else
                    {
                        group = new TargetGroupFactory(groupTuple, this);
                        group.Add(factory);
                        targetGroups.Add(groupTuple, group);
                    }
                }
            }
        }

        Vector3 RotateIntoUnitySpace(Vector3 v)
        {
            return CoordinateConverter * v;
        }

        /// <summary>
        /// Generates craft object based OPT model. This overload uses the default skin (0).
        /// </summary>
        public GameObject CreateCraftObject()
        {
            return CreateCraftObject(0);
        }

        /// <summary>
        /// Generates craft object based OPT model.
        /// </summary>
        /// <param name="skin">
        /// Which skin to use.  This is usually based on which squadron, EG Red, Blue, Gold, Alpha, Beta, etc.
        /// If the model has no skins, this is ignored.
        /// </param>
        public GameObject CreateCraftObject(int skin)
        {
            var craft = Object.Instantiate(CraftBase);

            foreach (var targetGroup in targetGroups)
            {
                Helpers.AttachTransform(craft, targetGroup.Value.CreateTargetGroup(skin));
            }

            foreach (var partFactory in nonTargetGroupedParts)
            {
                Helpers.AttachTransform(craft, partFactory.CreatePart(skin));
            }

            return craft;
        }

        Texture2D MakeUnityTexture(XWOpt.OptNode.Texture textureNode)
        {
            int pallet;

            // Generally higher pallet numbers are brighter.
            // Pick a brightness suitable for unity lighting
            switch (opt.Version)
            {
                case (2):
                    // TIE98 pallet 0-7 are 0xCDCD paddding. Pallet 8 is very dark, pallet 15 is normal level.
                    pallet = 15;
                    break;
                default:
                    // medium brigtness.  Pallet 15 is oversaturated.
                    pallet = 8;
                    break;
            }

            Texture2D texture = new Texture2D(textureNode.Width, textureNode.Height, TextureFormat.RGB565, false);

            texture.LoadRawTextureData(textureNode.ToRgb565(pallet));
            texture.Apply();

            return texture;
        }
    }
}
