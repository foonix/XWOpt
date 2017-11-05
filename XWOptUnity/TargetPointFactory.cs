using SchmooTech.XWOpt.OptNode;
using UnityEngine;

namespace SchmooTech.XWOptUnity
{
    class TargetPointFactory
    {
        PartFactory _part;

        internal TargetPointFactory(PartFactory part)
        {
            _part = part;
        }

        internal GameObject CreateTargetPoint(GameObject parent, Vector3 location, PartDescriptor<Vector3> partDescriptor)
        {
            var targetPoint = Object.Instantiate(_part._craft.TargetPointBase) as GameObject;
            targetPoint.name = partDescriptor.PartType.ToString();
            targetPoint.transform.localPosition = location;
            targetPoint.transform.localRotation = new Quaternion(0, 0, 0, 0);
            targetPoint.transform.parent = parent.transform;

            return targetPoint;
        }
    }
}
