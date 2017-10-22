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

    public class Hardpoint<Vector3T> : BaseNode
    {
        public HardpointType type;
        public Vector3T coords;

        static Vector3Adapter<Vector3T> v3Adapter = new Vector3Adapter<Vector3T>();

        internal Hardpoint(OptReader reader) : base(reader)
        {
            reader.ReadUnknownUseValue(0);
            reader.ReadUnknownUseValue(0);
            reader.ReadUnknownUseValue(1);

            reader.FollowPointerToNextByte();

            type = (HardpointType)reader.ReadUInt32();

            v3Adapter.Read(reader, ref coords);
        }
    }
}
