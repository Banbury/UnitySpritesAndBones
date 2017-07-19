/*
The MIT License (MIT)

Copyright (c) 2014 Play-Em

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
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using System;
using System.Collections;

public class RemapMeshesEditor : EditorWindow {
    [MenuItem("Sprites And Bones/Remap Meshes")]
    protected static void ShowRemapMeshesEditor() {
        var wnd = GetWindow<RemapMeshesEditor>();
        wnd.titleContent.text = "Remap Meshes";
        wnd.Show();
    }

	private GameObject o;

	private bool remapMaterials = false;

	static public void Remap(GameObject o, bool remapMat) {
		#if UNITY_EDITOR
		Debug.Log("Remapping meshes for " + o.name);


		Skin2D[] children = o.GetComponentsInChildren<Skin2D>(true);
		for(int i = 0; i < children.Length; i++) {
			if (children[i] != null) {
				children[i].AssignReferenceMesh();

				if (children[i].skinnedMeshRenderer.sharedMesh == null 
				&& children[i].referenceMesh != null 
				|| children[i].skinnedMeshRenderer.sharedMesh != null 
				&& children[i].referenceMesh != null 
				&& children[i].skinnedMeshRenderer.sharedMesh != children[i].referenceMesh) {
					children[i].skinnedMeshRenderer.sharedMesh = children[i].referenceMesh;
					Debug.Log("Remapped " + children[i].skinnedMeshRenderer.sharedMesh.name);
				}
				if (remapMat) {
					children[i].AssignReferenceMaterial();
					children[i].skinnedMeshRenderer.sharedMaterial = children[i].referenceMaterial;
				}
			}
		}
		#endif
	}

    public void OnGUI() {
		GUILayout.Label("Select a GameObject to remap Skin2D child meshes", EditorStyles.boldLabel);
		EditorGUILayout.Separator();
        GUILayout.Label("GameObject", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        
        if (Selection.activeGameObject != null) {
			o = Selection.activeGameObject;
		}
		EditorGUILayout.ObjectField(o, typeof(GameObject), true);
		EditorGUI.EndChangeCheck();

		GUILayout.BeginHorizontal ();
        if (o != null) {
			if (GUILayout.Button("Remap Meshes")) {
                #if UNITY_EDITOR
				Remap(o, remapMaterials);
				#endif
            }
			remapMaterials = GUILayout.Toggle (remapMaterials, "Remap Materials");
        }
    }

}