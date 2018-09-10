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
using System.Collections.Generic;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    internal class TargetGroupFactory
    {
        int Id { get; set; }
        PartType Type { get; set; }
        Vector3 Location { get; set; }

        List<PartFactory> parts = new List<PartFactory>();
        CraftFactory Craft { get; set; }

        TargetPointFactory targetPointFactory;

        internal TargetGroupFactory(DistinctTargetGroupTuple groupTuple, CraftFactory craft)
        {
            Id = groupTuple.id;
            Type = groupTuple.type;
            Location = groupTuple.location;
            Craft = craft;
            targetPointFactory = new TargetPointFactory(craft, groupTuple);
        }

        internal void Add(PartFactory part)
        {
            parts.Add(part);
        }

        internal GameObject CreateTargetGroup(int skin)
        {
            var targetGroup = Object.Instantiate(Craft.TargetingGroupBase);
            targetGroup.name = Type.ToString() + " Target Group";

            foreach (var partFactory in parts)
            {
                partFactory.CreatePart(targetGroup, skin);
            }

            Craft.ProcessTargetGroup?.Invoke(targetGroup, Id, Type, Location);

            Helpers.AttachTransform(targetGroup, targetPointFactory.CreateTargetPoint(), Location);

            return targetGroup;
        }
    }
}
