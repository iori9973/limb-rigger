using LimbRigger;
using Xunit;

namespace LimbRigger.Tests;

public class BoneNameNormalizerTests
{
    static string Norm(string? s) => BoneNameNormalizer.Normalize(s!);

    [Fact]
    public void EmptyOrNull_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, Norm(null));
        Assert.Equal(string.Empty, Norm(string.Empty));
    }

    [Fact]
    public void HumanBodyBoneEnumStrings_MatchExpectedCanonicalKeys()
    {
        Assert.Equal("arml", Norm("LeftUpperArm"));
        Assert.Equal("armr", Norm("RightUpperArm"));
        Assert.Equal("lowerarml", Norm("LeftLowerArm"));
        Assert.Equal("handl", Norm("LeftHand"));
        Assert.Equal("shoulderl", Norm("LeftShoulder"));
        Assert.Equal("legl", Norm("LeftUpperLeg"));
        Assert.Equal("shinl", Norm("LeftLowerLeg"));
        Assert.Equal("footl", Norm("LeftFoot"));
        Assert.Equal("indexproxl", Norm("LeftIndexProximal"));
        Assert.Equal("indexintl", Norm("LeftIndexIntermediate"));
        Assert.Equal("indexdistl", Norm("LeftIndexDistal"));
        Assert.Equal("thumbproxl", Norm("LeftThumbProximal"));
    }

    [Fact]
    public void Marycia_UnderscoreDotNaming_MatchesHumanoidCanonical()
    {
        Assert.Equal(Norm("LeftUpperArm"), Norm("Upper_arm.L"));
        Assert.Equal(Norm("LeftLowerArm"), Norm("Lower_arm.L"));
        Assert.Equal(Norm("LeftHand"), Norm("Hand.L"));
        Assert.Equal(Norm("LeftIndexProximal"), Norm("Index Proximal.L"));
        Assert.Equal(Norm("LeftThumbProximal"), Norm("Thumb Proximal.L"));
        Assert.Equal(Norm("LeftShoulder"), Norm("Shoulder.L"));
    }

    [Fact]
    public void Titan_DotNamingAndBlenderSuffix_MatchesHumanoidCanonical()
    {
        Assert.Equal(Norm("LeftUpperArm"), Norm("Arm.L.001"));
        Assert.Equal(Norm("RightUpperArm"), Norm("Arm.R.001"));
        Assert.Equal(Norm("LeftLowerArm"), Norm("Lower.Arm.L"));
        Assert.Equal(Norm("LeftHand"), Norm("Hand.L"));
        Assert.Equal(Norm("LeftIndexProximal"), Norm("Index Proximal.L"));
        Assert.Equal(Norm("LeftThumbProximal"), Norm("Thumb proximal.L"));
        Assert.Equal(Norm("LeftMiddleProximal"), Norm("Middle proximal.L"));
    }

    [Fact]
    public void Anubis_SpaceNaming_MatchesHumanoidCanonical()
    {
        Assert.Equal(Norm("LeftUpperArm"), Norm("Upper Arm.L"));
        Assert.Equal(Norm("LeftLowerArm"), Norm("Lower Arm.L"));
        Assert.Equal(Norm("LeftHand"), Norm("Hand.L"));
        Assert.Equal(Norm("LeftUpperLeg"), Norm("Upper Leg.L"));
        Assert.Equal(Norm("LeftLowerLeg"), Norm("Lower Leg.L"));
    }

    [Fact]
    public void Mixamo_NamespacePrefix_IsStripped()
    {
        Assert.Equal(Norm("Hips"), Norm("mixamorig:Hips"));
        Assert.Equal(Norm("LeftUpperArm"), Norm("mixamorig:LeftArm"));
        Assert.Equal(Norm("LeftLowerArm"), Norm("mixamorig:LeftForeArm"));
        Assert.Equal(Norm("LeftHand"), Norm("mixamorig:LeftHand"));
        Assert.Equal(Norm("LeftUpperLeg"), Norm("mixamorig:LeftUpLeg"));
        Assert.Equal(Norm("LeftShoulder"), Norm("mixamorig:LeftShoulder"));
    }

    [Fact]
    public void Mixamo_FingerNumbering_IsConvertedToProxIntDist()
    {
        Assert.Equal(Norm("LeftIndexProximal"), Norm("mixamorig:LeftHandIndex1"));
        Assert.Equal(Norm("LeftIndexIntermediate"), Norm("mixamorig:LeftHandIndex2"));
        Assert.Equal(Norm("LeftIndexDistal"), Norm("mixamorig:LeftHandIndex3"));
        Assert.Equal(Norm("LeftThumbProximal"), Norm("mixamorig:LeftHandThumb1"));
        Assert.Equal(Norm("LeftMiddleIntermediate"), Norm("mixamorig:LeftHandMiddle2"));
        Assert.Equal(Norm("RightLittleDistal"), Norm("mixamorig:RightHandPinky3"));
        Assert.Equal(Norm("LeftLittleProximal"), Norm("mixamorig:LeftHandPinky1"));
    }

    [Fact]
    public void VRoid_JBipPrefixAndSingleLetterSide_IsConverted()
    {
        Assert.Equal(Norm("LeftUpperArm"), Norm("J_Bip_L_UpperArm"));
        Assert.Equal(Norm("RightUpperArm"), Norm("J_Bip_R_UpperArm"));
        Assert.Equal(Norm("LeftLowerArm"), Norm("J_Bip_L_LowerArm"));
        Assert.Equal(Norm("LeftHand"), Norm("J_Bip_L_Hand"));
        Assert.Equal(Norm("LeftIndexProximal"), Norm("J_Bip_L_Index1"));
        Assert.Equal(Norm("LeftIndexIntermediate"), Norm("J_Bip_L_Index2"));
        Assert.Equal(Norm("LeftThumbProximal"), Norm("J_Bip_L_Thumb1"));
    }

    [Fact]
    public void VRoid_CenterPrefix_IsStripped()
    {
        Assert.Equal(Norm("Hips"), Norm("J_Bip_C_Hips"));
        Assert.Equal(Norm("Spine"), Norm("J_Bip_C_Spine"));
        Assert.Equal(Norm("Chest"), Norm("J_Bip_C_Chest"));
        Assert.Equal(Norm("Neck"), Norm("J_Bip_C_Neck"));
        Assert.Equal(Norm("Head"), Norm("J_Bip_C_Head"));
    }

    [Fact]
    public void Biped_Bip01Prefix_IsConverted()
    {
        Assert.Equal(Norm("LeftUpperArm"), Norm("Bip01_L_UpperArm"));
        Assert.Equal(Norm("LeftLowerArm"), Norm("Bip01_L_Forearm"));
        Assert.Equal(Norm("LeftHand"), Norm("Bip01_L_Hand"));
        Assert.Equal(Norm("LeftUpperArm"), Norm("Bip001_L_UpperArm"));
    }

    [Fact]
    public void SingleLetterPrefix_DoesNotMatch_WhenPrecedingLeftRightAlready()
    {
        Assert.Equal(Norm("LeftUpperArm"), Norm("Left_UpperArm"));
        Assert.Equal(Norm("RightHand"), Norm("Right_Hand"));
    }

    [Fact]
    public void LRSuffix_DotNotation_NormalizesToTrailingLR()
    {
        Assert.Equal(Norm("LeftHand"), Norm("Hand.L"));
        Assert.Equal(Norm("RightHand"), Norm("Hand.R"));
        Assert.Equal(Norm("LeftHand"), Norm("Hand_L"));
        Assert.Equal(Norm("RightHand"), Norm("Hand-R"));
    }

    [Fact]
    public void BlenderDupSuffix_OnlyAtEnd_IsStripped()
    {
        Assert.Equal(Norm("Hand.L"), Norm("Hand.L.001"));
        Assert.Equal(Norm("Hand.L"), Norm("Hand.L.002"));
        Assert.Equal("bone001lhand", Norm("Bone001_L_Hand"));
    }

    [Fact]
    public void CaseInsensitive_PrefixStripping()
    {
        Assert.Equal(Norm("LeftHand"), Norm("MixamoRig:LeftHand"));
        Assert.Equal(Norm("LeftHand"), Norm("MIXAMORIG:LeftHand"));
        Assert.Equal(Norm("LeftUpperArm"), Norm("J_BIP_L_UpperArm"));
    }

    [Fact]
    public void NoMatch_ReturnsConsistentForUnknownBones()
    {
        Assert.Equal("breast", Norm("Breast"));
        Assert.Equal("breastl", Norm("Breast.L"));
        Assert.Equal("hair", Norm("Hair"));
    }
}
