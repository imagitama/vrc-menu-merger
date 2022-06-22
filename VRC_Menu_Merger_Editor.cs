using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

using VRC_Menu_Merger;

[System.Serializable]
[CustomEditor(typeof(VRC_Menu_Merger_Instance))]
public class VRC_Menu_Merger_Editor : Editor
{
    SuccessStates successState;

    enum SuccessStates {
        Unknown,
        Success,
        Failed
    }

    VRCAvatarDescriptor sourceVrcAvatarDescriptor;

    private string customDirectoryPathInsideAssets = "";

    private bool overrideBase = false;
    private bool overrideAdditive = false;
    private bool overrideGesture = false;
    private bool overrideAction = false;
    private bool overrideFX = false;

    private bool overrideCustomMenu = false;
    private bool overrideCustomParams = false;

    private UnityEngine.Object[] sourcesForBase = new UnityEngine.Object[0] {};
    private UnityEngine.Object[] sourcesForAdditive = new UnityEngine.Object[0] {};
    private UnityEngine.Object[] sourcesForGesture = new UnityEngine.Object[0] {};
    private UnityEngine.Object[] sourcesForAction = new UnityEngine.Object[0] {};
    private UnityEngine.Object[] sourcesForFX = new UnityEngine.Object[0] {};

    private UnityEngine.Object[] sourcesForCustomMenu = new UnityEngine.Object[0] {};
    private UnityEngine.Object[] sourcesForCustomParams = new UnityEngine.Object[0] {};
    
    void OnEnable() {
        SyncFromSerializedObject();
    }

    void SyncFromSerializedObject() {
        // use a serialized object (which is linked by the CustomEditor attribute) to persist data between opens
        sourceVrcAvatarDescriptor = (VRCAvatarDescriptor)serializedObject.FindProperty("sourceVrcAvatarDescriptor").objectReferenceValue;

        customDirectoryPathInsideAssets = serializedObject.FindProperty("customDirectoryPathInsideAssets").stringValue;

        overrideBase = serializedObject.FindProperty("overrideBase").boolValue;
        overrideAdditive = serializedObject.FindProperty("overrideAdditive").boolValue;
        overrideGesture = serializedObject.FindProperty("overrideGesture").boolValue;
        overrideAction = serializedObject.FindProperty("overrideAction").boolValue;
        overrideFX = serializedObject.FindProperty("overrideFX").boolValue;
        overrideCustomMenu = serializedObject.FindProperty("overrideCustomMenu").boolValue;
        overrideCustomParams = serializedObject.FindProperty("overrideCustomParams").boolValue;

        SerializedProperty prop;
        prop = serializedObject.FindProperty("sourcesForBase");
        sourcesForBase = new AnimatorController[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++) {
            sourcesForBase[i] = (AnimatorController)prop.GetArrayElementAtIndex(i).objectReferenceValue;
        }

        prop = serializedObject.FindProperty("sourcesForAdditive");
        sourcesForAdditive = new AnimatorController[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++) {
            sourcesForAdditive[i] = (AnimatorController)prop.GetArrayElementAtIndex(i).objectReferenceValue;
        }

        prop = serializedObject.FindProperty("sourcesForGesture");
        sourcesForGesture = new AnimatorController[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++) {
            sourcesForGesture[i] = (AnimatorController)prop.GetArrayElementAtIndex(i).objectReferenceValue;
        }

        prop = serializedObject.FindProperty("sourcesForAction");
        sourcesForAction = new AnimatorController[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++) {
            sourcesForAction[i] = (AnimatorController)prop.GetArrayElementAtIndex(i).objectReferenceValue;
        }

        prop = serializedObject.FindProperty("sourcesForFX");
        sourcesForFX = new AnimatorController[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++) {
            sourcesForFX[i] = (AnimatorController)prop.GetArrayElementAtIndex(i).objectReferenceValue;
        }

        prop = serializedObject.FindProperty("sourcesForCustomMenu");
        sourcesForCustomMenu = new VRCExpressionsMenu[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++) {
            sourcesForCustomMenu[i] = (VRCExpressionsMenu)prop.GetArrayElementAtIndex(i).objectReferenceValue;
        }

        prop = serializedObject.FindProperty("sourcesForCustomParams");
        sourcesForCustomParams = new VRCExpressionParameters[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++) {
            sourcesForCustomParams[i] = (VRCExpressionParameters)prop.GetArrayElementAtIndex(i).objectReferenceValue;
        }
    }

