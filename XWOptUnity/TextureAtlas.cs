using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    internal class TextureAtlas
    {
        /// <summary>
        /// Index by name into AtlasLocations
        /// </summary>
        internal Dictionary<string, int> TextureId { get; private set; } = new Dictionary<string, int>();

        /// <summary>
        /// Rect transform of where in the atlas the original texture was placed.
        /// </summary>
        internal Rect[] AtlasLocations { get; private set; }

        internal Texture2D AlbidoAtlas { get; private set; }
        internal Texture2D EmissiveAtlas { get; private set; }

        internal Material Material { get; private set; }

        public TextureAtlas(List<TextureCacheEntry> textureCache, Shader shader, string name)
        {
            var atlasRects = new List<Rect>();

            //if (!Texture2D.GenerateAtlas(textureCache.Select(t => t.Size).ToArray(), 0, 0, atlasRects))
            //{
            //    throw new BadImageFormatException("Texture2D.GenerateAtlas() failed.");
            //}

            AlbidoAtlas = new Texture2D(0, 0);
            EmissiveAtlas = new Texture2D(0, 0);

            var albidoAndEmissive = textureCache.Select(t => t.MakeTextures()).ToArray();

            var albidoRects = AlbidoAtlas.PackTextures(albidoAndEmissive.Select(t => t.albido).ToArray(), 0);
            var emissiveRects = EmissiveAtlas.PackTextures(albidoAndEmissive.Select(t => t.emissive).ToArray(), 0);

            if (!Enumerable.SequenceEqual(albidoRects, emissiveRects))
            {
                throw new InvalidOperationException("Texture2D.PackTextures is not deterministic!");
            }

            AtlasLocations = albidoRects;
            for(int i = 0; i < textureCache.Count; i++)
            {
                TextureId[textureCache[i].Name] = i;
            }

            Material = new Material(shader)
            {
                mainTexture = AlbidoAtlas,
                name = name,
            };

            // Enable emission in the standard shader.
            if (null != EmissiveAtlas)
            {
                Material.EnableKeyword("_EMISSION");
                Material.SetTexture("_EmissionMap", EmissiveAtlas);
                Material.SetColor("_EmissionColor", Color.white);
                Material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
        }
    }
}
