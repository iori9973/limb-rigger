using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
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

                var legacy = go.GetComponent<RotationConstraint>();
                if (legacy != null)
                {
                    Undo.DestroyObjectImmediate(legacy);
                }

                var existing = go.GetComponent<VRCRotationConstraint>();
                VRCRotationConstraint rc = existing != null
                    ? existing
                    : Undo.AddComponent<VRCRotationConstraint>(go);

                Undo.RecordObject(rc, ApplyLabel);

                // 適用時点のサブボーンの向きを維持するためのオフセットを焼き込む。
                // VRCRotationConstraint はソースごとの ParentRotationOffset を使わず、
                // コンストレイント単体の RotationOffset を使う（Unity の RotationConstraint と同様）。
                // ワールド空間で result = source.rotation * Euler(RotationOffset) == subBone.rotation
                // となる offset = Inverse(source.rotation) * subBone.rotation を RotationOffset に入れる。
                // ※v0.2.5 では同じ値を ParentRotationOffset に入れていたが効かずスナップした。
                Quaternion restOffset = Quaternion.Inverse(m.baseBone.rotation) * go.transform.rotation;

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
                rc.RotationAtRest = go.transform.localRotation.eulerAngles;
                rc.RotationOffset = restOffset.eulerAngles;
                rc.GlobalWeight = 1f;
                rc.SolveInLocalSpace = false;
                rc.IsActive = true;
                rc.Locked = false;

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

            var vrcConstraints = subLimbRoot.GetComponentsInChildren<VRCRotationConstraint>(true);
            foreach (var rc in vrcConstraints)
            {
                Undo.DestroyObjectImmediate(rc);
                removed++;
            }

            var unityConstraints = subLimbRoot.GetComponentsInChildren<RotationConstraint>(true);
            foreach (var rc in unityConstraints)
            {
                Undo.DestroyObjectImmediate(rc);
                removed++;
            }

            Undo.CollapseUndoOperations(group);
            return removed;
        }
    }
}
