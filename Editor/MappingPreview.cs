using UnityEngine;

namespace LimbRigger
{
    public enum MappingTier
    {
        Unmapped = 0,
        NameMatch = 1,
        Humanoid = 2,
        ManualOverride = 3,
    }

    [System.Serializable]
    public class BoneMapping
    {
        public Transform subLimbBone;
        public Transform baseBone;
        public MappingTier tier;
        public bool enabled = true;
    }
}
