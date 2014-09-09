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

[InitializeOnLoad]
[CustomEditor(typeof(InverseKinematics))]
public class InverseKinematicsEditor : Editor {

	private InverseKinematics ik; 

	static InverseKinematicsEditor ()
	{
		SceneView.onSceneGUIDelegate += OnScene;
	}

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

	//Angle Limit code adapted from Veli-Pekka Kokkonen's SimpleCCDEditor http://goo.gl/6oSzDx

	// Scales scene view gizmo, feel free to change ;)
	const float gizmoSize = 0.5f;

	static void OnScene(SceneView sceneview)
	{
		var targets = GameObject.FindObjectsOfType<InverseKinematics>();

		foreach (var target in targets)
		{
			foreach (var node in target.angleLimits)
			{
				if (node.Transform == null)
					continue;

				Transform transform = node.Transform;
				Vector3 position = transform.position;

				float handleSize = HandleUtility.GetHandleSize(position);
				float discSize = handleSize * gizmoSize;


				Bone b = transform.GetComponent<Bone>();
				Bone pb = transform.parent.GetComponent<Bone>();
				float parentRotation = pb ? Vector2.Angle(pb.Head, b.Head) : 0;
				Vector3 min = Quaternion.Euler(0, 0, node.min + parentRotation)*Vector3.down;
				Vector3 max = Quaternion.Euler(0, 0, node.max + parentRotation)*Vector3.down;

				Handles.color = new Color(0, 1, 0, 0.1f);
				Handles.DrawWireDisc(position, Vector3.back, discSize);
				Handles.DrawSolidArc(position, Vector3.forward, min, node.max - node.min, discSize);

				Handles.color = Color.green;
				Handles.DrawLine(position, position + min * discSize);
				Handles.DrawLine(position, position + max * discSize);

				Vector3 toChild = target.RootBone.position - position;
				Handles.DrawLine(position, position + toChild);
			}
		}
	}
}
