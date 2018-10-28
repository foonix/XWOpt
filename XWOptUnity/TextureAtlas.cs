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

        // The lowest lighted pallet will have bright areas representing self-lighting.
        void RebalanceColorsForEmissive()
        {
            for (int i = 0; i < _albido.Length; i += 2)
            {
                if (_emissive[i] == 0 || _emissive[i + 1] == 0)
                    continue;

                // Unpack RGB565, low order byte first.
                Color albedo = new Color(
                    (_albido[i + 1] >> 3) / 31f,
                    (((_albido[i + 1] & 7) << 3) | (_albido[i] >> 5)) / 63f,
                    (_albido[i] & 0x1F) / 31f
                    );

                Color emissive = new Color(
                    (_emissive[i + 1] >> 3) / 31f,
                    (((_emissive[i + 1] & 7) << 3) | (_emissive[i] >> 5)) / 63f,
                    (_emissive[i] & 0x1F) / 31f
                    );

                // Lowest brightness palette has too much ambient light to make a good emissive texture.
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
                _albido[i + 1] = (byte)((a_r << 3) | (a_g >> 3));
                _albido[i] = (byte)(((a_g & 0x1F) << 5) | a_b);

                byte e_r = (byte)(emissive.r * 31f);
                byte e_g = (byte)(emissive.g * 63f);
                byte e_b = (byte)(emissive.b * 31f);
                _emissive[i + 1] = (byte)((e_r << 3) | (e_g >> 3));
                _emissive[i] = (byte)(((e_g & 0x1F) << 5) | e_b);
            }
        }
    }
}
