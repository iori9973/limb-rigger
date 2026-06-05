using NUnit.Framework;
using UnityEngine;

namespace LimbRigger.Tests
{
    public class BoneMapperNormalizeTests
    {
        static string Norm(string s) => BoneMapper.Normalize(s);

        [Test]
        public void EmptyOrNull_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, Norm(null));
            Assert.AreEqual(string.Empty, Norm(string.Empty));
        }

        [Test]
        public void HumanBodyBoneEnumStrings_MatchExpectedCanonicalKeys()
        {
            Assert.AreEqual("arml", Norm("LeftUpperArm"));
            Assert.AreEqual("armr", Norm("RightUpperArm"));
            Assert.AreEqual("lowerarml", Norm("LeftLowerArm"));
            Assert.AreEqual("handl", Norm("LeftHand"));
            Assert.AreEqual("shoulderl", Norm("LeftShoulder"));
            Assert.AreEqual("legl", Norm("LeftUpperLeg"));
            Assert.AreEqual("shinl", Norm("LeftLowerLeg"));
            Assert.AreEqual("footl", Norm("LeftFoot"));
            Assert.AreEqual("indexproxl", Norm("LeftIndexProximal"));
            Assert.AreEqual("indexintl", Norm("LeftIndexIntermediate"));
            Assert.AreEqual("indexdistl", Norm("LeftIndexDistal"));
            Assert.AreEqual("thumbproxl", Norm("LeftThumbProximal"));
        }

        [Test]
        public void Marycia_UnderscoreDotNaming_MatchesHumanoidCanonical()
        {
            Assert.AreEqual(Norm("LeftUpperArm"), Norm("Upper_arm.L"));
            Assert.AreEqual(Norm("LeftLowerArm"), Norm("Lower_arm.L"));
            Assert.AreEqual(Norm("LeftHand"), Norm("Hand.L"));
            Assert.AreEqual(Norm("LeftIndexProximal"), Norm("Index Proximal.L"));
            Assert.AreEqual(Norm("LeftThumbProximal"), Norm("Thumb Proximal.L"));
            Assert.AreEqual(Norm("LeftShoulder"), Norm("Shoulder.L"));
        }

        [Test]
        public void Titan_DotNamingAndBlenderSuffix_MatchesHumanoidCanonical()
        {
            Assert.AreEqual(Norm("LeftUpperArm"), Norm("Arm.L.001"));
            Assert.AreEqual(Norm("RightUpperArm"), Norm("Arm.R.001"));
            Assert.AreEqual(Norm("LeftLowerArm"), Norm("Lower.Arm.L"));
            Assert.AreEqual(Norm("LeftHand"), Norm("Hand.L"));
            Assert.AreEqual(Norm("LeftIndexProximal"), Norm("Index Proximal.L"));
            Assert.AreEqual(Norm("LeftThumbProximal"), Norm("Thumb proximal.L"));
            Assert.AreEqual(Norm("LeftMiddleProximal"), Norm("Middle proximal.L"));
        }

        [Test]
        public void Anubis_SpaceNaming_MatchesHumanoidCanonical()
        {
            Assert.AreEqual(Norm("LeftUpperArm"), Norm("Upper Arm.L"));
            Assert.AreEqual(Norm("LeftLowerArm"), Norm("Lower Arm.L"));
            Assert.AreEqual(Norm("LeftHand"), Norm("Hand.L"));
            Assert.AreEqual(Norm("LeftUpperLeg"), Norm("Upper Leg.L"));
            Assert.AreEqual(Norm("LeftLowerLeg"), Norm("Lower Leg.L"));
        }

        [Test]
        public void Mixamo_NamespacePrefix_IsStripped()
        {
            Assert.AreEqual(Norm("Hips"), Norm("mixamorig:Hips"));
            Assert.AreEqual(Norm("LeftUpperArm"), Norm("mixamorig:LeftArm"));
            Assert.AreEqual(Norm("LeftLowerArm"), Norm("mixamorig:LeftForeArm"));
            Assert.AreEqual(Norm("LeftHand"), Norm("mixamorig:LeftHand"));
            Assert.AreEqual(Norm("LeftUpperLeg"), Norm("mixamorig:LeftUpLeg"));
            Assert.AreEqual(Norm("LeftShoulder"), Norm("mixamorig:LeftShoulder"));
        }

        [Test]
        public void Mixamo_FingerNumbering_IsConvertedToProxIntDist()
        {
            Assert.AreEqual(Norm("LeftIndexProximal"), Norm("mixamorig:LeftHandIndex1"));
            Assert.AreEqual(Norm("LeftIndexIntermediate"), Norm("mixamorig:LeftHandIndex2"));
            Assert.AreEqual(Norm("LeftIndexDistal"), Norm("mixamorig:LeftHandIndex3"));
            Assert.AreEqual(Norm("LeftThumbProximal"), Norm("mixamorig:LeftHandThumb1"));
            Assert.AreEqual(Norm("LeftMiddleIntermediate"), Norm("mixamorig:LeftHandMiddle2"));
            Assert.AreEqual(Norm("RightLittleDistal"), Norm("mixamorig:RightHandPinky3"));
            Assert.AreEqual(Norm("LeftLittleProximal"), Norm("mixamorig:LeftHandPinky1"));
        }

        [Test]
        public void VRoid_JBipPrefixAndSingleLetterSide_IsConverted()
        {
            Assert.AreEqual(Norm("LeftUpperArm"), Norm("J_Bip_L_UpperArm"));
            Assert.AreEqual(Norm("RightUpperArm"), Norm("J_Bip_R_UpperArm"));
            Assert.AreEqual(Norm("LeftLowerArm"), Norm("J_Bip_L_LowerArm"));
            Assert.AreEqual(Norm("LeftHand"), Norm("J_Bip_L_Hand"));
            Assert.AreEqual(Norm("LeftIndexProximal"), Norm("J_Bip_L_Index1"));
            Assert.AreEqual(Norm("LeftIndexIntermediate"), Norm("J_Bip_L_Index2"));
            Assert.AreEqual(Norm("LeftThumbProximal"), Norm("J_Bip_L_Thumb1"));
        }

        [Test]
        public void VRoid_CenterPrefix_IsStripped()
        {
            Assert.AreEqual(Norm("Hips"), Norm("J_Bip_C_Hips"));
            Assert.AreEqual(Norm("Spine"), Norm("J_Bip_C_Spine"));
            Assert.AreEqual(Norm("Chest"), Norm("J_Bip_C_Chest"));
            Assert.AreEqual(Norm("Neck"), Norm("J_Bip_C_Neck"));
            Assert.AreEqual(Norm("Head"), Norm("J_Bip_C_Head"));
        }

        [Test]
        public void Biped_Bip01Prefix_IsConverted()
        {
            Assert.AreEqual(Norm("LeftUpperArm"), Norm("Bip01_L_UpperArm"));
            Assert.AreEqual(Norm("LeftLowerArm"), Norm("Bip01_L_Forearm"));
            Assert.AreEqual(Norm("LeftHand"), Norm("Bip01_L_Hand"));
            Assert.AreEqual(Norm("LeftUpperArm"), Norm("Bip001_L_UpperArm"));
        }

        [Test]
        public void SingleLetterPrefix_DoesNotMatch_WhenPrecedingLeftRightAlready()
        {
            // "Left_" should NOT be rewritten as "left_eft_" by the L_ regex
            Assert.AreEqual(Norm("LeftUpperArm"), Norm("Left_UpperArm"));
            Assert.AreEqual(Norm("RightHand"), Norm("Right_Hand"));
        }

        [Test]
        public void LRSuffix_DotNotation_NormalizesToTrailingLR()
        {
            Assert.AreEqual(Norm("LeftHand"), Norm("Hand.L"));
            Assert.AreEqual(Norm("RightHand"), Norm("Hand.R"));
            Assert.AreEqual(Norm("LeftHand"), Norm("Hand_L"));
            Assert.AreEqual(Norm("RightHand"), Norm("Hand-R"));
        }

        [Test]
        public void BlenderDupSuffix_OnlyAtEnd_IsStripped()
        {
            Assert.AreEqual(Norm("Hand.L"), Norm("Hand.L.001"));
            Assert.AreEqual(Norm("Hand.L"), Norm("Hand.L.002"));
            // digits in the middle of the name should NOT be stripped
            Assert.AreEqual("bone001lhand", Norm("Bone001_L_Hand"));
        }

        [Test]
        public void CaseInsensitive_PrefixStripping()
        {
            // Mixed case variants of the prefix should all work
            Assert.AreEqual(Norm("LeftHand"), Norm("MixamoRig:LeftHand"));
            Assert.AreEqual(Norm("LeftHand"), Norm("MIXAMORIG:LeftHand"));
            Assert.AreEqual(Norm("LeftUpperArm"), Norm("J_BIP_L_UpperArm"));
        }

        [Test]
        public void NoMatch_ReturnsConsistentForUnknownBones()
        {
            // Unknown bones should normalize consistently (for Tier 2 fallback)
            Assert.AreEqual("breast", Norm("Breast"));
            Assert.AreEqual("breastl", Norm("Breast.L"));
            Assert.AreEqual("hair", Norm("Hair"));
        }
    }
}
