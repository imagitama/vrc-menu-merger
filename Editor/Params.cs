using System.Collections.Generic;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace PeanutTools_VRC_Menu_Merger {
    class Params {
        public static void SaveParams(VRCExpressionParameters myParams, string pathInsideProject) {
            AssetDatabase.CreateAsset(myParams, pathInsideProject);
        }
        
        public static VRCExpressionParameters MergeParams(UnityEngine.Object[] sources) {
            VRCExpressionParameters baseParams = ScriptableObject.CreateInstance(typeof(VRCExpressionParameters)) as VRCExpressionParameters;

            baseParams.parameters = new VRCExpressionParameters.Parameter[0];

            List<VRCExpressionParameters.Parameter> newParameters = new List<VRCExpressionParameters.Parameter>();

            foreach (UnityEngine.Object source in sources) {
                VRCExpressionParameters paramsToMerge = (VRCExpressionParameters)source;

                foreach (VRCExpressionParameters.Parameter parameterToAdd in paramsToMerge.parameters)
                {
                    bool alreadyExists = false;

                    foreach (VRCExpressionParameters.Parameter parameterToTest in newParameters) {
                        if (parameterToTest.name == parameterToAdd.name) {
                            alreadyExists = true;

                            if (parameterToTest.valueType != parameterToAdd.valueType) {
                                throw new System.Exception("Cannot add parameter " + parameterToAdd.name + " as it already exists and is different type");
                            }
                        }
                    }

                    if (alreadyExists == false) {
                        newParameters.Add(parameterToAdd);
                    }
                }
            }
            
            baseParams.parameters = newParameters.ToArray();

            return baseParams;
        }
    }
}