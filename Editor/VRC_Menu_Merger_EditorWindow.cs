using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.Animations;
using UnityEngine.Rendering;

using VRC.SDKBase.Editor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Editor;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation;
using VRC.SDKBase.Validation.Performance.Stats;
using VRCStation = VRC.SDK3.Avatars.Components.VRCStation;
using VRC.SDK3.Validation;
using VRC.Core;
using VRCSDK2;

using PeanutTools_VRC_Menu_Merger;

public class VRC_Menu_Merger_EditorWindow : EditorWindow
{
    enum SuccessStates {
        Unknown,
        Success,
        Failed
    }

    enum Tab {
        Menus,
        Params,
        Animators
    }

    Vector2 scrollPosition;
    Tab selectedTab = 0;
    SuccessStates successState;
    UnityEngine.Object createdObject;

    UnityEngine.Object[] menuSources = new UnityEngine.Object[0] {};
    UnityEngine.Object[] paramsSources = new UnityEngine.Object[0] {};
    UnityEngine.Object[] animatorsSources = new UnityEngine.Object[0] {};

    string outputFileName = "";
    string outputPath = "";
    string pathToAssetToAdd = "";

    [MenuItem("PeanutTools/VRC Menu Merger")]
    public static void ShowWindow()
    {
        var window = GetWindow<VRC_Menu_Merger_EditorWindow>();
        window.titleContent = new GUIContent("VRC Menu Merger");
        window.minSize = new Vector2(250, 50);
    }

    string GetLabelForTabButton(Tab tab) {
        switch (tab) {
            case Tab.Menus:
                return "Menus";
            case Tab.Params:
                return "Params";
            case Tab.Animators:
                return "Animators";
            default:
                throw new System.Exception("Unknown tab!");
        }
    }

    void RenderTabButton(Tab tab) {
        var oldColor = GUI.backgroundColor;
        bool isSelected = selectedTab == tab;
        GUI.backgroundColor = isSelected ? Color.blue : oldColor;

        if (GUILayout.Button(GetLabelForTabButton(tab), GUILayout.Width(150), GUILayout.Height(25))) {
            SwitchToTab(tab);
        }

        GUI.backgroundColor = oldColor;
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUIStyle italicStyle = new GUIStyle(GUI.skin.label);
        italicStyle.fontStyle = FontStyle.Italic;

        GUILayout.Label("VRC Menu Merger", EditorStyles.boldLabel);
        GUILayout.Label("Merges one or more VRChat menus, parameter lists and animator controllers.", italicStyle);

        CustomGUI.LineGap();

        GUILayout.BeginHorizontal();
        RenderTabButton(Tab.Menus);
        RenderTabButton(Tab.Params);
        RenderTabButton(Tab.Animators);
        GUILayout.EndHorizontal();

        CustomGUI.LineGap();

        CustomGUI.RenderHelpInfo(GetTypeForFilePicker());
        
        CustomGUI.LineGap();
        CustomGUI.HorizontalRule();

        switch (selectedTab) {
            case Tab.Menus:
                menuSources = SourcesManager.RenderMenuMerger(menuSources, typeof(VRCExpressionsMenu));
                break;
            case Tab.Params:
                paramsSources = SourcesManager.RenderMenuMerger(paramsSources, typeof(VRCExpressionParameters));
                break;
            case Tab.Animators:
                animatorsSources = SourcesManager.RenderMenuMerger(animatorsSources, typeof(AnimatorController));
                break;
            default:
                throw new System.Exception("Unknown tab!");
        }
        
        CustomGUI.LineGap();
        CustomGUI.HorizontalRule();
        CustomGUI.LineGap();

        UnityEngine.Object existingFile;

        existingFile = EditorGUILayout.ObjectField("Replace existing file", null, GetTypeForFilePicker());

        if (existingFile != null) {
            SetOutputDetailsUsingPath(Utils.GetPathOfAsset(existingFile));
        }

        CustomGUI.LineGap();

        GUILayout.Label("New filename: ");

        outputFileName = EditorGUILayout.TextField(outputFileName);

        CustomGUI.RenderAssetFolderSelector(ref outputPath);

        CustomGUI.LineGap();

        GUILayout.Label("Outputting into:\n" + outputPath + "/" + outputFileName + "." + GetOutputFileExtension());
        
        CustomGUI.LineGap();

        EditorGUI.BeginDisabledGroup(outputFileName == "" || GetCurrentSources().Length < 2);
        if (GUILayout.Button("Merge!", GUILayout.Width(250), GUILayout.Height(50))) {
            Merge();
        }
        EditorGUI.EndDisabledGroup();

        if (successState == SuccessStates.Success) {
            CustomGUI.LineGap();
            CustomGUI.RenderSuccessMessage();
        }

        if (createdObject != null) {
            CustomGUI.LineGap();
                
            if (CustomGUI.StandardButton("View File")) {
                Utils.SelectAsset(createdObject);
            }
        }

        CustomGUI.LineGap();
        CustomGUI.HorizontalRule();
        CustomGUI.LineGap();

        CustomGUI.MyLinks();
        
        EditorGUILayout.EndScrollView();
    }

