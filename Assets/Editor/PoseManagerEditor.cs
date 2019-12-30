using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor(typeof (PoseManager))]
[CanEditMultipleObjects]
public class PoseManagerEditor : Editor {

    private static string poseName;

    public override void OnInspectorGUI()
    {
        PoseManager pm = (PoseManager)target;

        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pose Editor");
        poseName = EditorGUILayout.TextField("Pose Name", poseName);

        if (GUILayout.Button("Save"))
        {
            if (!String.IsNullOrEmpty(poseName) || true)
            {
                Debug.Log("Saving Pose: " + poseName);
                Pose asset = ScriptableObject.CreateInstance<Pose>();
                asset.writePose(pm);
                AssetDatabase.CreateAsset(asset, "Assets/Poses/" + poseName + ".asset");
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.Log("Invalid Pose: " + poseName);
            }
        }

        if (GUILayout.Button("Force Pose"))
        {
            pm.SetPose(pm.currentPose);
        }

        if (GUILayout.Button("Mirror Pose"))
        {
            pm.Mirror();
        }

    }
}
