using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;

[CustomEditor(typeof(Skeleton))]
public class SkeletonEditor : Editor {
    private Skeleton skeleton;
	private string poseFileName = "New Pose";
    void OnEnable() {
        skeleton = (Skeleton)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Poses", EditorStyles.boldLabel);

		poseFileName = EditorGUILayout.TextField("Pose Filename",poseFileName);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save pose")) {
			skeleton.SavePose(poseFileName);
        }

        if (GUILayout.Button("Reset pose")) {
            if (skeleton.basePose != null) {
                skeleton.RestorePose(skeleton.basePose);
            }
        }

        GUILayout.EndHorizontal();
        if (skeleton.basePose == null) {
            EditorGUILayout.HelpBox("You have not selected a base pose.", MessageType.Error);
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
                    skeleton.SetEditMode(!skeleton.editMode);
                }
                break;
        }
    }
}
