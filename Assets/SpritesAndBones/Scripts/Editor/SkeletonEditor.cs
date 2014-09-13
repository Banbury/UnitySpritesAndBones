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

		if (GUILayout.Button("Use Shadows")) {
			skeleton.useShadows = !skeleton.useShadows;
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
		if(GUILayout.Button("Calculate weights")) {
			skeleton.CalculateWeights();
		}
        if (skeleton.basePose == null) {
            EditorGUILayout.HelpBox("You have not selected a base pose.", MessageType.Error);
        }
		if(GUILayout.Button("Save Children Positions")) {
			Bone[] bones = skeleton.gameObject.GetComponentsInChildren<Bone>();
			foreach (Bone bone in bones) {
				bone.SaveChildPosRot();
			}
		}
		if(GUILayout.Button("Load Children Positions")) {
			Bone[] bones = skeleton.gameObject.GetComponentsInChildren<Bone>();
			foreach (Bone bone in bones) {
				bone.LoadChildPosRot();
			}
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
