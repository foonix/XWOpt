using System.Collections.Generic;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    internal partial class TextureAtlas : IBakeable
    {
        internal Material Material { get; private set; }
        internal AtlasLayout Layout { get; private set; }

        Texture2D albidoAtlas;
        Texture2D emissiveAtlas;

        string name;
        private int optVersion;
        private float? emissiveExponent;
        Shader shader;
        List<XWOpt.OptNode.Texture> textures;

        byte[] _albido;
        byte[] _emissive;

        public TextureAtlas(List<XWOpt.OptNode.Texture> textures, Shader shader, string name, int optVersion, float? emissiveExponent)
        {
            this.textures = textures;
            this.shader = shader;
            this.name = name;
            this.optVersion = optVersion;
            this.emissiveExponent = emissiveExponent;

            Layout = new AtlasLayout(textures);
        }

        public void ParallelizableBake(int? degreesOfParallelism)
        {
            Layout.ParallelizableBake(degreesOfParallelism);

            _albido = new byte[Layout.SizeX * Layout.SizeY * 2];
            PopulateAtlas(_albido, VersionSpecificPaletteNumber(false));
            if (emissiveExponent.HasValue)
            {
                _emissive = new byte[Layout.SizeX * Layout.SizeY * 2];
                PopulateAtlas(_emissive, VersionSpecificPaletteNumber(true));
                RebalanceColorsForEmissive();
            }
        }

        public void MainThreadBake()
        {
            albidoAtlas = new Texture2D(Layout.SizeX, Layout.SizeY, TextureFormat.RGB565, false, true)
            {
                name = name,
            };

            albidoAtlas.LoadRawTextureData(_albido);
            albidoAtlas.Apply();

            Material = new Material(shader)
            {
                mainTexture = albidoAtlas,
                name = name,
                enableInstancing = true,
            };

            if (null != _emissive)
            {
                emissiveAtlas = new Texture2D(Layout.SizeX, Layout.SizeY, TextureFormat.RGB565, false, true)
                {
                    name = name,
                };
                emissiveAtlas.LoadRawTextureData(_emissive);
                emissiveAtlas.Apply();

                Material.EnableKeyword("_EMISSION");
                Material.SetTexture("_EmissionMap", emissiveAtlas);
                Material.SetColor("_EmissionColor", Color.white);
                Material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
        }

        void PopulateAtlas(byte[] target, int palette)
        {
            foreach (var texture in textures)
            {
                Rect location = Layout.GetPixelLocation(texture.Name);

                int margin = Layout.Padding / 2;
                texture.BlitRangeInto(
                    target: target,
                    targetWidth: Layout.SizeX,
                    targetHeight: Layout.SizeY,
                    sourceX: -margin,
                    sourceY: -margin,
                    targetX: Mathf.RoundToInt(location.x - margin),
                    targetY: Mathf.RoundToInt(location.y - margin),
                    sizeX: Mathf.RoundToInt(location.width + margin * 2),
                    sizeY: Mathf.RoundToInt(location.height + margin * 2),
                    palletNumber: palette,
                    mipLevel: 0
                );
            }
        }

        int VersionSpecificPaletteNumber(bool emissive)
        {
            int palette;

            // Generally higher pallet numbers are brighter.
            // Pick a brightness suitable for unity lighting
            switch (optVersion)
            {
                case 1:
                case 2:
                    if (emissive)
                    {
                        palette = 0;
                    }
                    else
                    {
                        palette = 8;
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

        void UnpackColourR5G6B5(byte[] data, int offset, out float red, out float green, out float blue)
        {
            // Unpack RGB565, low order byte first.
            red = (data[offset + 1] >> 3) / 31f;
            green = (((data[offset + 1] & 7) << 3) | (data[offset] >> 5)) / 63f;
            blue = (data[offset] & 0x1F) / 31f;
        }

        Color UnpackColourR5G6B5(byte[] data, int offset)
        {
            UnpackColourR5G6B5(data, offset, out var red, out var green, out var blue);
            return new Color(red, green, blue);
        }

        void PackColourR5G6B5(Color rgb, byte[] data, int offset)
        {
            byte a_r = (byte)(rgb.r * 31f);
            byte a_g = (byte)(rgb.g * 63f);
            byte a_b = (byte)(rgb.b * 31f);

            data[offset + 1] = (byte)((a_r << 3) | (a_g >> 3));
            data[offset] = (byte)(((a_g & 0x1F) << 5) | a_b);
        }

        // The lowest lighted pallet will have bright areas representing self-lighting.
        void RebalanceColorsForEmissive()
        {
            for (int i = 0; i < _albido.Length; i += 2)
            {
                if (_emissive[i] == 0 && _emissive[i + 1] == 0)
                    continue;

                Color minLighting = UnpackColourR5G6B5(_emissive, i);
                Color maxLighting = UnpackColourR5G6B5(_albido, i);

                // Repack RGB565
                // Quick and dirty heuristic to find emissive pixels. Must be 25% lit at maximum lighting (stops shadows being emissive),
                // and must not change more than 4% across all light levels (stops normal non-emissive pixels)
                if (maxLighting.maxColorComponent > 0.25f && (maxLighting - minLighting).maxColorComponent < 0.04f)
                {
                    PackColourR5G6B5(Color.black, _albido, i);
                    PackColourR5G6B5(minLighting, _emissive, i);
                }
                else
                {
                    PackColourR5G6B5(maxLighting, _albido, i);
                    PackColourR5G6B5(Color.black, _emissive, i);
                }
            }
        }
    }
}
