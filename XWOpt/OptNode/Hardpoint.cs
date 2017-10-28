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

namespace SchmooTech.XWOpt.OptNode
{
    public enum WeaponType
    {
        None,
        RebelLaser,
        TurboRebelLaser,
        EmpireLaser,
        TurboEmpireLaser,
        IonCannon,
        TurboIonCannon,
        Torpedo,
        Missile,
        SuperRebelLaser,
        SuperEmpireLaser,
        SuperIonCannon,
        SuperTorpedo,
        SuperMissile,
        DumbBomb,
        FiredBomb,
        Magpulse,
        TurboMagpulse,
        SuperMagpulse,
        NewWeapon1,
        NewWeapon2,
        NewWeapon3,
        NewWeapon4,
        NewWeapon5,
        NewWeapon6,
        InsideHangar,
        OutsideHangar,
        DockFromBig,
        DockFromSmall,
        DockToBig,
        DockToSmall,
        Cockpit,
    }

    public class Hardpoint<TVector3> : BaseNode
    {
        private WeaponType weaponType;
        private TVector3 location;

        static Vector3Adapter<TVector3> v3Adapter = new Vector3Adapter<TVector3>();

        public WeaponType WeaponType { get => weaponType; set => weaponType = value; }
        public TVector3 Location { get => location; set => location = value; }

        internal Hardpoint(OptReader reader) : base(reader)
        {
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(1, this);

            reader.FollowPointerToNextByte(this);

            weaponType = (WeaponType)reader.ReadUInt32();

            v3Adapter.Read(reader, ref location);
        }
    }
}
