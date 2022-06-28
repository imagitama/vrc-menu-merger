using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace PeanutTools_VRC_Menu_Merger {
    public class SourcesManager {
        public static UnityEngine.Object[] RenderMenuMerger(UnityEngine.Object[] sources, Type sourceType) {
            UnityEngine.Object[] newSources = sources;
            var count = 0;

            if (sources.Length == 0) {
                CustomGUI.ItalicLabel("No sources defined");
            }

            foreach (UnityEngine.Object source in sources) {
                GUILayout.Label("\n" + Utils.GetPathOfAsset(source));

                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginDisabledGroup(count == 0);
                if (CustomGUI.TinyButton("^")) {
                    newSources = MoveSourceUp(sources, source);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(count == sources.Length - 1);
                if (CustomGUI.TinyButton("v")) {
                    newSources = MoveSourceDown(sources, source);
                }
                EditorGUI.EndDisabledGroup();

                if (CustomGUI.TinyButton("x")) {
                    newSources = RemoveSource(sources, source);
                }

                if (CustomGUI.TinyButton("View")) {
                    Utils.SelectAsset(source);
                }

                EditorGUILayout.EndHorizontal();

                count++;
            }

            CustomGUI.LineGap();

            return RenderAddSourceForm(newSources, sourceType);
        }

        public static UnityEngine.Object[] RenderAddSourceForm(UnityEngine.Object[] sources, Type sourceType) {
            UnityEngine.Object[] newSources = sources;

            UnityEngine.Object newSource;
    
            newSource = EditorGUILayout.ObjectField("Select a file", null, sourceType);

            if (newSource != null) {
                newSources = AddSource(sources, newSource);
            }

            CustomGUI.LineGap();

            return newSources;
        }

        public static bool GetDoesSourceAlreadyExists(UnityEngine.Object[] sources, UnityEngine.Object source) {
            return Array.IndexOf(sources, source) > -1;
        }

        public static UnityEngine.Object[] AddSource(UnityEngine.Object[] sources, UnityEngine.Object newSource) {
            Debug.Log("Adding source \"" + newSource + "\"...");

            if (GetDoesSourceAlreadyExists(sources, newSource) == true) {
                Debug.Log("Tried to add source " + newSource + " but it already exists");
                return sources;
            }

            List<UnityEngine.Object> newSources = sources.ToList();
            newSources.Add(newSource);
            return newSources.ToArray();
        }

        public static UnityEngine.Object[] RemoveSource(UnityEngine.Object[] sources, UnityEngine.Object source) {
            Debug.Log("Removing source \"" + source + "\"...");

            if (GetDoesSourceAlreadyExists(sources, source) == false) {
                return sources;
            }

            List<UnityEngine.Object> newSources = sources.ToList();
            newSources.Remove(source);
            return newSources.ToArray();
        }
        
        public static UnityEngine.Object[] MoveSourceUp(UnityEngine.Object[] sources, UnityEngine.Object source) {
            Debug.Log("Moving source \"" + source + "\" up...");

            List<UnityEngine.Object> sourcesList = sources.ToList();
            int idx = sourcesList.IndexOf(source);
            sourcesList.RemoveAt(idx);
            sourcesList.Insert(idx - 1, source);
            return sourcesList.ToArray();
        }

        public static UnityEngine.Object[] MoveSourceDown(UnityEngine.Object[] sources, UnityEngine.Object source) {
            Debug.Log("Moving source \"" + source + "\" down...");

            List<UnityEngine.Object> sourcesList = sources.ToList();
            int idx = sourcesList.IndexOf(source);
            sourcesList.RemoveAt(idx);
            sourcesList.Insert(idx + 1, source);
            return sourcesList.ToArray();
        }
    }
}