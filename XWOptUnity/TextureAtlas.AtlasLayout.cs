using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    internal partial class TextureAtlas
    {
        /// <summary>
        /// Determines layout and padding based on available mips, leaving room to prevent
        /// mip bleed texture filtering issues.
        /// </summary>
        internal class AtlasLayout : IBakeable
        {
            public int Mips { get; } = 3;
            public int Padding { get { return Mips > 1 ? 1 << Mips : 2; } }

            /// <summary>
            /// Index by name into AtlasLocations
            /// </summary>
            public Dictionary<string, int> TextureId { get; } = new Dictionary<string, int>();

            /// <summary>
            /// Rect transform of where in the atlas the original texture was placed.
            /// </summary>
            public List<Rect> AtlasLocations { get; } = new List<Rect>();

            public int SizeX { get; private set; }
            public int SizeY { get; private set; }

            // Avoid trying atlas sizes that are unlikely to hold anything.
            // texture dimensions must be powers of two.
            const int minimumSize = 7;  // 128
            // avoid huge texture
            // unity limit is 12 (4096)
            const int maximumSize = 11;  // 2048

            public AtlasLayout(List<XWOpt.OptNode.Texture> textures)
            {
                Vector2[] sizes = new Vector2[textures.Count];
                for (int i = 0; i < textures.Count; i++)
                {
                    sizes[i] = new Vector2(textures[i].Width, textures[i].Height);
                    TextureId[textures[i].Name] = i;
                }

                int atlasSize = minimumSize;
                for (; atlasSize <= maximumSize; atlasSize++)
                {
                    int size = 1 << atlasSize;

                    // Leave padding space on top/right sides for tiling from bottom/left textures
                    Texture2D.GenerateAtlas(sizes, Padding, size - Padding, AtlasLocations);

                    // GenerateAtlas seems to return 0'd vectors if they didn't fit.
                    if (AtlasLocations.Any(v => v.y > 0 || v.x > 0))
                    {
                        break;
                    }
                }

                if (AtlasLocations.Count == 0)
                {
                    throw new InvalidOperationException("Could not pack atlas textures within texture limits.");
                }

                SizeY = SizeX = 1 << atlasSize;

                // If Texture2D.GenerateAtlas() happened to pack everything in only the bottom or left halves of the texture, then memory use can be trimmed %50.
                if (!AtlasLocations.Any(l => l.xMax + Padding > SizeX / 2))
                {
                    SizeX /= 2;
                }
                else if (!AtlasLocations.Any(l => l.yMax + Padding > SizeY / 2))
                {
                    SizeY /= 2;
                }
            }

            public void ParallelizableBake(int? degreesOfParallelism) { }
            public void MainThreadBake() { }

            public Rect GetPixelLocation(string name)
            {
                return AtlasLocations[TextureId[name]];
            }

            public Rect GetUvLocation(int id)
            {
                Rect pixelLocation = AtlasLocations[id];

                return new Rect(
                    pixelLocation.x / SizeX,
                    pixelLocation.y / SizeY,
                    pixelLocation.width / SizeX,
                    pixelLocation.height / SizeY
                );
            }
        }
    }
}
