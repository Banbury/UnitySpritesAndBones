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

        skeleton.useShadows = EditorGUILayout.Toggle("Use Shadows", skeleton.useShadows);

        if (GUILayout.Button("Flip")) {
			skeleton.flip = !skeleton.flip;
        }

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

		if(skeleton.GetComponentInChildren<Skin2D>() != null && GUILayout.Button("Calculate weights")) {
			skeleton.CalculateWeights();
		}
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

    [MenuItem("Sprites And Bones/Create Ragdoll")]
    protected static void ShowSkinMeshEditor() {
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Skeleton>() != null) {
            Bone[] bones = Selection.activeGameObject.GetComponentsInChildren<Bone>();

            foreach (Bone bone in bones) {
                BoxCollider2D coll = bone.gameObject.AddComponent<BoxCollider2D>();
                coll.size = new Vector2(bone.length / 2, bone.length);
                coll.center = new Vector2(0, bone.length / 2);

                bone.gameObject.AddComponent<Rigidbody2D>();

                if (bone.transform.parent != null && bone.transform.parent.GetComponent<Bone>() != null) {
                    Bone parentBone = bone.transform.parent.GetComponent<Bone>();
                    HingeJoint2D hinge = bone.gameObject.AddComponent<HingeJoint2D>();
                    hinge.connectedBody = parentBone.GetComponent<Rigidbody2D>();
                    hinge.connectedAnchor = bone.transform.localPosition;
                }
            }
        }
        else {
            Debug.LogError("No Skeleton selected.");
        }
    }
}
