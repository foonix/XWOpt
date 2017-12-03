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
        /// <summary>
        /// An enum identifying the type of part.
        /// Used for targeting, and seems to be used for removing unneded parts on some models.
        /// </summary>
        public PartType PartType { get; set; }

        /// <summary>
        /// 0,1,4,5,8,9 = Mesh continues in straight line when destroyed 2,3,6,10 = Mesh breaks off and explodes 7 = destructible parts
        /// Looks like bitmask but not sure
        /// </summary>
        public int ExplosionType { get; set; }

        /// <summary>
        /// Width of the hit box
        /// </summary>
        public TVector3 HitboxSpan { get; set; }

        /// <summary>
        /// Center point of hit box. For targeting the specific part of the craft with "," key.
        /// </summary>
        public TVector3 HitboxCenterPoint { get; set; }

        /// <summary>
        /// Lower X,Y,Z bound of the hit box
        /// </summary>
        public TVector3 HitboxLowerCorner { get; set; }

        /// <summary>
        /// Upper X,Y,Z bound of the hit box
        /// </summary>
        public TVector3 HitboxUpperCorner { get; set; }

        /// <summary>
        /// Parts with the same TargetingGroupId, TargetPoint, and PartType
        /// are considered the same part for targeting purposes.
        /// </summary>
        public int TargetGroupId { get; set; }

        /// <summary>
        /// Where the target point should be displayed to the user.
        /// </summary>
        public TVector3 TargetPoint { get; set; }

        internal PartDescriptor(OptReader reader) : base(reader)
        {
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(0, this);
            reader.ReadUnknownUseValue(1, this);

            reader.FollowPointerToNextByte(this);

            PartType = (PartType)reader.ReadUInt32();
            ExplosionType = reader.ReadInt32();

            HitboxSpan = reader.ReadVector<TVector3>();
            HitboxCenterPoint = reader.ReadVector<TVector3>();
            HitboxLowerCorner = reader.ReadVector<TVector3>();
            HitboxUpperCorner = reader.ReadVector<TVector3>();

            TargetGroupId = reader.ReadInt32();

            TargetPoint = reader.ReadVector<TVector3>();
        }
    }
}
