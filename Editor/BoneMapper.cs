using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        static readonly Regex BlenderDupSuffix = new Regex(@"\.\d{3,}$", RegexOptions.Compiled);

        static readonly Regex RigPrefix = new Regex(
            @"^(mixamorig[:_]?|j_?bip_?|j_?sec_?|bip0?0?1?_?|bone_|armature\|)",
            RegexOptions.Compiled);

        static readonly Regex SingleLetterLeftPrefix = new Regex(
            @"^l(?!eft)[_.\s-]",
            RegexOptions.Compiled);

        static readonly Regex SingleLetterRightPrefix = new Regex(
            @"^r(?!ight)[_.\s-]",
            RegexOptions.Compiled);

        static readonly Regex SingleLetterCenterPrefix = new Regex(
            @"^c[_.\s-]",
            RegexOptions.Compiled);

        static readonly Regex MixamoFingerNumber = new Regex(
            @"(?:hand)?(thumb|index|middle|ring|little|pinky)(\d)",
            RegexOptions.Compiled);

        static readonly (string from, string to)[] SemanticAliases = new[]
        {
            ("upperarm", "arm"),
            ("forearm", "lowerarm"),
            ("upperleg", "leg"),
            ("upleg", "leg"),
            ("thigh", "leg"),
            ("lowerleg", "shin"),
            ("calf", "shin"),
            ("clavicle", "shoulder"),
            ("wrist", "hand"),
            ("ankle", "foot"),
            ("proximal", "prox"),
            ("intermediate", "int"),
            ("distal", "dist"),
            ("metacarpal", "meta"),
        };

        public static string Normalize(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            string s = name.ToLowerInvariant();

            s = RigPrefix.Replace(s, string.Empty);
            s = BlenderDupSuffix.Replace(s, string.Empty);

            s = SingleLetterLeftPrefix.Replace(s, "left_");
            s = SingleLetterRightPrefix.Replace(s, "right_");
            s = SingleLetterCenterPrefix.Replace(s, string.Empty);

            s = s.Replace("_", "").Replace(".", "").Replace(" ", "").Replace("-", "");

            s = NormalizeLR(s);

            s = MixamoFingerNumber.Replace(s, match =>
            {
                var finger = match.Groups[1].Value;
                if (finger == "pinky") finger = "little";
                var num = match.Groups[2].Value;
                string pos;
                switch (num)
                {
                    case "1": pos = "prox"; break;
                    case "2": pos = "int"; break;
                    case "3": pos = "dist"; break;
                    default: pos = num; break;
                }
                return finger + pos;
            });

            foreach (var (from, to) in SemanticAliases)
            {
                s = s.Replace(from, to);
            }

            return s;
        }

        static string NormalizeLR(string s)
        {
            if (s.StartsWith("left"))
            {
                s = s.Substring(4) + "l";
            }
            else if (s.StartsWith("right"))
            {
                s = s.Substring(5) + "r";
            }

            if (s.EndsWith("left"))
            {
                s = s.Substring(0, s.Length - 4) + "l";
            }
            else if (s.EndsWith("right"))
            {
                s = s.Substring(0, s.Length - 5) + "r";
            }

            return s;
        }
    }
}
