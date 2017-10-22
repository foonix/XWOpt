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

    public class PartDescriptor<Vector3T> : BaseNode
    {
        internal PartDescriptor(OptReader opt) : base(opt)
        {

        }
    }
}