    void SetOutputDetailsUsingPath(string absolutePath) {
        string newFileName = Path.GetFileNameWithoutExtension(absolutePath);
        outputFileName = newFileName;
        
        string newOutputPath = absolutePath.Replace(Application.dataPath + "/", "").Replace("/" + Path.GetFileName(absolutePath), "").Replace(Path.GetFileName(absolutePath), "");
        outputPath = newOutputPath;
    }

    string GetDefaultOutputPath() {
        return "MergedAssets";
    }

    string GetDefaultOutputFileName() {
        switch (selectedTab) {
            case Tab.Menus:
                return "MergedVRCMenu";
            case Tab.Params:
                return "MergedVRCParameters";
            case Tab.Animators:
                return "MergedAnimatorController";
            default:
                throw new System.Exception("Unknown tab!");
        }
    }
    
    string GetOutputFileExtension() {
        switch (selectedTab) {
            case Tab.Menus:
            case Tab.Params:
                return "asset";
            case Tab.Animators:
                return "controller";
            default:
                throw new System.Exception("Unknown tab!");
        }
    }

    void SwitchToTab(Tab newTab) {
        selectedTab = newTab;
    }

    System.Type GetTypeForFilePicker() {
        switch (selectedTab) {
            case Tab.Menus:
                return typeof(VRCExpressionsMenu);
            case Tab.Params:
                return typeof(VRCExpressionParameters);
            case Tab.Animators:
                return typeof(AnimatorController);
            default:
                throw new System.Exception("Unknown selected tab!");
        }
    }

    void CreateOutputDirectories() {
        string absolutePathToFolder = Application.dataPath + "/" + outputPath;
        Directory.CreateDirectory(absolutePathToFolder);
    }

    string GetFinalOutputPathInsideProject() {
        return "Assets/" + outputPath + "/" + outputFileName + "." + GetOutputFileExtension();
    }

    void Merge() {
        successState = SuccessStates.Unknown;

        CreateOutputDirectories();

        switch (selectedTab) {
            case Tab.Menus:
                VRCExpressionsMenu newMenu = Menus.MergeMenus(GetCurrentSources());
                Menus.SaveMenu(newMenu, GetFinalOutputPathInsideProject());
                createdObject = newMenu;
                break;
            case Tab.Params:
                VRCExpressionParameters newParams = Params.MergeParams(GetCurrentSources());
                Params.SaveParams(newParams, GetFinalOutputPathInsideProject());
                createdObject = newParams;
                break;
            case Tab.Animators:
                AnimatorController newAnimatorController = Animators.MergeAnimators(GetCurrentSources());
                Animators.SaveAnimatorController(newAnimatorController, GetFinalOutputPathInsideProject());
                createdObject = newAnimatorController;
                break;
            default:
                throw new System.Exception("Unknown tab");
        }
        
        successState = SuccessStates.Success;

        HideSuccessMessageAfterDelay();
    }

    void AddSource(UnityEngine.GameObject newSource) {
        if (SourcesManager.GetDoesSourceAlreadyExists(GetCurrentSources(), newSource)) {
            return;
        }

        switch (selectedTab) {
            case Tab.Menus:
                menuSources = SourcesManager.AddSource(menuSources, newSource);
                break;
            case Tab.Params:
                paramsSources = SourcesManager.AddSource(paramsSources, newSource);
                break;
            case Tab.Animators:
                animatorsSources = SourcesManager.AddSource(animatorsSources, newSource);
                break;
            default:
                throw new System.Exception("Unknown tab!");
        }
    }

    UnityEngine.Object[] GetCurrentSources() {
        switch (selectedTab) {
            case Tab.Menus:
                return menuSources;
            case Tab.Params:
                return paramsSources;
            case Tab.Animators:
                return animatorsSources;
            default:
                throw new System.Exception("Unknown tab!");
        }
    }

    void SetCurrentSources(UnityEngine.Object[] newSources) {
        switch (selectedTab) {
            case Tab.Menus:
                menuSources = newSources;
                break;
            case Tab.Params:
                paramsSources = newSources;
                break;
            case Tab.Animators:
                animatorsSources = newSources;
                break;
            default:
                throw new System.Exception("Unknown tab!");
        }
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
