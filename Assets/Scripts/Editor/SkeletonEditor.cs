using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;

[CustomEditor(typeof(Skeleton))]
public class SkeletonEditor : Editor {
    private Skeleton skeleton;

    void OnEnable() {
        skeleton = (Skeleton)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Save pose")) {
            skeleton.SavePose();
        }
    }

    void OnSceneGUI() {
        switch (Event.current.type) {
            case EventType.DragUpdated:
                if (Array.Find(DragAndDrop.objectReferences, o => o is Pose) != null)
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                break;
            case EventType.DragPerform:
                Pose pose = (Pose)Array.Find(DragAndDrop.objectReferences, o => o is Pose);
                if (pose != null) {
                    skeleton.RestorePose(pose);
                }
                break;
            case EventType.KeyUp:
                if (Event.current.keyCode == KeyCode.Tab) {
                    skeleton.editMode = !skeleton.editMode;
                }
                break;
        }
    }
}
