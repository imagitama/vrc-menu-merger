using UnityEngine;
using UnityEngine.Animations;

using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

public class VRC_Menu_Merger_Instance : MonoBehaviour
{
    public VRCAvatarDescriptor sourceVrcAvatarDescriptor;

    public string customDirectoryPathInsideAssets;

    public bool overrideBase = false;
    public bool overrideAdditive = false;
    public bool overrideGesture = false;
    public bool overrideAction = false;
    public bool overrideFX = false;

    public bool overrideCustomMenu = false;
    public bool overrideCustomParams = false;

    public RuntimeAnimatorController[] sourcesForBase = new RuntimeAnimatorController[0] {};
    public RuntimeAnimatorController[] sourcesForAdditive = new RuntimeAnimatorController[0] {};
    public RuntimeAnimatorController[] sourcesForGesture = new RuntimeAnimatorController[0] {};
    public RuntimeAnimatorController[] sourcesForAction = new RuntimeAnimatorController[0] {};
    public RuntimeAnimatorController[] sourcesForFX = new RuntimeAnimatorController[0] {};

    public VRCExpressionsMenu[] sourcesForCustomMenu = new VRCExpressionsMenu[0] {};
    public VRCExpressionParameters[] sourcesForCustomParams = new VRCExpressionParameters[0] {};
}