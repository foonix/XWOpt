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
    public enum PartType
    {
        DefaultType,
        MainHull,
        Wing,
        Fuselage,
        GunTurret,
        SmallGun,
        Engine,
        Bridge,
        ShieldGen,
        EnergyGen,
        Launcher,
        CommSys,
        BeamSys,
        CommandBeam,
        DockingPlat,
        LandingPlat,
        Hangar,
        CargoPod,
        MiscHull,
        Antenna,
        RotWing,
        RotGunTurret,
        RotLauncher,
        RotCommSys,
        RotBeamSys,
        RotCommandBeam,
        Custom1,
        Custom2,
        Custom3,
        Custom4,
        Custom5,
        Custom6,
    }

    public class PartDescriptor<TVector3> : BaseNode
    {
        public PartType partType;

        // 0,1,4,5,8,9 = Mesh continues in straight line when destroyed 2,3,6,10 = Mesh breaks off and explodes 7 = destructible parts
        // Looks like bitmask but not sure
        public int ExplosionType;

        public TVector3 span; // width of the hit box
        public TVector3 centerPoint; // Center point of hit box. For targeting the specific part of the craft with "," key.
        public TVector3 minXYZ; // Lower X,Y,Z bound of the hit box
        public TVector3 maxXYZ; // Upper X,Y,Z bound of the hit box

        public long targetingGroupID;

        static Vector3Adapter<TVector3> v3Adapter = new Vector3Adapter<TVector3>();

        internal PartDescriptor(OptReader reader) : base(reader)
        {
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(1, this);

            reader.FollowPointerToNextByte(this);

            partType = (PartType)reader.ReadUInt32();
            ExplosionType = reader.ReadInt32();

            v3Adapter.Read(reader, ref span);
            v3Adapter.Read(reader, ref centerPoint);
            v3Adapter.Read(reader, ref minXYZ);
            v3Adapter.Read(reader, ref maxXYZ);

            targetingGroupID = reader.ReadUInt32();
        }
    }
}
