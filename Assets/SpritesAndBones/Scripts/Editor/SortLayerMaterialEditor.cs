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
using System.IO;
#endif
using System.Collections;
using System.Collections.Generic;

public class SortLayerMaterialEditor : EditorWindow {
    private Renderer renderer;
	private GameObject o;
	private GameObject copyFrom;
	private GameObject copyTo;
	private GameObject copyPosFrom;
	private GameObject copyPosTo;
	private int addToLayer = 0;

	private bool includeChildrenForSorting = false;
	private bool includeChildrenForMaterials = false;

    [MenuItem("Sprites And Bones/Sorting Layers And Materials")]
    protected static void ShowSortLayerMaterialEditor() {
        var wnd = GetWindow<SortLayerMaterialEditor>();
        wnd.titleContent.text = "Sort Layers and Create Materials";
        wnd.Show();
    }

	static void CreateMaterial (GameObject go) {
		// Create a simple material asset
		if (go.GetComponent<Renderer>() != null)
		{
			Material material = new Material(go.GetComponent<Renderer>().sharedMaterial);
			material.CopyPropertiesFromMaterial(go.GetComponent<Renderer>().sharedMaterial);
			go.GetComponent<Renderer>().sharedMaterial = material;
			MaterialPropertyBlock block = new MaterialPropertyBlock();
			go.GetComponent<Renderer>().GetPropertyBlock(block);
			#if UNITY_EDITOR
			if(!Directory.Exists("Assets/Materials")) {
				AssetDatabase.CreateFolder("Assets", "Materials");
				AssetDatabase.Refresh();
			}

			string textureName = null;
			if (block.GetTexture(0).name != null) {
				textureName = block.GetTexture(0).name;
			} else {
				textureName = material.mainTexture.name;
			}
			AssetDatabase.CreateAsset(material, "Assets/Materials/" + textureName + ".mat");
			Debug.Log("Created material " + textureName + " for " + go.name);
			#endif
		}
	}

	public static void Reset ()
	{
        #if UNITY_EDITOR
		if (Selection.activeGameObject != null) {
			GameObject o = Selection.activeGameObject;
			if (o.GetComponent<Renderer>() != null)
			{
				o.GetComponent<Renderer>().sortingLayerName = "Default";
				o.GetComponent<Renderer>().sortingOrder = 0;
			}
			Transform[] children = o.GetComponentsInChildren<Transform>(true);
			foreach(Transform child in children) {
				if (child.gameObject.GetComponent<Renderer>() != null)
				{
					child.gameObject.GetComponent<Renderer>().sortingLayerName = "Default";
					child.gameObject.GetComponent<Renderer>().sortingOrder = 0;
				}
			}
		}
		#endif
    }

