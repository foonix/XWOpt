﻿/*
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
using System.IO;
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
    /// <param name="descriptor">The XWOpt rotation information associated with the part</param>
    public delegate void ProcessPartHandler(GameObject part, PartDescriptor<Vector3> descriptor, RotationInfo<Vector3> rotationInfo);

    /// <summary>
    /// Callback for game specific setup of hardpoint objects after instantiation.
    /// Use this to modify or filter hardpoints, eg connect them to the game's firing system, etc.
    /// </summary>
    /// <param name="hardpoint">The unity objet for the ship part containing the hardpoint</param>
    /// <param name="descriptor">The XWOpt PartDescriptor for the part containing the hardpoint</param>
    /// <param name="optHardpoint">The XWOpt Hardpoint causing the hardpoint in question to be created</param>
    public delegate void ProcessHardpointHandler(GameObject hardpoint, PartDescriptor<Vector3> descriptor, Hardpoint<Vector3> optHardpoint);

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
        public Shader PartShader
        {
            get
            {
                return partShader ?? (partShader = Shader.Find("Standard"));
            }
            set
            {
                partShader = value;
                NeedsBake = true;
            }
        }
        private Shader partShader;

        /// <summary>
        /// Distance between opposite corners of the box encompasing the craft.  Used for LOD cutover.
        /// </summary>
        public float Size { get; private set; }

        /// <summary>
        /// Generate emissive textures using low light level pallets.
        /// This allows things like engines and windows to "glow in the dark."
        /// Note this will cause the lowest possible darkness to be clamped at the original game's lowest level.
        /// </summary>
        public bool MakeEmissiveTexture
        {
            get
            {
                return makeEmissiveTexture;
            }
            set
            {
                makeEmissiveTexture = value;
                NeedsBake = true;
            }
        }
        private bool makeEmissiveTexture = true;


        /// <summary>
        /// Exponent for darkening emissive textures.
        /// Set high enough to make the dark parts nearly black while leaving emissive parts bright.
        /// </summary>
        public float EmissiveExponent
        {
            get
            {
                return emissiveExponent;
            }
            set
            {
                emissiveExponent = value;
                NeedsBake = true;
            }
        }
        private float emissiveExponent = 2f;

        public OptFile<Vector2, Vector3> Opt { get; private set; } = new OptFile<Vector2, Vector3>
        {
            //Logger = msg => Debug.Log(msg),
            RotateFromOptSpace = new CoordinateSystemConverter<Vector3>(RotateIntoUnitySpace)
        };

        /// <summary>
        /// True if the baking process has not completed or any setting has been changed that would affect the baking process.
        /// </summary>
        public bool NeedsBake { get; private set; } = true;

        internal Dictionary<string, TextureCacheEntry> unpackedTextures = new Dictionary<string, TextureCacheEntry>();
        internal Dictionary<string, Material> materials = new Dictionary<string, Material>();
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
        ) * Matrix4x4.Scale(new Vector3(ScaleFactor, ScaleFactor, ScaleFactor));

        // Size conversion between OPT coordinates and Unity
        public const float ScaleFactor = 0.0244140625f;

        public CraftFactory(string fileName)
        {
            FileName = fileName;

            Opt.Read(fileName);

            CraftFactoryImpl();
        }

        public CraftFactory(Stream stream)
        {
            FileName = stream.ToString();

            Opt.Read(stream);

            CraftFactoryImpl();
        }

        private void CraftFactoryImpl()
        {
            // Determine total size of the craft.  Used for LOD size.
            bool foundDescriptor = false;
            Vector3 upperBound = new Vector3(); // upper bound
            Vector3 lowerBound = new Vector3(); // lower bound
            foreach (var descriptor in Opt.OfType<PartDescriptor<Vector3>>())
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

            foreach (NodeCollection shipPart in Opt.RootNodes.OfType<NodeCollection>())
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

        /// <summary>
        /// Perform as much computationally expensive conversion work as pracitcal.
        /// 
        /// This does not use the Unity API and is safe to call outside of the unity main thread.
        /// </summary>
        public void Bake()
        {
            materials.Clear();
            unpackedTextures.Clear();

            foreach (var textureNode in Opt.OfType<XWOpt.OptNode.Texture>())
            {
                unpackedTextures.Add(
                    textureNode.Name,
                    new TextureCacheEntry(textureNode, Opt.Version, makeEmissiveTexture ? emissiveExponent : (float?)null)
                );
            }

            NeedsBake = false;
        }

        static Vector3 RotateIntoUnitySpace(Vector3 v)
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
        /// 
        /// If NeedsBake is true, this will trigger bake().
        /// </summary>
        /// <param name="skin">
        /// Which skin to use.  This is usually based on which squadron, EG Red, Blue, Gold, Alpha, Beta, etc.
        /// If the model has no skins, this is ignored.
        /// </param>
        public GameObject CreateCraftObject(int skin)
        {
            if (NeedsBake)
            {
                Bake();
            }

            // Some models seem to share textures between parts by placing them at the top level.
            // So we need to gather all of the textures in the model
            // Making the assumption here that texture names are unique.
            materials = new Dictionary<string, Material>();
            foreach (var textureCacheEntry in unpackedTextures)
            {
                if (!materials.ContainsKey(textureCacheEntry.Key))
                    materials.Add(textureCacheEntry.Key, textureCacheEntry.Value.MakeMaterial(PartShader));
            }

            var craft = Object.Instantiate(CraftBase);

            foreach (var targetGroup in targetGroups)
            {
                Helpers.AttachTransform(craft, targetGroup.Value.CreateTargetGroup(skin));
            }

            foreach (var partFactory in nonTargetGroupedParts)
            {
                partFactory.CreatePart(craft, skin);
            }

            return craft;
        }
    }
}
