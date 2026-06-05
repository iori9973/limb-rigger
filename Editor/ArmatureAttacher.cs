using UnityEditor;
using UnityEngine;

namespace LimbRigger
{
    public static class ArmatureAttacher
    {
        const string UndoLabel = "Limb Rigger: Attach Limb";

        public static void Attach(GameObject subLimb, GameObject avatarRoot, Transform attachmentPoint, Transform subtreeRoot)
        {
            if (attachmentPoint == null) return;
            var entry = subLimb != null ? subLimb : (subtreeRoot != null ? subtreeRoot.gameObject : null);
            if (entry == null) return;
            if (subtreeRoot == null) subtreeRoot = entry.transform;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(UndoLabel);
            int group = Undo.GetCurrentGroup();

            if (PrefabUtility.IsPartOfAnyPrefab(entry))
            {
                var outer = PrefabUtility.GetOutermostPrefabInstanceRoot(entry);
                if (outer != null)
                {
                    PrefabUtility.UnpackPrefabInstance(outer, PrefabUnpackMode.Completely, InteractionMode.UserAction);
                }
            }

            if (subtreeRoot == entry.transform)
            {
                Undo.SetTransformParent(entry.transform, attachmentPoint, UndoLabel);
            }
            else
            {
                if (avatarRoot != null)
                {
                    Undo.SetTransformParent(entry.transform, avatarRoot.transform, UndoLabel);
                }
                Undo.SetTransformParent(subtreeRoot, attachmentPoint, UndoLabel);
            }

            Undo.CollapseUndoOperations(group);
        }
    }
}
