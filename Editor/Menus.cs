using System.Collections.Generic;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace PeanutTools_VRC_Menu_Merger {
    class Menus {
        public static void SaveMenu(VRCExpressionsMenu menu, string pathInsideProject) {
            AssetDatabase.CreateAsset(menu, pathInsideProject);
        }

        public static VRCExpressionsMenu MergeMenus(UnityEngine.Object[] sources) {
            VRCExpressionsMenu baseMenu = ScriptableObject.CreateInstance(typeof(VRCExpressionsMenu)) as VRCExpressionsMenu;

            List<VRCExpressionsMenu.Control> newControls = new List<VRCExpressionsMenu.Control>();

            foreach (UnityEngine.Object source in sources) {
                VRCExpressionsMenu menuToMerge = (VRCExpressionsMenu)source;

                foreach (VRCExpressionsMenu.Control controlToAdd in menuToMerge.controls)
                {
                    if (baseMenu.controls.Count == 8) {
                        throw new System.Exception("Cannot add control " + controlToAdd.name + " as we cannot add more than 8 controls to a single VRC menu");
                    }

                    int existingControlIndex = newControls.FindIndex(control => control.name == controlToAdd.name);

                    if (existingControlIndex > -1) {
                        Debug.Log("Control \"" + controlToAdd.name + "\" already exists, setting its sub-menu to the new one...");

                        // TODO: Expand this to merge more stuff
                        newControls[existingControlIndex].subMenu = controlToAdd.subMenu;
                    } else {
                        newControls.Add(controlToAdd);
                    }
                }
            }

            baseMenu.controls = newControls;

            return baseMenu;
        }
    }
}