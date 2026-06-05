using System.Text.RegularExpressions;

namespace LimbRigger
{
    public static class BoneNameNormalizer
    {
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
