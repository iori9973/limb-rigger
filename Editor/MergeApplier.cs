using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LimbRigger
{
    /// <summary>
    /// 各サブパーツボーンを、対応する本体ボーンの子へ再ペアレントする「マージ方式」。
    /// Rotation Constraint と違い、位置・回転ごと本体に追従し、適用時の配置をそのまま保持する。
    /// VRChat のコンストレイント内部計算に依存しないため、確実に配置が保たれる。
    /// </summary>
    public static class MergeApplier
    {
        const string ApplyLabel = "Limb Rigger: Merge Bones";

        public static int Apply(IEnumerable<BoneMapping> mappings)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(ApplyLabel);
            int group = Undo.GetCurrentGroup();

            // 有効かつ対応先のあるマッピングだけを対象にする。
            var targets = new List<BoneMapping>();
            foreach (var m in mappings)
            {
                if (!m.enabled) continue;
                if (m.subLimbBone == null || m.baseBone == null) continue;
                // 本体ボーンがサブボーン自身/その子孫だと循環参照になるため除外（通常は起きない）。
                if (IsSelfOrDescendant(m.baseBone, m.subLimbBone)) continue;
                targets.Add(m);
            }

            // 親を先に処理する（浅い階層→深い階層）。worldPositionStays=true なので
            // 結果は順序に依存しないが、挙動を分かりやすくするため浅い順にそろえる。
            targets.Sort((a, b) => Depth(a.subLimbBone).CompareTo(Depth(b.subLimbBone)));

            int moved = 0;
            foreach (var m in targets)
            {
                var sub = m.subLimbBone;
                if (sub.parent == m.baseBone) { moved++; continue; } // 既に正しい親
                // 第3引数 true = ワールド位置・回転・スケールを保持して再ペアレント。
                Undo.SetTransformParent(sub, m.baseBone, true, ApplyLabel);
                moved++;
            }

            Undo.CollapseUndoOperations(group);
            return moved;
        }

        static int Depth(Transform t)
        {
            int d = 0;
            while (t.parent != null) { d++; t = t.parent; }
            return d;
        }

        static bool IsSelfOrDescendant(Transform t, Transform ancestor)
        {
            var cur = t;
            while (cur != null)
            {
                if (cur == ancestor) return true;
                cur = cur.parent;
            }
            return false;
        }
    }
}
