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

public class VRC_Menu_Merger : EditorWindow
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

    [System.Serializable]
    public class Source {
        public string path;
    }

    Vector2 scrollPosition;
    Tab selectedTab = 0;
    SuccessStates successState;

    List<Source> menuSources = new List<Source>();
    List<Source> paramsSources = new List<Source>();
    List<Source> animatorsSources = new List<Source>();

    string outputFileName = "";
    string outputPath = "";
    string pathToAssetToAdd = "";

    [MenuItem("PeanutTools/VRC Menu Merger")]
    public static void ShowWindow()
    {
        var window = GetWindow<VRC_Menu_Merger>();
        window.titleContent = new GUIContent("VRC Menu Merger");
        window.minSize = new Vector2(250, 50);
    }

    void HorizontalRule() {
       Rect rect = EditorGUILayout.GetControlRect(false, 1);
       rect.height = 1;
       EditorGUI.DrawRect(rect, new Color ( 0.5f,0.5f,0.5f, 1 ) );
    }

    void LineGap() {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    void ForceRefresh() {
        GUI.FocusControl(null);
    }

    void RenderLink(string label, string url) {
        Rect rect = EditorGUILayout.GetControlRect();

        if (rect.Contains(Event.current.mousePosition)) {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (Event.current.type == EventType.MouseUp) {
                Help.BrowseURL(url);
            }
        }

        GUIStyle style = new GUIStyle();
        style.normal.textColor = new Color(0.5f, 0.5f, 1);

        GUI.Label(rect, label, style);
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

        LineGap();

        RenderTabButton(Tab.Menus);
        RenderTabButton(Tab.Params);
        RenderTabButton(Tab.Animators);

        LineGap();

        RenderHelpInfo();
        
        LineGap();
        HorizontalRule();
        LineGap();

        RenderSources();
        
        LineGap();
        HorizontalRule();
        LineGap();

        RenderAddSourceForm();
        
        LineGap();
        HorizontalRule();
        LineGap();

        if (GUILayout.Button("Select Existing File", GUILayout.Width(150), GUILayout.Height(25))) {
            string absolutePath = EditorUtility.OpenFilePanel("Select an existing file", Application.dataPath, GetAllowedExtensions());

            if (absolutePath != "") {
                SetOutputDetailsUsingPath(absolutePath);
            }

            ForceRefresh();
        }

        GUILayout.Label("New filename: ");

        outputFileName = EditorGUILayout.TextField(outputFileName);

        RenderAssetFolderSelector(ref outputPath);

        LineGap();

        GUILayout.Label("Outputting into:\n" + GetDisplayValueForPath(outputPath + "/" + outputFileName + "." + GetOutputFileExtension()));
        
        LineGap();

        EditorGUI.BeginDisabledGroup(outputFileName == "" || GetCurrentSources().Count < 2);
        if (GUILayout.Button("Merge!", GUILayout.Width(250), GUILayout.Height(50))) {
            Merge();
        }
        EditorGUI.EndDisabledGroup();

        if (successState == SuccessStates.Success) {
            LineGap();

            GUIStyle successLabelStyle = new GUIStyle(GUI.skin.label);
            successLabelStyle.normal.textColor = Color.green;
            GUILayout.Label("Merge successful!", successLabelStyle);
        }

        LineGap();
        HorizontalRule();
        LineGap();

        GUILayout.Label("Links:");

        RenderLink("  Download new versions from GitHub", "https://github.com/imagitama/vrc-menu-merger");
        RenderLink("  Get support from my Discord", "https://discord.gg/R6Scz6ccdn");
        RenderLink("  Follow me on Twitter", "https://twitter.com/@HiPeanutBuddha");
        
        EditorGUILayout.EndScrollView();
    }

    void SetOutputDetailsUsingPath(string absolutePath) {
        string newFileName = Path.GetFileNameWithoutExtension(absolutePath);
        outputFileName = newFileName;
        
        string newOutputPath = absolutePath.Replace(Application.dataPath + "/", "").Replace("/" + Path.GetFileName(absolutePath), "").Replace(Path.GetFileName(absolutePath), "");
        outputPath = newOutputPath;
    }
    
    void RenderHelpInfo() {
        string helpInfo = "";
        
        switch (selectedTab) {
            case Tab.Menus:
                helpInfo = "Copies each control from each menu into a single menu. Will error if you try and add more than the limit (8).";
                break;
            case Tab.Params:
                helpInfo = "Copies each parameter from each parameter list into a single list. Will error if you add a parameter that already exists with a different type.";
                break;
            case Tab.Animators:
                helpInfo = "Copies each layer and each parameter from each controller into a single controller. Will error if you add a parameter that already exists with a different type. Ignores duplicate layer names.";
                break;
            default:
                throw new System.Exception("Unknown index!");
        }

        GUIStyle myCustomStyle = new GUIStyle(GUI.skin.GetStyle("label"))
        {
            wordWrap = true
        };

        GUILayout.Label(helpInfo, myCustomStyle);
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
    
    string GetDisplayValueForPath(string path) {
        return "Assets" + path;
    }

    void SwitchToTab(Tab newTab) {
        selectedTab = newTab;
    }

    void RenderAddSourceForm() {
        GUILayout.Label("Add file:");

        RenderAssetFileSelector();

        LineGap();
    }

    string GetAllowedExtensions() {
        switch (selectedTab) {
            case Tab.Menus:
            case Tab.Params:
                return "asset";
            case Tab.Animators:
                return "controller";
            default:
                throw new System.Exception("Unknown index!");
        }
    }

    string RenderAssetFileSelector() {
        if (GUILayout.Button("Select File", GUILayout.Width(75), GUILayout.Height(25))) {
            string absolutePath = EditorUtility.OpenFilePanel("Select a file", Application.dataPath, GetAllowedExtensions());
            string relativePath = absolutePath.Replace(Application.dataPath + "/", "");

            if (relativePath != "") {
                AddSource(relativePath);
            }

            ForceRefresh();
        }
        
        return "";
    }

    string RenderAssetFolderSelector(ref string pathToUse) {
        GUILayout.Label("Path:");
        pathToUse = EditorGUILayout.TextField(pathToUse);
        
        if (GUILayout.Button("Select Folder", GUILayout.Width(100), GUILayout.Height(25))) {
            string absolutePath = EditorUtility.OpenFolderPanel("Select a folder", Application.dataPath, "");
            string relativePath = absolutePath.Replace(Application.dataPath + "/", "");
            pathToUse = "/" + relativePath;
            ForceRefresh();
        }
        
        return "";
    }

    void RenderSources() {
        var count = 0;

        if (GetCurrentSources().Count == 0) {
            GUILayout.Label("No sources defined");
        }

        foreach (Source source in GetCurrentSources()) {
            GUILayout.Label("\n" + source.path);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(count == 0);
            if (GUILayout.Button("^", GUILayout.Width(50), GUILayout.Height(25))) {
                MoveSourceUp(source);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(count == GetCurrentSources().Count - 1);
            if (GUILayout.Button("v", GUILayout.Width(50), GUILayout.Height(25))) {
                MoveSourceDown(source);
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("X", GUILayout.Width(50), GUILayout.Height(25))) {
                RemoveSource(source);
            }

            EditorGUILayout.EndHorizontal();

            count++;
        }
    }

    void CreateOutputDirectories() {
        string absolutePathToFolder = Application.dataPath + "/" + outputPath;
        Directory.CreateDirectory(absolutePathToFolder);
    }

    string GetFinalOutputPathInsideProject() {
        return "Assets/" + outputPath + "/" + outputFileName + "." + GetOutputFileExtension();
    }

    AnimatorController CreateTemplateAnimatorController() {
        string pathInsideProject = GetFinalOutputPathInsideProject();
        AnimatorController newAnimatorController = AnimatorController.CreateAnimatorControllerAtPath(pathInsideProject);
        return newAnimatorController;
    }

    void Merge() {
        successState = SuccessStates.Unknown;

        CreateOutputDirectories();

        switch (selectedTab) {
            case Tab.Menus:
                MergeMenus();
                break;
            case Tab.Params:
                MergeParams();
                break;
            case Tab.Animators:
                MergeAnimators();
                break;
            default:
                throw new System.Exception("Unknown tab");
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

    void MergeMenus() {
        VRCExpressionsMenu baseMenu = ScriptableObject.CreateInstance(typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;

        string pathToMenu = GetFinalOutputPathInsideProject();

        AssetDatabase.CreateAsset(baseMenu, pathToMenu);

        foreach (Source source in GetCurrentSources()) {
            string pathToImport = "Assets/" + source.path.Replace(Application.dataPath, "");
            VRCExpressionsMenu menuToMerge = (VRCExpressionsMenu)AssetDatabase.LoadAssetAtPath(pathToImport, typeof(VRCExpressionsMenu));
    
            foreach (VRCExpressionsMenu.Control control in menuToMerge.controls)
            {
                if (baseMenu.controls.Count == 8) {
                    throw new System.Exception("Cannot add control " + control.name + " from \"" + pathToImport + "\" add more than 8 controls to a single VRC menu");
                }

                baseMenu.controls.Add(control);
            }
        }
    }

    void MergeParams() {
        VRCExpressionParameters baseParams = ScriptableObject.CreateInstance(typeof(VRCExpressionParameters)) as VRCExpressionParameters;

        baseParams.parameters = new VRCExpressionParameters.Parameter[0];

        List<VRCExpressionParameters.Parameter> newParameters = new List<VRCExpressionParameters.Parameter>();

        foreach (Source source in GetCurrentSources()) {
            string pathToImport = "Assets/" + source.path.Replace(Application.dataPath, "");
            VRCExpressionParameters paramsToMerge = (VRCExpressionParameters)AssetDatabase.LoadAssetAtPath(pathToImport, typeof(VRCExpressionParameters));
    
            foreach (VRCExpressionParameters.Parameter parameterToAdd in paramsToMerge.parameters)
            {
                bool alreadyExists = false;

                foreach (VRCExpressionParameters.Parameter parameterToTest in newParameters) {
                    if (parameterToTest.name == parameterToAdd.name) {
                        alreadyExists = true;

                        if (parameterToTest.valueType != parameterToAdd.valueType) {
                            throw new System.Exception("Cannot add parameter " + parameterToAdd.name + " from VRC Params asset \"" + pathToImport + "\" as it already exists and is different type");
                        }
                    }
                }

                if (alreadyExists == false) {
                    newParameters.Add(parameterToAdd);
                }
            }
        }
        
        baseParams.parameters = newParameters.ToArray();

        string pathToAsset = GetFinalOutputPathInsideProject();

        AssetDatabase.CreateAsset(baseParams, pathToAsset);
    }

    void MergeAnimators() {
        AnimatorController baseAnimatorController = CreateTemplateAnimatorController();

        List<AnimatorControllerLayer> newLayers = new List<AnimatorControllerLayer>();
        List<AnimatorControllerParameter> newParameters = new List<AnimatorControllerParameter>();

        foreach (Source source in GetCurrentSources()) {
            string pathToImport = "Assets/" + source.path.Replace(Application.dataPath, "");
            AnimatorController animatorControllerToMerge = (AnimatorController)AssetDatabase.LoadAssetAtPath(pathToImport, typeof(AnimatorController));

            AnimatorControllerLayer[] layersToAdd = animatorControllerToMerge.layers;

            foreach (AnimatorControllerLayer layerToAdd in layersToAdd) {
                foreach (AnimatorControllerLayer layerToTest in newLayers) {
                    if (layerToTest.name == layerToAdd.name) {
                        Debug.LogWarning("Layer \"" + layerToAdd.name + "\" from controller \"" + pathToImport + "\" already exists in the new controller, adding anyway...");
                    }
                }

                newLayers.Add(layerToAdd);
            }

            AnimatorControllerParameter[] parametersToAdd = animatorControllerToMerge.parameters;

            foreach (AnimatorControllerParameter parameterToAdd in parametersToAdd) {
                bool alreadyExists = false;

                foreach (AnimatorControllerParameter parameterToTest in newParameters) {
                    if (parameterToTest.name == parameterToAdd.name) {
                        alreadyExists = true;

                        if (parameterToTest.type != parameterToAdd.type) {
                            throw new System.Exception("Cannot add parameter " + parameterToAdd.name + " from controller \"" + pathToImport + "\" as it already exists and is different type");
                        }
                    }
                }

                if (alreadyExists == false) {
                    newParameters.Add(parameterToAdd);
                }
            }
        }

        baseAnimatorController.parameters = newParameters.ToArray();
        baseAnimatorController.layers = newLayers.ToArray();
    }

    void CheckIfSourcePathAlreadyExists(string pathToAsset, List<Source> sources) {
        if (sources.Exists(x => x.path == pathToAsset)) {
            throw new System.Exception("Cannot add source as it already exists");
        }
    }

    void AddSource(string pathToAsset) {
        CheckIfSourcePathAlreadyExists(pathToAsset, GetCurrentSources());

        switch (selectedTab) {
            case Tab.Menus:
                menuSources.Add(new Source() {
                    path = pathToAsset
                });
                break;
            case Tab.Params:
                paramsSources.Add(new Source() {
                    path = pathToAsset
                });
                break;
            case Tab.Animators:
                animatorsSources.Add(new Source() {
                    path = pathToAsset
                });
                break;
            default:
                throw new System.Exception("Unknown tab!");
        }
    }

    List<Source> GetCurrentSources() {
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

    void SetCurrentSources(List<Source> newSources) {
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

    void RemoveSource(Source source) {
        // fix error when editing list in place
        List<Source> newSources = GetCurrentSources().ToArray().ToList();
        newSources.Remove(source);
        SetCurrentSources(newSources);
    }
    
    void MoveSourceUp(Source source) {
        // fix error when editing list in place
        List<Source> newSources = GetCurrentSources().ToArray().ToList();
        int idx = newSources.IndexOf(source);
        newSources.RemoveAt(idx);
        newSources.Insert(idx - 1, source);
        SetCurrentSources(newSources);
    }

    void MoveSourceDown(Source source) {
        // fix error when editing list in place
        List<Source> newSources = GetCurrentSources().ToArray().ToList();
        int idx = newSources.IndexOf(source);
        newSources.RemoveAt(idx);
        newSources.Insert(idx + 1, source);
        SetCurrentSources(newSources);
    }
}
