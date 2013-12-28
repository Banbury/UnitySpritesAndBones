using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Pose))]
public class PoseEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        GUILayout.Label("Rotations: " + ((Pose)target).rotations.Length);
    }

    public override bool HasPreviewGUI() {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background) {
        //Debug.Log(((Pose)target).rotations.Count);
    }
}
