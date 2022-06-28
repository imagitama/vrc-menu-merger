using System.IO;
using UnityEngine;
using UnityEditor;

namespace PeanutTools_VRC_Menu_Merger {
    class Utils {
        public static string GetPathOfAsset(Object thing) {
            return AssetDatabase.GetAssetPath(thing);
        }

        public static string GetNameOfAsset(Object thing) {
            string pathInsideProject = AssetDatabase.GetAssetPath(thing);
            return Path.GetFileName(pathInsideProject);
        }

        public static void SelectAsset(Object thing) {
            Selection.activeObject = thing;
        }
    }
}