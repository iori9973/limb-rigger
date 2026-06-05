using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LimbRigger
{
    public class LimbRiggerWindow : EditorWindow
    {
        GameObject avatarRoot;
        Transform attachmentPoint;
        Transform subLimbSubtreeRoot;
        GameObject subLimbWrapper;

        List<BoneMapping> mappings = new List<BoneMapping>();
        Vector2 scroll;
        bool showWrapperField;

        [MenuItem("Tools/Limb Rigger")]
        public static void Open()
        {
            GetWindow<LimbRiggerWindow>("Limb Rigger");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Avatar / Sub Limb", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "操作順: ①位置・スケールを目視で合わせる ②アーマチュア接続 ③マッピング解析 ④プレビュー確認 ⑤適用",
                MessageType.Info);

            avatarRoot = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("Avatar Root", "本体アバターのルート GameObject。Humanoid Animator がある側。"),
                avatarRoot, typeof(GameObject), true);

            attachmentPoint = (Transform)EditorGUILayout.ObjectField(
                new GUIContent("Attachment Point", "サブパーツを取り付ける本体側の親ボーン (例: Chest / Hips)"),
                attachmentPoint, typeof(Transform), true);

            subLimbSubtreeRoot = (Transform)EditorGUILayout.ObjectField(
                new GUIContent("Sub Limb Root", "取り付ける義手・サブアーム等のルート Transform。単体パーツならその prefab のルート、フルアバター流用なら取り出したい部位のルート (例: Shoulder.L)"),
                subLimbSubtreeRoot, typeof(Transform), true);

            showWrapperField = EditorGUILayout.Foldout(showWrapperField, "Advanced (フルアバター流用)");
            if (showWrapperField)
            {
                EditorGUI.indentLevel++;
                subLimbWrapper = (GameObject)EditorGUILayout.ObjectField(
                    new GUIContent("Wrapper Root", "サブパーツが大きな prefab (例: Anubis 全体) の一部の場合、その prefab のルートを指定。空欄なら Sub Limb Root を直接 Attachment Point 配下に置く"),
                    subLimbWrapper, typeof(GameObject), true);
                EditorGUI.indentLevel--;
            }

            ShowAvatarRootDiagnostics();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Step 1: Attach", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(attachmentPoint == null || subLimbSubtreeRoot == null))
            {
                if (GUILayout.Button("アーマチュア接続"))
                {
                    var entry = subLimbWrapper != null ? subLimbWrapper : subLimbSubtreeRoot.gameObject;
                    ArmatureAttacher.Attach(entry, avatarRoot, attachmentPoint, subLimbSubtreeRoot);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Step 2: Bone Mapping", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(avatarRoot == null || subLimbSubtreeRoot == null))
            {
                if (GUILayout.Button("マッピング解析"))
                {
                    var anim = avatarRoot.GetComponent<Animator>();
                    if (anim == null)
                    {
                        Debug.LogWarning("[Limb Rigger] Avatar Root に Animator が見つかりません。Humanoid Avatar の Animator を含む GameObject を指定してください。");
                        mappings.Clear();
                    }
                    else
                    {
                        mappings = BoneMapper.Map(anim, subLimbSubtreeRoot);
                        if (mappings.Count == 0)
                        {
                            Debug.LogWarning("[Limb Rigger] マッピング 0 件。Sub Limb Root が空、または対応ボーンが見つかりません。");
                        }
                    }
                }
            }

            if (mappings.Count > 0)
            {
                int h = 0, n = 0, m = 0, u = 0;
                foreach (var mp in mappings)
                {
                    switch (mp.tier)
                    {
                        case MappingTier.Humanoid: h++; break;
                        case MappingTier.NameMatch: n++; break;
                        case MappingTier.ManualOverride: m++; break;
                        default: u++; break;
                    }
                }
                EditorGUILayout.LabelField($"Mappings ({mappings.Count}) — H:{h} N:{n} M:{m} Unmapped:{u}", EditorStyles.boldLabel);
                using (var s = new EditorGUILayout.ScrollViewScope(scroll, GUILayout.MinHeight(200)))
                {
                    scroll = s.scrollPosition;
                    foreach (var mp in mappings)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            mp.enabled = EditorGUILayout.Toggle(mp.enabled, GUILayout.Width(16));
                            var prev = GUI.color;
                            GUI.color = TierColor(mp.tier);
                            EditorGUILayout.LabelField(TierBadge(mp.tier), GUILayout.Width(28));
                            GUI.color = prev;
                            EditorGUILayout.LabelField(mp.subLimbBone != null ? mp.subLimbBone.name : "<null>", GUILayout.MinWidth(120));
                            EditorGUILayout.LabelField("→", GUILayout.Width(20));
                            var newBase = (Transform)EditorGUILayout.ObjectField(mp.baseBone, typeof(Transform), true);
                            if (newBase != mp.baseBone)
                            {
                                mp.baseBone = newBase;
                                mp.tier = MappingTier.ManualOverride;
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Step 3: Apply", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(mappings.Count == 0))
            {
                if (GUILayout.Button("適用 (Constraint + Source + Activate)"))
                {
                    int applied = ConstraintApplier.Apply(mappings);
                    Debug.Log($"[Limb Rigger] Applied constraints to {applied} bones.");
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rollback", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(subLimbSubtreeRoot == null))
            {
                if (GUILayout.Button("生成物を削除"))
                {
                    int removed = ConstraintApplier.Remove(subLimbSubtreeRoot);
                    Debug.Log($"[Limb Rigger] Removed {removed} constraints.");
                }
            }
        }

        void ShowAvatarRootDiagnostics()
        {
            if (avatarRoot == null) return;
            var anim = avatarRoot.GetComponent<Animator>();
            if (anim == null)
            {
                EditorGUILayout.HelpBox(
                    "Avatar Root に Animator がありません。Humanoid Animator を持つ GameObject (通常はアバター prefab のルート) を指定してください。",
                    MessageType.Error);
                return;
            }
            if (!anim.isHuman)
            {
                EditorGUILayout.HelpBox(
                    "Avatar Root の Animator が Humanoid ではありません。Rig タブを Humanoid に設定したアバターを使用してください。Tier 1 (Humanoid) と Tier 1.5 (Base Humanoid + Sub 名前マッチ) の自動マッピングが機能しません。",
                    MessageType.Warning);
            }
        }

        static Color TierColor(MappingTier t)
        {
            switch (t)
            {
                case MappingTier.Humanoid: return new Color(0.4f, 1f, 0.4f);
                case MappingTier.NameMatch: return new Color(1f, 0.85f, 0.3f);
                case MappingTier.ManualOverride: return new Color(0.7f, 0.7f, 1f);
                default: return new Color(1f, 0.5f, 0.5f);
            }
        }

        static string TierBadge(MappingTier t)
        {
            switch (t)
            {
                case MappingTier.Humanoid: return "H";
                case MappingTier.NameMatch: return "N";
                case MappingTier.ManualOverride: return "M";
                default: return "—";
            }
        }
    }
}
