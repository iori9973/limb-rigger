using System.Collections.Generic;
using UnityEngine;

namespace LimbRigger
{
    public static class BoneMapper
    {
        public static List<BoneMapping> Map(Animator baseAnimator, Transform subLimbRoot)
        {
            var mappings = new List<BoneMapping>();
            if (baseAnimator == null || subLimbRoot == null) return mappings;

            var subBones = new List<Transform>();
            CollectChildren(subLimbRoot, subBones);
            var subBoneSet = new HashSet<Transform>(subBones);
            var matched = new HashSet<Transform>();

            if (baseAnimator.isHuman)
            {
                var subAnimator = FindParentAnimator(subLimbRoot);
                bool subIsHuman = subAnimator != null && subAnimator != baseAnimator && subAnimator.isHuman;

                var subByNorm = new Dictionary<string, Transform>();
                if (!subIsHuman)
                {
                    foreach (var sb in subBones)
                    {
                        var key = Normalize(sb.name);
                        if (string.IsNullOrEmpty(key)) continue;
                        if (!subByNorm.ContainsKey(key)) subByNorm[key] = sb;
                    }
                }

                foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (bone == HumanBodyBones.LastBone) continue;
                    var baseBone = baseAnimator.GetBoneTransform(bone);
                    if (baseBone == null) continue;

                    Transform subBone = null;
                    if (subIsHuman)
                    {
                        subBone = subAnimator.GetBoneTransform(bone);
                        if (subBone == null || !subBoneSet.Contains(subBone)) continue;
                    }
                    else
                    {
                        var humKey = Normalize(bone.ToString());
                        if (string.IsNullOrEmpty(humKey)) continue;
                        if (!subByNorm.TryGetValue(humKey, out subBone)) continue;
                        if (matched.Contains(subBone)) continue;
                    }

                    mappings.Add(new BoneMapping
                    {
                        subLimbBone = subBone,
                        baseBone = baseBone,
                        tier = MappingTier.Humanoid,
                    });
                    matched.Add(subBone);
                }
            }

            var baseAllBones = new List<Transform>();
            CollectChildren(baseAnimator.transform, baseAllBones);
            var baseByNorm = new Dictionary<string, Transform>();
            foreach (var b in baseAllBones)
            {
                if (IsDescendantOf(b, subLimbRoot)) continue;
                var key = Normalize(b.name);
                if (string.IsNullOrEmpty(key)) continue;
                if (!baseByNorm.ContainsKey(key)) baseByNorm[key] = b;
            }

            foreach (var sb in subBones)
            {
                if (matched.Contains(sb)) continue;
                var key = Normalize(sb.name);
                Transform bb = null;
                MappingTier tier = MappingTier.Unmapped;
                if (!string.IsNullOrEmpty(key) && baseByNorm.TryGetValue(key, out bb))
                {
                    tier = MappingTier.NameMatch;
                }
                mappings.Add(new BoneMapping
                {
                    subLimbBone = sb,
                    baseBone = bb,
                    tier = tier,
                });
            }

            return mappings;
        }

        static void CollectChildren(Transform root, List<Transform> result)
        {
            result.Add(root);
            for (int i = 0; i < root.childCount; i++)
                CollectChildren(root.GetChild(i), result);
        }

        static Animator FindParentAnimator(Transform t)
        {
            while (t != null)
            {
                var a = t.GetComponent<Animator>();
                if (a != null) return a;
                t = t.parent;
            }
            return null;
        }

        static bool IsDescendantOf(Transform t, Transform ancestor)
        {
            var cur = t;
            while (cur != null)
            {
                if (cur == ancestor) return true;
                cur = cur.parent;
            }
            return false;
        }

        static string Normalize(string name) => BoneNameNormalizer.Normalize(name);
    }
}
