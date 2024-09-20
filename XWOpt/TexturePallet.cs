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

namespace SchmooTech.XWOpt.OptNode
{
    /// <summary>
    /// Collection of 16 RGB565 texture pallets, with 256 colors per pallet.
    /// Unused pallets are padded with 0xCDCD
    /// By convention, each pallet is increasing in brightness levels from the previous pallet.
    /// </summary>
    public class TexturePallet
    {
        // Number of pallets is implicit in the format specifications.
        const int PalletCount = 16;
        // Number of colors for each pallet fixed in the format specs.
        const int ColorCount = 256;

        readonly byte[,] eightBitPalettes = new byte[PalletCount, ColorCount];  // Colour Index for 256 Colour
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        readonly ushort[,] sixteenBitPalettes = new ushort[PalletCount, ColorCount]; // R5G6B5 for 16 bit Colour

        public TexturePallet() { }

        internal TexturePallet(OptReader reader)
        {
            for (int i = 0; i < PalletCount; i++)
            {
                for (int j = 0; j < ColorCount; j++)
                {
                    eightBitPalettes[i, j] = reader.ReadByte();
                }
            }

            // For some reason pallets 0-7 seem to be padding, 8-15 appear to be increasing brightness
            for (int i = 0; i < PalletCount; i++)
            {
                for (int j = 0; j < ColorCount; j++)
                {
                    sixteenBitPalettes[i, j] = reader.ReadUInt16();
                }
            }
        }

        // An indexer *is* a method. =P
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
        [CLSCompliant(false)]
        public ushort this[int palletNumber, int which]
        {
            get { return GetValue(palletNumber, which); }
            set { SetValue(palletNumber, which, value); }
        }

        [CLSCompliant(false)]
        public void SetValue(int palletNumber, int which, ushort color)
        {
            BoundsCheck(palletNumber, which);

            sixteenBitPalettes[palletNumber, which] = color;
        }

        [CLSCompliant(false)]
        public ushort GetValue(int palletNumber, int which)
        {
            BoundsCheck(palletNumber, which);

            return sixteenBitPalettes[palletNumber, which];
        }

        private static void BoundsCheck(int palletNumber, int which)
        {
            if (palletNumber < 0 || palletNumber >= PalletCount)
            {
                throw new ArgumentException("Pallet number out of range");
            }

            if (which < 0 || which >= ColorCount)
            {
                throw new ArgumentException("Color index out of range");
            }
        }
    }
}
