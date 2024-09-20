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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SchmooTech.XWOptUnity
{
    [RequireComponent(typeof(MeshFilter))]
    public class SkinMeshSwitch : MonoBehaviour
    {
        public Mesh [] _skins;
        internal MeshFilter _meshFilter;

        public void Initialise(Mesh [] skins)
        {
            _skins = skins;
            if (!didAwake)
            {
                Awake();
            }
        }

        public void Awake()
        {
            if (_meshFilter == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }
        }

        public void SwitchSkin(int skin)
        {
            if (!didAwake)
            {
                Awake();
            }

            // Lower LODs may not have skin specific textures even if higher LODs do.
            if (skin >= _skins.Length)
            {
                skin = 0;
            }

            _meshFilter.sharedMesh = _skins[skin];
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SkinMeshSwitch))]
    public class SkinMeshSwitchEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var skinMeshFilter = target as SkinMeshSwitch;

            if (skinMeshFilter._skins != null)
            {
                for (var i = 0; i < skinMeshFilter._skins.Length; i++)
                {
                    var skin = skinMeshFilter._skins[i];
                    if (GUILayout.Button($"Skin {i}: {skin.name}"))
                    {
                        skinMeshFilter.SwitchSkin(i);
                    }
                }
            }

            base.OnInspectorGUI();
        }
    }
#endif
}
