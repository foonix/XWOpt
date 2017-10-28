using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SchmooTech.XWOpt.OptNode
{
    [CLSCompliant(false)]
    public class TexturePallet
    {
        // Number of pallets is implicit in the format.
        // RGB565
        // Unused pallets are padded with 0xCDCD
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        ushort[,] pallets = new ushort[16, 256];

        public TexturePallet() { }

        internal TexturePallet(OptReader reader)
        {
            // For some reason pallets 0-7 seem to be padding, 8-15 appear to be increasing brightness
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    pallets[i, j] = reader.ReadUInt16();
                }
            }
        }

        // An indexer *is* a method. =P
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
        public ushort this[int palletNumber, int which]
        {
            get { return GetValue(palletNumber, which); }
            set { SetValue(palletNumber, which, value); }
        }

        public void SetValue(int palletNumber, int which, ushort color)
        {
            pallets[palletNumber, which] = color;
        }

        public ushort GetValue(int palletNumber, int which)
        {
            return pallets[palletNumber, which];
        }
    }
}
