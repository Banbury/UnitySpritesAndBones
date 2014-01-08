using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Skin2D))]
public class Skin2DEditor : Editor {
    public override void OnInspectorGUI() {
        Skin2D skin = (Skin2D)target;

        DrawDefaultInspector();

        EditorGUILayout.Separator();

        if (skin.skeleton != null && skin.GetComponent<MeshFilter>().sharedMesh != null && GUILayout.Button("Calculate weights")) {
            skin.CalculateBoneWeights();
        }

        if (skin.GetComponent<SkinnedMeshRenderer>().sharedMesh != null && GUILayout.Button("Save")) {
            skin.SaveAsPrefab();
        }
    }
}
