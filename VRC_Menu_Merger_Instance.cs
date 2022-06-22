using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC_Menu_Merger;
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

    public AnimatorController[] sourcesForBase = new AnimatorController[0] {};
    public AnimatorController[] sourcesForAdditive = new AnimatorController[0] {};
    public AnimatorController[] sourcesForGesture = new AnimatorController[0] {};
    public AnimatorController[] sourcesForAction = new AnimatorController[0] {};
    public AnimatorController[] sourcesForFX = new AnimatorController[0] {};

    public VRCExpressionsMenu[] sourcesForCustomMenu = new VRCExpressionsMenu[0] {};
    public VRCExpressionParameters[] sourcesForCustomParams = new VRCExpressionParameters[0] {};
}