    public override void OnInspectorGUI()
    {
        CustomGUI.BoldLabel("VRC Menu Merger");
        CustomGUI.ItalicLabel("Merges one or more VRChat menus, parameter lists and animator controllers.");

        CustomGUI.LineGap();

        sourceVrcAvatarDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar", sourceVrcAvatarDescriptor, typeof(VRCAvatarDescriptor));

        CustomGUI.LineGap();

        if (sourceVrcAvatarDescriptor != null) {
            GUILayout.Label("Change default path of merged assets:");
            CustomGUI.RenderAssetFolderSelector(ref customDirectoryPathInsideAssets);
            
            CustomGUI.LineGap();
            CustomGUI.HorizontalRule();
            CustomGUI.LineGap();

            RenderDetectedAvatar();
        }
        
        CustomGUI.LineGap();
        CustomGUI.HorizontalRule();
        CustomGUI.LineGap();

        EditorGUI.BeginDisabledGroup(GetIsReadyForMerge() == false);
        if (CustomGUI.PrimaryButton("Merge and apply")) {
            Merge();
        }
        EditorGUI.EndDisabledGroup();

        if (successState == SuccessStates.Success) {
            CustomGUI.LineGap();
            CustomGUI.RenderSuccessMessage();
        }

        CustomGUI.LineGap();
        CustomGUI.HorizontalRule();
        CustomGUI.LineGap();

        CustomGUI.MyLinks();

        SyncToSerializedObject();
    }
    
    void SyncToSerializedObject() {
        serializedObject.FindProperty("sourceVrcAvatarDescriptor").objectReferenceValue = sourceVrcAvatarDescriptor;

        serializedObject.FindProperty("customDirectoryPathInsideAssets").stringValue = customDirectoryPathInsideAssets;

        serializedObject.FindProperty("overrideBase").boolValue = overrideBase;
        serializedObject.FindProperty("overrideAdditive").boolValue = overrideAdditive;
        serializedObject.FindProperty("overrideGesture").boolValue = overrideGesture;
        serializedObject.FindProperty("overrideAction").boolValue = overrideAction;
        serializedObject.FindProperty("overrideFX").boolValue = overrideFX;
        serializedObject.FindProperty("overrideCustomMenu").boolValue = overrideCustomMenu;
        serializedObject.FindProperty("overrideCustomParams").boolValue = overrideCustomParams;

        SerializedProperty prop;
        prop = serializedObject.FindProperty("sourcesForBase");
        prop.ClearArray();
        for (int i = 0; i < sourcesForBase.Length; i++) {
            prop.InsertArrayElementAtIndex(i);
            SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);
            arrayElement.objectReferenceValue = sourcesForBase[i];
        }

