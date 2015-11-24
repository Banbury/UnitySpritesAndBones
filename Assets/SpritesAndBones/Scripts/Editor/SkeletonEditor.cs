/*
The MIT License (MIT)

Copyright (c) 2014 Banbury & Play-Em

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;

[CustomEditor(typeof(Skeleton))]
public class SkeletonEditor : Editor {
    private Skeleton skeleton;
	private string poseFileName = "New Pose";
	private string shadows;
	private string zsorting;

    void OnEnable() {
        skeleton = (Skeleton)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EditorGUILayout.Separator();

		if (GUILayout.Button("FlipY")) {
			skeleton.flipY = !skeleton.flipY;
        }

		if (GUILayout.Button("FlipX")) {
			skeleton.flipX = !skeleton.flipX;
        }

		EditorGUILayout.Separator();

		if (GUILayout.Button("Use Shadows")) {
			skeleton.useShadows = !skeleton.useShadows;
        }
		if (skeleton.useShadows)
		{
			shadows = "On";
		}
		else
		{
			shadows = "Off";
		}
		EditorGUILayout.LabelField("Shadows:", shadows, EditorStyles.whiteLabel);

		EditorGUILayout.Separator();

		if (GUILayout.Button("Use Z Sorting")) {
			skeleton.useZSorting = !skeleton.useZSorting;
        }
		if (skeleton.useZSorting)
		{
			zsorting = "On";
		}
		else
		{
			zsorting = "Off";
		}
		EditorGUILayout.LabelField("Z Sorting:", zsorting, EditorStyles.whiteLabel);

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
		if(GUILayout.Button("Calculate weights")) {
			skeleton.CalculateWeights();
		}
        if (skeleton.basePose == null) {
            EditorGUILayout.HelpBox("You have not selected a base pose.", MessageType.Error);
        }

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Save/Load Children Positions", EditorStyles.boldLabel);

        if (!skeleton.editMode) {
            EditorGUILayout.HelpBox("Skeleton Needs to be in Edit Mode.", MessageType.Error);
        }

		if(GUILayout.Button("Save Children Positions") && skeleton.editMode) {
			Bone[] bones = skeleton.gameObject.GetComponentsInChildren<Bone>(true);
			foreach (Bone bone in bones) {
				bone.SaveChildPosRot();
				if (bone.HasChildPositionsSaved()){
					skeleton.hasChildPositionsSaved = true;
				}
			}
			Debug.Log("Saved Children Positions and Rotations in Skeleton.");
		}

        if (!skeleton.hasChildPositionsSaved) {
            EditorGUILayout.HelpBox("You have not saved children positions.", MessageType.Error);
        }

		if(GUILayout.Button("Load Children Positions") && skeleton.hasChildPositionsSaved && skeleton.editMode) {
			Bone[] bones = skeleton.gameObject.GetComponentsInChildren<Bone>(true);
			foreach (Bone bone in bones) {
				bone.LoadChildPosRot();
			}
			Debug.Log("Loaded Children Positions and Rotations in Skeleton.");
		}

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Create Skin2D Prefabs from Children", EditorStyles.boldLabel);

		if(GUILayout.Button("Create Skin2D Prefabs")) {
			Skin2D[] skins = skeleton.gameObject.GetComponentsInChildren<Skin2D>(true);
			foreach (Skin2D skin in skins) {
				bool skinActive = skin.gameObject.activeSelf;
				skin.gameObject.SetActive(true);
				skin.SaveAsPrefab();
				skin.gameObject.SetActive(skinActive);
			}
			Debug.Log("Saved all Skins as Prefabs.");
		}

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Disconnect All Skin2D Prefabs", EditorStyles.boldLabel);

		if(GUILayout.Button("Disconnect Skin2D Prefabs")) {
			Skin2D[] skins = skeleton.gameObject.GetComponentsInChildren<Skin2D>(true);
			foreach (Skin2D skin in skins) {
				bool skinActive = skin.gameObject.activeSelf;
				skin.gameObject.SetActive(true);
				PrefabUtility.DisconnectPrefabInstance(skin.gameObject);
				skin.gameObject.SetActive(skinActive);
			}
			Debug.Log("Disconnected Skin2D Prefabs.");
		}

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Lock All Skin Bone Weights", EditorStyles.boldLabel);

		if(GUILayout.Button("Lock Skin2D Bone Weights")) {
			Skin2D[] skins = skeleton.gameObject.GetComponentsInChildren<Skin2D>(true);
			foreach (Skin2D skin in skins) {
				bool skinActive = skin.gameObject.activeSelf;
				skin.gameObject.SetActive(true);
				skin.lockBoneWeights = true;
				skin.gameObject.SetActive(skinActive);
			}
			Debug.Log("Locked Skin2D Bone Weights.");
		}

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Unlock All Skin Bone Weights", EditorStyles.boldLabel);

		if(GUILayout.Button("Unlock Skin2D Bone Weights")) {
			Skin2D[] skins = skeleton.gameObject.GetComponentsInChildren<Skin2D>(true);
			foreach (Skin2D skin in skins) {
				bool skinActive = skin.gameObject.activeSelf;
				skin.gameObject.SetActive(true);
				skin.lockBoneWeights = false;
				skin.gameObject.SetActive(skinActive);
			}
			Debug.Log("Unlocked Skin2D Bone Weights.");
		}

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Reset Skins' Control Point Names", EditorStyles.boldLabel);

		if(GUILayout.Button("Reset Control Point Names")) {
			ControlPoint[] cps = skeleton.gameObject.GetComponentsInChildren<ControlPoint>(true);
			foreach (ControlPoint cp in cps) {
				cp.Rename();
			}
			Debug.Log("Reset all control point names.");
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
				coll.offset = new Vector2(0, bone.length / 2);
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
