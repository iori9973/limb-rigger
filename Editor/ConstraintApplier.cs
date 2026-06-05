using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace LimbRigger
{
    public static class ConstraintApplier
    {
        const string ApplyLabel = "Limb Rigger: Apply Constraints";
        const string RemoveLabel = "Limb Rigger: Remove Constraints";

        public static int Apply(IEnumerable<BoneMapping> mappings)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(ApplyLabel);
            int group = Undo.GetCurrentGroup();
            int applied = 0;

            foreach (var m in mappings)
            {
                if (!m.enabled) continue;
                if (m.subLimbBone == null || m.baseBone == null) continue;

                var go = m.subLimbBone.gameObject;
                var existing = go.GetComponent<VRCRotationConstraint>();
                VRCRotationConstraint rc = existing != null
                    ? existing
                    : Undo.AddComponent<VRCRotationConstraint>(go);

                Undo.RecordObject(rc, ApplyLabel);

                rc.Sources.Clear();
                rc.Sources.Add(new VRCConstraintSource
                {
                    SourceTransform = m.baseBone,
                    Weight = 1f,
                    ParentRotationOffset = Vector3.zero,
                    ParentPositionOffset = Vector3.zero,
                });
                rc.AffectsRotationX = true;
                rc.AffectsRotationY = true;
                rc.AffectsRotationZ = true;
                rc.RotationAtRest = m.subLimbBone.localEulerAngles;
                rc.RotationOffset = Vector3.zero;
                rc.GlobalWeight = 1f;
                rc.SolveInLocalSpace = false;
                rc.IsActive = true;
                rc.Locked = true;

                applied++;
            }

            Undo.CollapseUndoOperations(group);
            return applied;
        }

        public static int Remove(Transform subLimbRoot)
        {
            if (subLimbRoot == null) return 0;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(RemoveLabel);
            int group = Undo.GetCurrentGroup();
            int removed = 0;

            var constraints = subLimbRoot.GetComponentsInChildren<VRCRotationConstraint>(true);
            foreach (var rc in constraints)
            {
                Undo.DestroyObjectImmediate(rc);
                removed++;
            }

            Undo.CollapseUndoOperations(group);
            return removed;
        }
    }
}