        prop = serializedObject.FindProperty("sourcesForAdditive");
        prop.ClearArray();
        for (int i = 0; i < sourcesForAdditive.Length; i++) {
            prop.InsertArrayElementAtIndex(i);
            SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);
            arrayElement.objectReferenceValue = sourcesForAdditive[i];
        }

        prop = serializedObject.FindProperty("sourcesForGesture");
        prop.ClearArray();
        for (int i = 0; i < sourcesForGesture.Length; i++) {
            prop.InsertArrayElementAtIndex(i);
            SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);
            arrayElement.objectReferenceValue = sourcesForGesture[i];
        }

        prop = serializedObject.FindProperty("sourcesForAction");
        prop.ClearArray();
        for (int i = 0; i < sourcesForAction.Length; i++) {
            prop.InsertArrayElementAtIndex(i);
            SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);
            arrayElement.objectReferenceValue = sourcesForAction[i];
        }

        prop = serializedObject.FindProperty("sourcesForFX");
        prop.ClearArray();
        for (int i = 0; i < sourcesForFX.Length; i++) {
            prop.InsertArrayElementAtIndex(i);
            SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);
            arrayElement.objectReferenceValue = sourcesForFX[i];
        }

        prop = serializedObject.FindProperty("sourcesForCustomMenu");
        prop.ClearArray();
        for (int i = 0; i < sourcesForCustomMenu.Length; i++) {
            prop.InsertArrayElementAtIndex(i);
            SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);
            arrayElement.objectReferenceValue = sourcesForCustomMenu[i];
        }

        prop = serializedObject.FindProperty("sourcesForCustomParams");
        prop.ClearArray();
        for (int i = 0; i < sourcesForCustomParams.Length; i++) {
            prop.InsertArrayElementAtIndex(i);
            SerializedProperty arrayElement = prop.GetArrayElementAtIndex(i);
            arrayElement.objectReferenceValue = sourcesForCustomParams[i];
        }

        serializedObject.ApplyModifiedProperties();
    }

    bool GetIsReadyForMerge() {
        if (
            overrideBase == false && 
            overrideAdditive == false && 
            overrideGesture == false && 
            overrideAction == false && 
            overrideFX == false && 
            overrideCustomMenu == false && 
            overrideCustomParams == false
        ) {
            return false;
        }
        if (overrideBase == true && sourcesForBase.Length < 2) {
            return false;
        }
        if (overrideAdditive == true && sourcesForAdditive.Length < 2) {
            return false;
        }
        if (overrideGesture == true && sourcesForGesture.Length < 2) {
            return false;
        }
        if (overrideAction == true && sourcesForAction.Length < 2) {
            return false;
        }
        if (overrideFX == true && sourcesForFX.Length < 2) {
            return false;
        }
        if (overrideCustomMenu == true && sourcesForCustomMenu.Length < 2) {
            return false;
        }
        if (overrideCustomParams == true && sourcesForCustomParams.Length < 2) {
            return false;
        }
        return true;
    }

    void RenderCustomAnimLayer(VRCAvatarDescriptor.CustomAnimLayer customAnimLayer, int idx) {
        AnimatorController controller = customAnimLayer.animatorController as AnimatorController;

        CustomGUI.BoldLabel(customAnimLayer.type.ToString());

        if (controller != null) {
            GUILayout.BeginHorizontal();

            GUILayout.Label("Existing: " + controller.name);
            
            if (CustomGUI.TinyButton("View")) {
                Utils.SelectAsset(controller);
            }

            GUILayout.EndHorizontal();
        }

        bool isToOverride = false;
        UnityEngine.Object[] sources = new UnityEngine.Object[0];

        // TODO: Make nested arrays for this
        switch (idx) {
            case 0:
                isToOverride = overrideBase;
                sources = sourcesForBase;
                break;

            case 1:
                isToOverride = overrideAdditive;
                sources = sourcesForAdditive;
                break;

            case 2:
                isToOverride = overrideGesture;
                sources = sourcesForGesture;
                break;

            case 3:
                isToOverride = overrideAction;
                sources = sourcesForAction;
                break;

            case 4:
                isToOverride = overrideFX;
                sources = sourcesForFX;
                break;
        }

        isToOverride = EditorGUILayout.Toggle("Override with controller", isToOverride);
        
        if (isToOverride == true) {
            UnityEngine.Object[] newSources = SourcesManager.RenderMenuMerger(sources, typeof(AnimatorController));
            GUILayout.Label("Output: " + GetPathInsideAssetsForSavedAssets(GetFileNameForCustomAnimLayer(customAnimLayer.type)));

            switch (idx) {
                case 0:
                    sourcesForBase = newSources;
                    break;

                case 1:
                    sourcesForAdditive = newSources;
                    break;

                case 2:
                    sourcesForGesture = newSources;
                    break;

                case 3:
                    sourcesForAction = newSources;
                    break;

                case 4:
                    sourcesForFX = newSources;
                    break;
            }
        }

        switch (idx) {
            case 0:
                overrideBase = isToOverride;
                break;

            case 1:
                overrideAdditive = isToOverride;
                break;

            case 2:
                overrideGesture = isToOverride;
                break;

            case 3:
                overrideAction = isToOverride;
                break;

            case 4:
                overrideFX = isToOverride;
                break;
        }
        
        CustomGUI.LineGap();
    }

    string GetAvatarName() {
        return sourceVrcAvatarDescriptor.gameObject.name;
    }

    string GetFileNameForCustomAnimLayer(VRCAvatarDescriptor.AnimLayerType type) {
        return type.ToString() + " (merged).controller";
    }

    string GetFileNameForCustomMenu() {
        return "VRCMenu (merged).asset";
    }

    string GetFileNameForCustomParams() {
        return "VRCParameters (merged).asset";
    }

    void RenderDetectedAvatar() {
        CustomGUI.LargeLabel("Playable Layers");
        CustomGUI.RenderHelpInfo(typeof(AnimatorController));
        
        CustomGUI.LineGap();

        int idx = 0;

        foreach (VRCAvatarDescriptor.CustomAnimLayer customAnimLayer in sourceVrcAvatarDescriptor.baseAnimationLayers) {
            RenderCustomAnimLayer(customAnimLayer, idx);

            CustomGUI.HorizontalRule();
            CustomGUI.LineGap();

            idx++;
        }

        VRCExpressionsMenu menu = sourceVrcAvatarDescriptor.expressionsMenu;

        CustomGUI.LargeLabel("Custom Menu");
        CustomGUI.RenderHelpInfo(typeof(VRCExpressionsMenu));

        CustomGUI.LineGap();

        if (menu != null) {
            CustomGUI.LineGap();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Existing: " + Utils.GetPathOfAsset(menu));
            
            if (CustomGUI.TinyButton("View")) {
                Utils.SelectAsset(menu);
            }

            GUILayout.EndHorizontal();
        }
        
        overrideCustomMenu = EditorGUILayout.Toggle("Override with menu", overrideCustomMenu);

        if (overrideCustomMenu == true) {
            CustomGUI.LineGap();

            sourcesForCustomMenu = SourcesManager.RenderMenuMerger(sourcesForCustomMenu, typeof(VRCExpressionsMenu));

            GUILayout.Label("Output: " + GetPathInsideAssetsForSavedAssets(GetFileNameForCustomMenu()));
        }
        
        CustomGUI.LineGap();
        CustomGUI.HorizontalRule();
        CustomGUI.LineGap();

        VRCExpressionParameters myParams = sourceVrcAvatarDescriptor.expressionParameters;

        CustomGUI.LargeLabel("Custom Params");
        
        CustomGUI.RenderHelpInfo(typeof(VRCExpressionParameters));

        if (myParams != null) {
            CustomGUI.LineGap();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Existing: " + Utils.GetPathOfAsset(myParams));
            
            if (CustomGUI.TinyButton("View")) {
                Utils.SelectAsset(myParams);
            }

            GUILayout.EndHorizontal();
        }

        overrideCustomParams = EditorGUILayout.Toggle("Override with params", overrideCustomParams);

        if (overrideCustomParams == true) {
            sourcesForCustomParams = SourcesManager.RenderMenuMerger(sourcesForCustomParams, typeof(VRCExpressionParameters));
            GUILayout.Label("Output: " + GetPathInsideAssetsForSavedAssets(GetFileNameForCustomParams()));
        }
    }

    void CreateOutputDirectories() {
        string absolutePathToFolder = Application.dataPath + "/" + GetDirectoryPathInsideAssetsForSavedAssets();
        Debug.Log(absolutePathToFolder);
        Directory.CreateDirectory(absolutePathToFolder);
    }

    string GetDirectoryPathInsideAssetsForSavedAssets() {
        if (customDirectoryPathInsideAssets != "") {
            return customDirectoryPathInsideAssets;
        }

        string avatarName = GetAvatarName();

        string dir = avatarName;

        return dir;
    }

    string GetPathInsideAssetsForSavedAssets(string fileName) {
        return GetDirectoryPathInsideAssetsForSavedAssets() + "/" + fileName;
    }

    void SetAvatarCustomMenu(VRCExpressionsMenu menuToSet) {
        sourceVrcAvatarDescriptor.expressionsMenu = menuToSet;
    }

    void SetAvatarCustomParams(VRCExpressionParameters paramsToSet) {
        sourceVrcAvatarDescriptor.expressionParameters = paramsToSet;
    }

    void Merge() {
        successState = SuccessStates.Unknown;

        CreateOutputDirectories();

        sourceVrcAvatarDescriptor.customizeAnimationLayers = true;
        sourceVrcAvatarDescriptor.customExpressions = true;

        if (overrideBase) {
            AnimatorController newAnimatorController = Animators.MergeAnimators(sourcesForBase);
            Animators.SaveAnimatorController(newAnimatorController, "Assets/" + GetPathInsideAssetsForSavedAssets("Base (merged).controller"));
            sourceVrcAvatarDescriptor.baseAnimationLayers[0].animatorController = newAnimatorController;
            sourceVrcAvatarDescriptor.baseAnimationLayers[0].isDefault = false;
        }

        if (overrideAdditive) {
            AnimatorController newAnimatorController = Animators.MergeAnimators(sourcesForAdditive);
            Animators.SaveAnimatorController(newAnimatorController, "Assets/" + GetPathInsideAssetsForSavedAssets("Additive (merged).controller"));
            sourceVrcAvatarDescriptor.baseAnimationLayers[1].animatorController = newAnimatorController;
            sourceVrcAvatarDescriptor.baseAnimationLayers[1].isDefault = false;
        }

        if (overrideGesture) {
            AnimatorController newAnimatorController = Animators.MergeAnimators(sourcesForGesture);
            Animators.SaveAnimatorController(newAnimatorController, "Assets/" + GetPathInsideAssetsForSavedAssets("Gesture (merged).controller"));
            sourceVrcAvatarDescriptor.baseAnimationLayers[2].animatorController = newAnimatorController;
            sourceVrcAvatarDescriptor.baseAnimationLayers[2].isDefault = false;
        }

        if (overrideAction) {
            AnimatorController newAnimatorController = Animators.MergeAnimators(sourcesForAction);
            Animators.SaveAnimatorController(newAnimatorController, "Assets/" + GetPathInsideAssetsForSavedAssets("Action (merged).controller"));
            sourceVrcAvatarDescriptor.baseAnimationLayers[3].animatorController = newAnimatorController;
            sourceVrcAvatarDescriptor.baseAnimationLayers[3].isDefault = false;
        }

        if (overrideFX) {
            AnimatorController newAnimatorController = Animators.MergeAnimators(sourcesForFX);
            Animators.SaveAnimatorController(newAnimatorController, "Assets/" + GetPathInsideAssetsForSavedAssets("FX (merged).controller"));
            sourceVrcAvatarDescriptor.baseAnimationLayers[4].animatorController = newAnimatorController;
            sourceVrcAvatarDescriptor.baseAnimationLayers[4].isDefault = false;
        }

        if (overrideCustomMenu) {
            VRCExpressionsMenu newMenu = Menus.MergeMenus(sourcesForCustomMenu);
            Menus.SaveMenu(newMenu, "Assets/" + GetPathInsideAssetsForSavedAssets(GetFileNameForCustomMenu()));
            SetAvatarCustomMenu(newMenu);
        }

        if (overrideCustomParams) {
            VRCExpressionParameters newParams = Params.MergeParams(sourcesForCustomParams);
            Params.SaveParams(newParams, "Assets/" + GetPathInsideAssetsForSavedAssets(GetFileNameForCustomParams()));
            SetAvatarCustomParams(newParams);
        }

        successState = SuccessStates.Success;

        HideSuccessMessageAfterDelay();
    }

    async Task HideSuccessMessageAfterDelay()
    {   
        await Task.Run(() => ResetSuccessState());
    }
    
    void ResetSuccessState() {
        Thread.Sleep(2000);
        successState = SuccessStates.Unknown;
    }
 }
 