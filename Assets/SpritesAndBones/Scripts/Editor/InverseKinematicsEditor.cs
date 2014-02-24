/*
The MIT License (MIT)

Copyright (c) 2013 Banbury

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
using System.Collections;

[CustomEditor(typeof(InverseKinematics))]
public class InverseKinematicsEditor : Editor {

	private InverseKinematics ik; 

	void OnEnable() {
		ik = (InverseKinematics)target;
	}

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

		if (GUILayout.Button("Create Target Helper")) {
			CreateHelper();
		}

        if (((InverseKinematics)target).target == null) {
            EditorGUILayout.HelpBox("Please select a target.", MessageType.Error);
        }
    }

    [DrawGizmo(GizmoType.SelectedOrChild | GizmoType.NotSelected)]
    static void DrawIKGizmo(InverseKinematics ik, GizmoType gizmoType) {
        Handles.Label(ik.transform.position + new Vector3(0.1f, 0), "IK");
    }

	//Create a Helper for the IK Component and sets it as the IK's Target
	private void CreateHelper(){
		//Create the Helper GameObject named after the bone
		GameObject o = new GameObject (ik.name + "_IK");
		Undo.RegisterCreatedObjectUndo (o, "Create helper");
		o.AddComponent<Helper>();
        o.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

		//set the helpers position to match the bones position
        Bone b = ik.GetComponent<Bone>();
        if (b != null) {
            o.transform.position = b.Head;
        }
        else {
            o.transform.position = ik.transform.position;
        }

		//set the helper as a child of the skeleton
		o.transform.parent = ik.transform.root.GetComponentInChildren<Skeleton>().transform;

		//set the helper as the target
		ik.target = o.transform;

		//selects the transform of the Helper
		Selection.activeTransform = o.transform;
	}
}
