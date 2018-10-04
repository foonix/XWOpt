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
    /// <summary>
    /// Contains both the albido and emissive texture bytes to be loaded into a material.
    /// </summary>
    internal class TextureCacheEntry
    {
        internal string Name { get; private set; }
        private TextureFormat AlbidoFormat { get; set; } = TextureFormat.RGB565;
        private TextureFormat EmissiveFormat { get; set; } = TextureFormat.RGB565;

        private readonly byte[] rawAlbedo;
        private readonly byte[] rawEmissive;

        private XWOpt.OptNode.Texture textureNode;
        private readonly int optVersion;

        /// <summary>
        /// Generate albido and emssive textures.
        /// </summary>
        /// <param name="textureNode">If set, generates an emissive texture.</param>
        /// <param name="optVersion">OPT file version</param>
        /// <param name="emissiveExponent">Darkening bias for emissive texture generation.</param>
        internal TextureCacheEntry(XWOpt.OptNode.Texture textureNode, int optVersion, float? emissiveExponent)
        {
            this.textureNode = textureNode;
            this.optVersion = optVersion;
            Name = textureNode.Name;

            rawAlbedo = textureNode.ToRgb565(VersionSpecificPaletteNumber(false));

            if (emissiveExponent.HasValue)
            {
                // The lowest lighted pallet will have bright areas representing self-lighting.
                // So use a texture generated from that pallet as an emissive texture.
                rawEmissive = textureNode.ToRgb565(VersionSpecificPaletteNumber(true));

                for (int i = 0; i < rawAlbedo.Length; i += 2)
                {
                    // Unpack RGB565, low order byte first.
                    Color albedo = new Color(
                        (rawAlbedo[i + 1] >> 3) / 31f,
                        (((rawAlbedo[i + 1] & 7) << 3) | (rawAlbedo[i] >> 5)) / 63f,
                        (rawAlbedo[i] & 0x1F) / 31f
                        );

                    Color emissive = new Color(
                        (rawEmissive[i + 1] >> 3) / 31f,
                        (((rawEmissive[i + 1] & 7) << 3) | (rawEmissive[i] >> 5)) / 63f,
                        (rawEmissive[i] & 0x1F) / 31f
                        );

                    // Lowest brightness pallet has too much ambient light to make a good emissive texture.
                    // So we try to squash the ambient part without reducing brightness of the emissive features too much.
                    emissive.r = Mathf.Pow(emissive.r, emissiveExponent.Value);
                    emissive.g = Mathf.Pow(emissive.g, emissiveExponent.Value);
                    emissive.b = Mathf.Pow(emissive.b, emissiveExponent.Value);

                    // The material will be oversaturated if the emissive layer is simply layerd over the main texture.
                    // So reduce the albedo by the emissive part.
                    albedo -= emissive;

                    // Repack RGB565
                    byte a_r = (byte)(albedo.r * 31f);
                    byte a_g = (byte)(albedo.g * 63f);
                    byte a_b = (byte)(albedo.b * 31f);
                    rawAlbedo[i + 1] = (byte)((a_r << 3) | (a_g >> 3));
                    rawAlbedo[i] = (byte)(((a_g & 0x1F) << 5) | a_b);

                    byte e_r = (byte)(emissive.r * 31f);
                    byte e_g = (byte)(emissive.g * 63f);
                    byte e_b = (byte)(emissive.b * 31f);
                    rawEmissive[i + 1] = (byte)((e_r << 3) | (e_g >> 3));
                    rawEmissive[i] = (byte)(((e_g & 0x1F) << 5) | e_b);
                }
            }
        }

        int VersionSpecificPaletteNumber(bool emissive)
        {
            int palette;

            // Generally higher pallet numbers are brighter.
            // Pick a brightness suitable for unity lighting
            switch (optVersion)
            {
                case (1):
                case (2):
                    // TIE98/Xwing98/XvT pallet 0-7 are 0xCDCD paddding. Pallet 8 is very dark, pallet 15 is normal level.
                    if (emissive)
                    {
                        palette = 8;
                    }
                    else
                    {
                        palette = 15;
                    }
                    break;
                default:
                    // XWA pallet 8 is medium brigtness.  Pallet 15 is oversaturated.
                    if (emissive)
                    {
                        palette = 0;
                    }
                    else
                    {
                        palette = 8;
                    }
                    break;
            }

            return palette;
        }

        internal Material MakeMaterial(Shader shader)
        {
            var material = new Material(shader)
            {
                name = textureNode.Name,
            };

            var Albido = new Texture2D(textureNode.Width, textureNode.Height, AlbidoFormat, false);
            Albido.LoadRawTextureData(rawAlbedo);
            Albido.Apply();
            material.mainTexture = Albido;

            if (null != rawEmissive)
            {
                var emissive = new Texture2D(textureNode.Width, textureNode.Height, EmissiveFormat, false);
                emissive.LoadRawTextureData(rawEmissive);
                emissive.Apply();
                // Enable emission in the standard shader.
                material.EnableKeyword("_EMISSION");
                material.SetTexture("_EmissionMap", emissive);
                material.SetColor("_EmissionColor", Color.white);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            material.SetFloat("_Glossiness", 0.1f);

            return material;
        }
    }
}