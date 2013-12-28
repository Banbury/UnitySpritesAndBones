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

[CustomEditor(typeof(Bone))]
public class BoneEditor : Editor {
    private Bone bone;

    void OnEnable() {
        bone = (Bone)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Add child")) {
            Bone.Create();
        }
        if (GUILayout.Button("Split")) {
            Bone.Split();
        }
        if (GUILayout.Button("Add IK")) {
            bone.AddIK();
        }

        GUILayout.EndHorizontal();
    }

    void OnSceneGUI() {
        Handles.color = Color.green;

        if (bone.editMode) {
            EditorGUI.BeginChangeCheck();
            Vector3 v = Handles.FreeMoveHandle(bone.Head, Quaternion.identity, 0.1f, Vector3.zero, Handles.RectangleCap);
            Undo.RecordObject(bone, "Change bone");
            bone.length = Vector2.Distance(v, bone.transform.position);
            bone.transform.up = (v - bone.transform.position).normalized;
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(bone);
            }
        }
        else {
            var ik = bone.GetComponent<InverseKinematics>();
            if (bone.transform.parent != null && (ik == null || !ik.enabled || ik.influence == 0) && bone.snapToParent) {
                Transform parent = bone.transform.parent;
                float length = Vector2.Distance(parent.position, bone.transform.position);

                Bone parentBone = parent.GetComponent<Bone>();

                if (parentBone != null && Mathf.Abs(parentBone.length - length) > 0.0001) {
                    bone.transform.parent = null;
                    parent.up = (bone.transform.position - parent.position).normalized;
                    parentBone.length = length;
                    bone.transform.parent = parent;
                }
            }
        }
    }
}
