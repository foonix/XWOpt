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
        // Not sure what this number means.  Seems to be the same on most textures inside of the same OPT.
        private int group;
        private string name;
        private int mipLevels = 0;

        private byte[] texturePalletRefs;
        private Collection<byte[]> mipPalletRefs = new Collection<byte[]>();

        // 16 pallets at decreasing light levels.
        // colors packed 5-6-5 blue, green, red.
        private readonly TexturePallet pallet = new TexturePallet();
        private int width;
        private int height;

        public int Group { get => group; set => group = value; }
        public string Name { get => name; set => name = value; }
        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public int MipLevels { get => mipLevels; set => mipLevels = value; }
        public Collection<byte[]> MipPalletRefs { get => mipPalletRefs; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        [CLSCompliant(false)]
        public TexturePallet Pallet { get => pallet; }

        internal Texture(OptReader reader, int textureNameOffset) : base(reader)
        {
            // TODO: Check for alpha channel 0 26 node.
            reader.ReadUnknownUseValue(0, this);

            group = reader.ReadInt32();

            var palletAddressOffset = reader.ReadInt32();

            reader.Seek(textureNameOffset);
            name = reader.ReadString(9);

            reader.SeekShouldPointHere(palletAddressOffset, this);

            // Skip pallet data for now to get pallet references.
            var palletOffset = reader.ReadInt32();

            reader.ReadUnknownUseValue(0, this);

            int size = reader.ReadInt32();
            int sizeWithMips = reader.ReadInt32();

            width = reader.ReadInt32();
            height = reader.ReadInt32();

            texturePalletRefs = new byte[size];
            reader.Read(texturePalletRefs, 0, size);

            // Read Mip data
            // Not sure we need these.
            int nextMipWidth = width / 2, nextMipHeight = height / 2;
            int mipSize = nextMipWidth * nextMipHeight;
            int mipDataToRead = sizeWithMips - size;
            while (mipDataToRead >= mipSize && mipSize > 0)
            {
                byte[] nextMipRefs = new byte[mipSize];
                reader.Read(nextMipRefs, 0, mipSize);
                mipPalletRefs.Add(nextMipRefs);
                mipLevels++;

                nextMipWidth = nextMipWidth / 2;
                nextMipHeight = nextMipHeight / 2;
                mipDataToRead -= mipSize;
                mipSize = nextMipWidth * nextMipHeight;
            }

            // Now go back and find the texture pallet.
            // A few files have invalid palletOffsets.
            if (palletOffset > reader.globalOffset)
            {
                pallet = reader.ReadPalette(palletOffset);
            }
        }

        /// <summary>
        /// Generates RGB565 image from pallet and color data.
        /// </summary>
        /// <param name="palletNumber">Which pallet to use when generating the image (0-15)</param>
        /// <returns>byte[] containing the image, in bottom left to top right order.</returns>
        public byte[] ToRgb565(int palletNumber)
        {
            var img = new Byte[texturePalletRefs.Length * 2];

            for (int i = 0; i < texturePalletRefs.Length; i++)
            {
                ushort color = pallet[palletNumber, texturePalletRefs[i]];

                img[i * 2] = (byte)(color & 0xFF);  // low order byte
                img[(i * 2) + 1] = (byte)(color >> 8);  // high order byte
            }

            return img;
        }
    }
}
