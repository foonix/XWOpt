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

using System.Collections.Generic;

namespace SchmooTech.XWOpt.OptNode
{
    public class Texture : BaseNode
    {
        public int uid;
        public string name;
        public int width, height, mipLevels = 0;

        public byte[] texturePalletRefs;
        public List<byte[]> mipPalletRefs = new List<byte[]>();

        // 16 pallets at decreasing light levels.
        // colors packed 5-6-5 blue, green, red.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        public ushort[,] pallet;

        internal Texture(OptReader reader, int textureNameOffset) : base(reader)
        {
            // TODO: Check for alpha channel 0 26 node.
            reader.ReadUnknownUseValue(0, this);

            uid = reader.ReadInt32();

            // TODO: Detect pallet reuse.
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
            reader.Seek(palletOffset);
            // Always 16 pallets, 256 colors each
            // For some reason pallets 0-7 seem to be padding, 8-15 appear to be increasing brightness
            pallet = new ushort[16, 256];
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    pallet[i, j] = reader.ReadUInt16();
                }
            }
        }
    }
}
