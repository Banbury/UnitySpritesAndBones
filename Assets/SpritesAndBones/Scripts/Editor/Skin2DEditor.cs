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
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System.Collections;

[CustomEditor(typeof(Skin2D))]
public class Skin2DEditor : Editor {
    public override void OnInspectorGUI() {
        Skin2D skin = (Skin2D)target;

        DrawDefaultInspector();

        EditorGUILayout.Separator();

        if (skin.GetComponent<SkinnedMeshRenderer>().sharedMesh != null && GUILayout.Button("Save as Prefab")) {
            skin.SaveAsPrefab();
        }

		EditorGUILayout.Separator();

        if (skin.GetComponent<SkinnedMeshRenderer>().sharedMesh != null && GUILayout.Button("Recalculate Bone Weights")) {
            skin.RecalculateBoneWeights();
        }

		EditorGUILayout.Separator();

        if (skin.GetComponent<SkinnedMeshRenderer>().sharedMesh != null && GUILayout.Button("Create Control Points")) {
            ControlPoint.CreateControlPoints(skin.GetComponent<SkinnedMeshRenderer>());
        }

        if (skin.GetComponent<SkinnedMeshRenderer>().sharedMesh != null && GUILayout.Button("Reset Control Points")) {
            skin.ResetControlPointPositions();
        }

		EditorGUILayout.Separator();

        if (skin.GetComponent<SkinnedMeshRenderer>().sharedMesh != null && GUILayout.Button("Generate Mesh Asset")) {
            #if UNITY_EDITOR
			// Check if the Meshes directory exists, if not, create it.
			if(!Directory.Exists("Assets/Meshes")) {
				AssetDatabase.CreateFolder("Assets", "Meshes");
				AssetDatabase.Refresh();
			}
			ScriptableObjectUtility.CreateAsset(skin.GetComponent<SkinnedMeshRenderer>().sharedMesh, "Meshes/" + skin.gameObject.name + ".Mesh");
			#endif
        }

        if (skin.GetComponent<SkinnedMeshRenderer>().sharedMaterial != null && GUILayout.Button("Generate Material Asset")) {
            #if UNITY_EDITOR
			Material material = new Material(skin.GetComponent<SkinnedMeshRenderer>().sharedMaterial);
			material.CopyPropertiesFromMaterial(skin.GetComponent<SkinnedMeshRenderer>().sharedMaterial);
			skin.GetComponent<SkinnedMeshRenderer>().sharedMaterial = material;
			if(!Directory.Exists("Assets/Materials")) {
				AssetDatabase.CreateFolder("Assets", "Materials");
				AssetDatabase.Refresh();
			}
			AssetDatabase.CreateAsset(material, "Assets/Materials/" + material.mainTexture.name + ".mat");
			Debug.Log("Created material " + material.mainTexture.name + " for " + skin.gameObject.name);
			#endif
        }
    }
}
