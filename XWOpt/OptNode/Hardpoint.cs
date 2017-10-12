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
        public Hardpoint(OptFile opt) : base(opt)
        {

        }
    }
}
