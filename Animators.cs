using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

namespace VRC_Menu_Merger {
    class Animators {
        public static void SaveAnimatorController(AnimatorController animatorController, string pathInsideProject) {
            AssetDatabase.CreateAsset(animatorController, pathInsideProject);
        }

        public static AnimatorController CreateTemplateAnimatorController(string pathInsideProject) {
            AnimatorController newAnimatorController = AnimatorController.CreateAnimatorControllerAtPath(pathInsideProject);
            return newAnimatorController;
        }

        public static AnimatorController MergeAnimators(UnityEngine.Object[] sources) {
            AnimatorController baseAnimatorController = new AnimatorController();

            List<AnimatorControllerLayer> newLayers = new List<AnimatorControllerLayer>();
            List<AnimatorControllerParameter> newParameters = new List<AnimatorControllerParameter>();

            foreach (UnityEngine.Object source in sources) {
                AnimatorController animatorControllerToMerge = (AnimatorController)source;

                AnimatorControllerLayer[] layersToAdd = animatorControllerToMerge.layers;

                foreach (AnimatorControllerLayer layerToAdd in layersToAdd) {
                    foreach (AnimatorControllerLayer layerToTest in newLayers) {
                        if (layerToTest.name == layerToAdd.name) {
                            Debug.LogWarning("Layer \"" + layerToAdd.name + "\" already exists in the new controller, adding anyway...");
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
                                throw new System.Exception("Cannot add parameter " + parameterToAdd.name + " as it already exists and is different type");
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

            return baseAnimatorController;
        }
    }
}