using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VRC_Menu_Merger {
    class CustomGUI {
        public static void LineGap() {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        public static void ItalicLabel(string text) {
            GUIStyle italicStyle = new GUIStyle(GUI.skin.label);
            italicStyle.fontStyle = FontStyle.Italic;
            GUILayout.Label(text, italicStyle);
        }

        public static void LargeLabel(string text) {
            GUIStyle italicStyle = new GUIStyle(GUI.skin.label);
            italicStyle.fontSize = 20;
            GUILayout.Label(text, italicStyle);
        }

        public static void BoldLabel(string text) {
            GUILayout.Label(text, EditorStyles.boldLabel);
        }

        public static void MyLinks() {
            GUILayout.Label("Links:");

            RenderLink("  Download new versions from GitHub", "https://github.com/imagitama/vrc-menu-merger");
            RenderLink("  Get support from my Discord", "https://discord.gg/R6Scz6ccdn");
            RenderLink("  Follow me on Twitter", "https://twitter.com/@HiPeanutBuddha");
        }

        public static void HorizontalRule() {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color ( 0.5f,0.5f,0.5f, 1 ) );
        }

        public static void ForceRefresh() {
            GUI.FocusControl(null);
        }

        public static void RenderLink(string label, string url) {
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

        public static bool PrimaryButton(string label) {
            return GUILayout.Button(label, GUILayout.Width(250), GUILayout.Height(50));
        }

        public static bool StandardButton(string label) {
            return GUILayout.Button(label, GUILayout.Width(150), GUILayout.Height(25));
        }

        public static bool TinyButton(string label) {
            return GUILayout.Button(label, GUILayout.Width(50), GUILayout.Height(15));
        }

        public static string RenderAssetFolderSelector(ref string pathToUse) {
            GUILayout.Label("Path:");
            pathToUse = EditorGUILayout.TextField(pathToUse);
            
            if (CustomGUI.StandardButton("Select Folder")) {
                string absolutePath = EditorUtility.OpenFolderPanel("Select a folder", Application.dataPath, "");
                string pathInsideProject = absolutePath.Replace(Application.dataPath + "/", "").Replace(Application.dataPath, "");
                pathToUse = pathInsideProject;
                CustomGUI.ForceRefresh();
            }
            
            return "";
        }

        public static string GetHelpText(System.Type type) {
            if (type == typeof(VRCExpressionsMenu)) {
                return  "Copies each control from each menu into a single menu. On conflict will overwrite the sub-menu. Will error if you try and add more than the limit (8).";
            } else if (type == typeof(VRCExpressionParameters)) {
                return "Copies each parameter from each parameter list into a single list. Will error if you add a parameter that already exists with a different type.";
            } else {
                return "Copies each layer and each parameter from each controller into a single controller. Will error if you add a parameter that already exists with a different type. Ignores duplicate layer names.";
            }
        }
            
        public static void RenderHelpInfo(System.Type type) {
            GUIStyle myCustomStyle = new GUIStyle(GUI.skin.GetStyle("label"))
            {
                fontStyle = FontStyle.Italic,
                wordWrap = true
            };

            GUILayout.Label(GetHelpText(type), myCustomStyle);
        }

        public static void RenderSuccessMessage() {
             GUIStyle successLabelStyle = new GUIStyle(GUI.skin.label);
            successLabelStyle.normal.textColor = Color.green;
            GUILayout.Label("Merge successful!", successLabelStyle);
        }
    }
}