    public void OnGUI() {
        GUILayout.Label("GameObject", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        
        if (Selection.activeGameObject != null) {
			o = Selection.activeGameObject;
		}
		EditorGUILayout.ObjectField(o, typeof(GameObject), true);
		EditorGUI.EndChangeCheck();

        if (o != null) {
			if (GUILayout.Button("Reset Sorting Layers")) {
                #if UNITY_EDITOR
				Reset();
				#endif
            }
            EditorGUILayout.Separator();

            GUILayout.Label("Add to Sorting Layers ", EditorStyles.boldLabel);

            addToLayer = EditorGUILayout.IntField("Add:", addToLayer);

			includeChildrenForSorting = EditorGUILayout.Toggle("Include Children", includeChildrenForSorting);

            if (GUILayout.Button("Add to Sorting Layers")) {
                #if UNITY_EDITOR
				if (Selection.activeGameObject != null) {
					o = Selection.activeGameObject;
					if (o.GetComponent<Renderer>() != null)
					{
						o.GetComponent<Renderer>().sortingOrder = o.GetComponent<Renderer>().sortingOrder+addToLayer;
					}
					if (includeChildrenForSorting)
					{
						Transform[] children = o.GetComponentsInChildren<Transform>(true);
						foreach(Transform child in children) {
							if (child.gameObject.GetComponent<Renderer>() != null)
							{
								child.gameObject.GetComponent<Renderer>().sortingOrder = child.gameObject.GetComponent<Renderer>().sortingOrder+addToLayer;
							}
						}
					}
				}
				#endif
            }

            GUILayout.Label("Create Materials from Renderer", EditorStyles.boldLabel);

            EditorGUILayout.Separator();
			includeChildrenForMaterials = EditorGUILayout.Toggle("Include Children", includeChildrenForMaterials);

            if (GUILayout.Button("Create Material")) {
				#if UNITY_EDITOR
				if (Selection.activeGameObject != null) {
					o = Selection.activeGameObject;
					if (o.GetComponent<Renderer>() != null)
					{
						CreateMaterial(o);
					}
					if (includeChildrenForMaterials)
					{
						Transform[] children = o.GetComponentsInChildren<Transform>(true);
						foreach(Transform child in children) {
							if (child.gameObject.GetComponent<Renderer>() != null)
							{
								CreateMaterial(child.gameObject);
							}
						}
					}
				}
				#endif
            }

			EditorGUILayout.Separator();
			GUILayout.Label("Copy Sorting Layers Between Objects", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			copyFrom = (GameObject)EditorGUILayout.ObjectField("Copy From: ", copyFrom, typeof(GameObject), true);
			copyTo = (GameObject)EditorGUILayout.ObjectField("Copy To: ", copyTo, typeof(GameObject), true);
			EditorGUI.EndChangeCheck();
			if (GUILayout.Button("Copy Sorting Layers")) {
                #if UNITY_EDITOR
				if (copyFrom != null && copyTo != null) {
					Transform[] copyFromChildren = copyFrom.GetComponentsInChildren<Transform>(true);
					Transform[] copyToChildren = copyTo.GetComponentsInChildren<Transform>(true);
					foreach(Transform copyFromChild in copyFromChildren) {
						if (copyFromChild.gameObject.GetComponent<Renderer>() != null)
						{
							foreach(Transform copyToChild in copyToChildren) {
								if (copyToChild.gameObject.GetComponent<Renderer>() != null && copyToChild.name == copyFromChild.name)
								{
									copyToChild.gameObject.GetComponent<Renderer>().sortingOrder = copyFromChild.gameObject.GetComponent<Renderer>().sortingOrder;
								}
							}
						}
					}
				}
				else
				{
					EditorGUILayout.HelpBox("No Gameobjects selected.", MessageType.Error);
				}
				#endif
            }

			EditorGUILayout.Separator();
			GUILayout.Label("Copy Positions and Rotations Between Objects", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			copyPosFrom = (GameObject)EditorGUILayout.ObjectField("Copy From: ", copyPosFrom, typeof(GameObject), true);
			copyPosTo = (GameObject)EditorGUILayout.ObjectField("Copy To: ", copyPosTo, typeof(GameObject), true);
			EditorGUI.EndChangeCheck();
			if (GUILayout.Button("Copy Positions and Rotations")) {
                #if UNITY_EDITOR
				if (copyPosFrom != null && copyPosTo != null) {
					Transform[] copyFromChildren = copyPosFrom.GetComponentsInChildren<Transform>(true);
					Transform[] copyToChildren = copyPosTo.GetComponentsInChildren<Transform>(true);
					foreach(Transform copyFromChild in copyFromChildren) {
						if (copyFromChild.gameObject.GetComponent<Renderer>() != null)
						{
							foreach(Transform copyToChild in copyToChildren) {
								if (copyToChild.gameObject.GetComponent<Renderer>() != null && copyToChild.name == copyFromChild.name)
								{
									copyToChild.position = new Vector3(copyFromChild.position.x, copyFromChild.position.y, copyFromChild.position.z);
									
									copyToChild.rotation = new Quaternion(copyFromChild.rotation.x, copyFromChild.rotation.y, copyFromChild.rotation.z, copyFromChild.rotation.w);
								}
							}
						}
					}
				}
				else
				{
					EditorGUILayout.HelpBox("No Gameobjects selected.", MessageType.Error);
				}
				#endif
            }
        }
    }

}
