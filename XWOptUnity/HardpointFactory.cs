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

using SchmooTech.XWOpt.OptNode;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    class HardpointFactory
    {
        CraftFactory Craft { get; set; }

        internal HardpointFactory(CraftFactory part)
        {
            Craft = part;
        }

        internal GameObject MakeHardpoint(GameObject parent, Hardpoint<Vector3> hardpointNode, PartDescriptor<Vector3> partDescriptor)
        {
            var hardpointObj = Object.Instantiate(Craft.HardpointBase) as GameObject;
            hardpointObj.name = hardpointNode.WeaponType.ToString();
            Helpers.AttachTransform(parent, hardpointObj, hardpointNode.Location);

            Craft.ProcessHardpoint?.Invoke(hardpointObj, partDescriptor, hardpointNode);

            return hardpointObj;
        }
    }
}