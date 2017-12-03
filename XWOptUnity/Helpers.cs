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

using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    internal static class Helpers
    {
        /// <summary>
        /// Attaches a part to a parent object on the origin.
        /// All verticies in the same craft share the same origin.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="child">The GameObject to attach to parent</param>
        internal static void AttachTransform(GameObject parent, GameObject child)
        {
            AttachTransform(parent, child, new Vector3(0, 0, 0));
        }

        /// <summary>
        /// Attaches a part to a parent object at a given offset.
        /// All verticies in the same craft share the same origin.
        /// </summary>
        /// <param name="parent">Parent GameObject</param>
        /// <param name="child">The GameObject to attach to parent</param>
        /// <param name="location">Transform location of the child object</param>
        internal static void AttachTransform(GameObject parent, GameObject child, Vector3 location)
        {
            Transform childTransform = child.GetComponent<Transform>();
            childTransform.parent = parent.transform;
            childTransform.localPosition = location;
            childTransform.localRotation = new Quaternion(0, 0, 0, 0);
        }
    }
}
