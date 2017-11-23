using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    class TargetPointFactory
    {
        CraftFactory Craft { get; set; }
        DistinctTargetGroupTuple GroupTuple { get; set; }

        internal TargetPointFactory(CraftFactory craft, DistinctTargetGroupTuple groupTuple)
        {
            Craft = craft;
            GroupTuple = groupTuple;
        }

        internal GameObject CreateTargetPoint()
        {
            var targetPoint = Object.Instantiate(Craft.TargetPointBase) as GameObject;
            targetPoint.name = GroupTuple.type.ToString() + " Target Point";

            // TODO: ProcessTargetPointHandler invocation

            return targetPoint;
        }
    }
}
