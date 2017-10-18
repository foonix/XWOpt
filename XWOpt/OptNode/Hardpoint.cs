using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SchmooTech.XWOpt.OptNode
{
    public enum HardpointType
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

    public class Hardpoint : BaseNode
    {
        public HardpointType type;
        public Object coords;

        internal Hardpoint(OptReader reader) : base(reader)
        {
            reader.ReadUnknownUseValue(0);
            reader.ReadUnknownUseValue(0);
            reader.ReadUnknownUseValue(1);

            reader.FollowPointerToNextByte();

            type = (HardpointType)reader.ReadUInt32();

            coords = reader.opt.vector3Cotr.Invoke(new object[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() });
        }
    }
}
