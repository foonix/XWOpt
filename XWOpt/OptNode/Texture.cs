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

using System;
using System.Collections.ObjectModel;

namespace SchmooTech.XWOpt.OptNode
{
    public class Texture : BaseNode
    {
        private Collection<byte[]> mipPalletRefs = new Collection<byte[]>();

        // 16 pallets at decreasing light levels.
        // colors packed 5-6-5 blue, green, red.
        private readonly TexturePallet pallet = new TexturePallet();

        public int Width { get; }
        public int Height { get; }
        public int MipLevels { get; }
        public Collection<byte[]> MipPalletRefs { get => mipPalletRefs; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        [CLSCompliant(false)]
        public TexturePallet Pallet { get => pallet; }

        const int Rgb565ByteWidth = 2;

        internal Texture(OptReader reader, NodeHeader nodeHeader) : base(reader, nodeHeader)
        {
            reader.Seek(nodeHeader.DataAddress);

            var paletteOffset = reader.ReadInt32();
            var paletteSize = reader.ReadInt32();
            var textureSizeIncludingMips = reader.ReadInt32();
            var completeSize = reader.ReadInt32();
            
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();

            var expectedSize = Height * Width;

            var finalPaletteOffset = paletteOffset;

            if (paletteSize != 0)
            {
                if (expectedSize == textureSizeIncludingMips)
                {
                    finalPaletteOffset = nodeHeader.DataAddress + 24 + completeSize;
                }
                else
                {
                    finalPaletteOffset = nodeHeader.DataAddress + 24 + expectedSize;
                }
            }

            var nextMipImageSize = Width * Height / 4;
            MipLevels = 1;
            while (nextMipImageSize != 0)
            {
                MipLevels++;
                nextMipImageSize /= 4;
            }

            var mipSize = Width * Height;

            for (var mip = 0; mip < MipLevels; mip++)
            {
                byte[] nextMipRefs = new byte[mipSize];
                reader.Read(nextMipRefs, 0, mipSize);
                mipPalletRefs.Add(nextMipRefs);
            }

            // A few files have invalid palletOffsets.
            if (finalPaletteOffset > reader.globalOffset)
            {
                pallet = reader.ReadPalette(finalPaletteOffset);
            }
        }

        /// <summary>
        /// Copy range of RGB565 color values from the texture into <paramref name="target"/>,
        /// dereferencing palette references.
        ///
        /// If the volume to be blitted exceeds either the texture or target area, colors will be wrapped to the other side.
        /// </summary>
        /// <param name="target">Buffer to blit into</param>
        /// <param name="targetWidth">Pixel width of target buffer. (1 pixel = 2 bytes)</param>
        /// <param name="sourceX">Bottom left X pixel coordinate of the texture area to blit</param>
        /// <param name="sourceY">Bottom left Y pixel coordinate of the texture area to blit</param>
        /// <param name="targetX">Bottom left X pixel coordinate to recieve color values</param>
        /// <param name="targetY">Bottom left Y pixel coordinate to recieve color values</param>
        /// <param name="sizeX">Pixel width of area to blit</param>
        /// <param name="sizeY">Pixel height of area to blit</param>
        /// <param name="palletNumber">Which palette to use</param>
        /// <param name="mipLevel">Texture mip level to blit from.  Source and size must be in the bounds of the mip level.</param>
        public void BlitRangeInto(byte[] target, int targetWidth, int targetHeight, int sourceX, int sourceY, int targetX, int targetY, int sizeX, int sizeY, int palletNumber, int mipLevel = 0)
        {
            for (int y = 0; y < sizeY; y++)
            {
                // positive wrap
                int tY = (targetY + y) % targetHeight;
                int sY = (sourceY + y) % Height;
                // negative wrap
                tY = tY < 0 ? targetHeight + tY : tY;
                sY = sY < 0 ? Height + sY : sY;

                for (int x = 0; x < sizeX; x++)
                {
                    // positive wrap
                    int tX = (targetX + x) % targetWidth;
                    int sX = (sourceX + x) % Width;
                    // negative wrap
                    tX = tX < 0 ? targetWidth + tX : tX;
                    sX = sX < 0 ? Width + sX : sX;

                    // target byte
                    int t = (Rgb565ByteWidth * tX) + (Rgb565ByteWidth * targetWidth * tY);

                    // source palette ref
                    int sr = sX + (Width * sY);

                    ushort color = pallet[palletNumber, mipPalletRefs[mipLevel][sr]];

                    target[t] = (byte)(color & 0xFF);  // low order byte
                    target[t + 1] = (byte)(color >> 8);  // high order byte
                }
            }
        }
    }
}
