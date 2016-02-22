/*
The MIT License (MIT)

Copyright (c) 2014 - 2016 Banbury & Play-Em

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
using System.Linq;
using UnityToolbag;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(SortingLayerExposed))]
[ExecuteInEditMode]
public class Skin2D : MonoBehaviour {

	public Sprite sprite;

	[HideInInspector]
    public Bone2DWeights boneWeights;

    private Material lineMaterial;
    private MeshFilter meshFilter;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private GameObject lastSelected = null;

	// Prevent the bone weights from being recalculated
	public bool lockBoneWeights = false;

	// The referenced material for this skin
	public Material referenceMaterial;

    #if UNITY_EDITOR
        [MenuItem("Sprites And Bones/Skin 2D")]
        public static void Create ()
        {
			if (Selection.activeGameObject != null) {
				GameObject o = Selection.activeGameObject;
				SkinnedMeshRenderer skin = o.GetComponent<SkinnedMeshRenderer>();
				SpriteRenderer spriteRenderer = o.GetComponent<SpriteRenderer>();
				if (skin == null && spriteRenderer != null) {
					Sprite thisSprite = spriteRenderer.sprite;
					SpriteMesh spriteMesh = new SpriteMesh();
					spriteMesh.spriteRenderer = spriteRenderer;
					spriteMesh.CreateSpriteMesh();
					Texture2D spriteTexture = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(spriteRenderer.sprite, false);

					// Copy the sprite material
					Material spriteMaterial = new Material(spriteRenderer.sharedMaterial);
					spriteMaterial.CopyPropertiesFromMaterial(spriteRenderer.sharedMaterial);
					spriteMaterial.mainTexture = spriteTexture;
					string sortLayerName = spriteRenderer.sortingLayerName;
					int sortOrder = spriteRenderer.sortingOrder;
					DestroyImmediate(spriteRenderer);
					Skin2D skin2D = o.AddComponent<Skin2D>();
					skin2D.sprite = thisSprite;
					skin = o.GetComponent<SkinnedMeshRenderer>();
					MeshFilter filter = o.GetComponent<MeshFilter>();
					skin.material = spriteMaterial;

					// Save out the material from the sprite so we have a default material
					if(!Directory.Exists("Assets/Materials")) {
						AssetDatabase.CreateFolder("Assets", "Materials");
						AssetDatabase.Refresh();
					}
					AssetDatabase.CreateAsset(spriteMaterial, "Assets/Materials/" + spriteMaterial.mainTexture.name + ".mat");
					Debug.Log("Created material " + spriteMaterial.mainTexture.name + " for " + skin.gameObject.name);
					skin2D.referenceMaterial = spriteMaterial;
					skin.sortingLayerName = sortLayerName;
					skin.sortingOrder = sortOrder;

					// Create the mesh from the selection
					filter.mesh = (Mesh)Selection.activeObject;
					if (filter.sharedMesh != null && skin.sharedMesh == null) {
						skin.sharedMesh = filter.sharedMesh;
					}
					// Recalculate the bone weights for the new mesh
					skin2D.RecalculateBoneWeights();
				}
				else
				{
					o = new GameObject ("Skin2D");
					Undo.RegisterCreatedObjectUndo (o, "Create Skin2D");
					o.AddComponent<Skin2D> ();
				}
			}
			else
			{
				GameObject o = new GameObject ("Skin2D");
                Undo.RegisterCreatedObjectUndo (o, "Create Skin2D");
                o.AddComponent<Skin2D> ();
			}
        }
    #endif
    
    // Use this for initialization
    void Start() {
#if UNITY_EDITOR
        CalculateVertexColors();
#endif
		if (Application.isPlaying) {
			Mesh oldMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
			if (oldMesh != null) {
				Mesh newMesh = (Mesh)Object.Instantiate(oldMesh);
				GetComponent<SkinnedMeshRenderer>().sharedMesh = newMesh;
			}
		}
    }

    // Update is called once per frame
    void Update () {
        if (MeshFilter.sharedMesh != null && SkinnedMeshRenderer.sharedMesh == null) {
            SkinnedMeshRenderer.sharedMesh = MeshFilter.sharedMesh;
        }
		if (GetComponent<SortingLayerExposed>() == null) {
            gameObject.AddComponent<SortingLayerExposed>();
        }

		// Ensure there is a reference material for the renderer
		if (referenceMaterial == null) {
			if (SkinnedMeshRenderer.sharedMaterial != null) {
				if (SkinnedMeshRenderer.sharedMaterial.name.Contains(" (Instance)")) {
					string materialName = SkinnedMeshRenderer.sharedMaterial.name.Replace(" (Instance)", "");
					Material material = AssetDatabase.LoadAssetAtPath("Assets/Materials/" + materialName + ".mat", typeof(Material)) as Material;
					referenceMaterial = material;
				} 
				else {
					referenceMaterial = SkinnedMeshRenderer.sharedMaterial;
				}
			}
		}
		else {
			if (referenceMaterial.name.Contains(" (Instance)")) {
				string materialName = referenceMaterial.name.Replace(" (Instance)", "");
				Material material = AssetDatabase.LoadAssetAtPath("Assets/Materials/" + materialName + ".mat", typeof(Material)) as Material;
				referenceMaterial = material;
			}
		}

		// Make sure the renderer is using a material if it is nullified
		if (SkinnedMeshRenderer.sharedMaterial == null) {
			SkinnedMeshRenderer.sharedMaterial = referenceMaterial;
		}
    }

    private MeshFilter MeshFilter {
        get {
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();
            return meshFilter;
        }
    }

    private SkinnedMeshRenderer SkinnedMeshRenderer {
        get {
            if (skinnedMeshRenderer == null)
                skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            return skinnedMeshRenderer;
        }
    }

    private Material LineMaterial {
        get {
            if (lineMaterial == null) {
                lineMaterial = new Material(Shader.Find("Lines/Colored Blended"));
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            }
            return lineMaterial;
        }
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos() {

        if (Application.isEditor && MeshFilter.sharedMesh != null) {
            CalculateVertexColors();
            GL.wireframe = true;
            LineMaterial.SetPass(0);
            Graphics.DrawMeshNow(MeshFilter.sharedMesh, transform.position, transform.rotation);
            GL.wireframe = false;
        }

    }

    public void CalculateBoneWeights(Bone[] bones) {
		CalculateBoneWeights(bones, false);
    }

    public void CalculateBoneWeights(Bone[] bones, bool weightToParent) {
		if (!lockBoneWeights) {
			if(MeshFilter.sharedMesh == null)
			{
				Debug.Log("No Shared Mesh.");
				return;
			}
			Mesh mesh = new Mesh();
			mesh.name = "Generated Mesh";
			mesh.vertices = MeshFilter.sharedMesh.vertices;
			mesh.triangles = MeshFilter.sharedMesh.triangles;
			mesh.normals = MeshFilter.sharedMesh.normals;
			mesh.uv = MeshFilter.sharedMesh.uv;
			mesh.uv2 = MeshFilter.sharedMesh.uv2;
			mesh.bounds = MeshFilter.sharedMesh.bounds;

			if (bones != null && mesh != null) {
				boneWeights = new Bone2DWeights();
				boneWeights.weights = new Bone2DWeight[] { };
				
				int index = 0;
				foreach (Bone bone in bones) {
					int i=0;

					bool boneActive = bone.gameObject.activeSelf;
					bone.gameObject.SetActive(true);
					foreach (Vector3 v in mesh.vertices) {
						float influence;
						if (!weightToParent || weightToParent && bone.transform != transform.parent)
						{
							influence = bone.GetInfluence(v + transform.position);
						}
						else
						{
							influence = 1.0f;
						}

						boneWeights.SetWeight(i, bone.name, index, influence);
					
						i++;
					}
					
					index++;
					bone.gameObject.SetActive(boneActive);
				}

				BoneWeight[] unitweights = boneWeights.GetUnityBoneWeights();
				mesh.boneWeights = unitweights;

				Transform[] bonesArr = bones.Select(b => b.transform).ToArray();
				Matrix4x4[] bindPoses = new Matrix4x4[bonesArr.Length];

				for (int i = 0; i < bonesArr.Length; i++) {
					bindPoses[i] = bonesArr[i].worldToLocalMatrix * transform.localToWorldMatrix;
				}

				mesh.bindposes = bindPoses;

				var skinRenderer = GetComponent<SkinnedMeshRenderer>();
				if (skinRenderer.sharedMesh != null && !AssetDatabase.Contains(skinRenderer.sharedMesh.GetInstanceID()))
					Object.DestroyImmediate(skinRenderer.sharedMesh);
				skinRenderer.bones = bonesArr;
				skinRenderer.sharedMesh = mesh;
				EditorUtility.SetDirty(skinRenderer.gameObject);
				if (PrefabUtility.GetPrefabType(skinRenderer.gameObject) != PrefabType.None) {
					AssetDatabase.SaveAssets();
				}
			}
		}
    }

    private void CalculateVertexColors() {
        GameObject go = Selection.activeGameObject;

        if (go == lastSelected || MeshFilter.sharedMesh == null) {
            return;
        }

        lastSelected = go;

        Mesh m = SkinnedMeshRenderer.sharedMesh;

        Color[] colors = new Color[m.vertexCount];

        for (int i = 0; i < colors.Length; i++) {
            colors[i] = Color.black;
        }

        if (go != null) {
            Bone bone = go.GetComponent<Bone>();

            if (bone != null) {
                if (SkinnedMeshRenderer.bones.Any(b => b.gameObject.GetInstanceID() == bone.gameObject.GetInstanceID())) {
                    for (int i = 0; i < colors.Length; i++) {
                        float value = 0;

                        BoneWeight bw = m.boneWeights[i];
                        if (bw.boneIndex0 == bone.index)
                            value = bw.weight0;
                        else if (bw.boneIndex1 == bone.index)
                            value = bw.weight1;
                        else if (bw.boneIndex2 == bone.index)
                            value = bw.weight2;
                        else if (bw.boneIndex3 == bone.index)
                            value = bw.weight3;

                        colors[i] = Util.HSBColor.ToColor(new Util.HSBColor(0.7f - value, 1.0f, 0.5f));
                    }
                }
            }
        }

        MeshFilter.sharedMesh.colors = colors;
    }

    public void SaveAsPrefab() {

		// Check if the Prefabs directory exists, if not, create it.
        DirectoryInfo prefabDir = new DirectoryInfo("Assets/Prefabs");
		if (Directory.Exists(prefabDir.FullName) == false)
        {
            Directory.CreateDirectory(prefabDir.FullName);
        }

		Skeleton[] skeletons = transform.root.gameObject.GetComponentsInChildren<Skeleton>(true);
		Skeleton skeleton = null;
		foreach (Skeleton s in skeletons)
		{
			if (transform.IsChildOf(s.transform))
			{
				skeleton = s;
			}
		}

        DirectoryInfo prefabSkelDir = new DirectoryInfo("Assets/Prefabs/" + skeleton.gameObject.name);
		if (Directory.Exists(prefabSkelDir.FullName) == false)
        {
            Directory.CreateDirectory(prefabSkelDir.FullName);
        }

        string path = "Assets/Prefabs/" + skeleton.gameObject.name + "/" + gameObject.name + ".prefab";

		// Need to create a new Mesh because replacing the prefab will erase the sharedMesh 
		// on the SkinnedMeshRenderer if it is linked to the prefab
        Mesh mesh = new Mesh();
		mesh.name = "Generated Mesh";
		mesh.vertices = SkinnedMeshRenderer.sharedMesh.vertices;
		mesh.triangles = SkinnedMeshRenderer.sharedMesh.triangles;
		mesh.normals = SkinnedMeshRenderer.sharedMesh.normals;
		mesh.uv = SkinnedMeshRenderer.sharedMesh.uv;
		mesh.uv2 = SkinnedMeshRenderer.sharedMesh.uv2;
		mesh.bounds = SkinnedMeshRenderer.sharedMesh.bounds;

		// Ensure there is a reference material for the renderer
		if (referenceMaterial == null) {
			if (SkinnedMeshRenderer.sharedMaterial.name.Contains(" (Instance)")) {
				string materialName = SkinnedMeshRenderer.sharedMaterial.name.Replace(" (Instance)", "");
				Debug.Log(materialName);
				Material material = AssetDatabase.LoadAssetAtPath("Assets/Materials/" + materialName + ".mat", typeof(Material)) as Material;
				referenceMaterial = material;
			} 
			else {
				referenceMaterial = SkinnedMeshRenderer.sharedMaterial;
			}
		}
		else {
			if (referenceMaterial.name.Contains(" (Instance)")) {
				string materialName = referenceMaterial.name.Replace(" (Instance)", "");
				Material material = AssetDatabase.LoadAssetAtPath("Assets/Materials/" + materialName + ".mat", typeof(Material)) as Material;
				referenceMaterial = material;
			}
		}

		// Create a new prefab erasing the old one
		Object obj = PrefabUtility.CreateEmptyPrefab(path);

		// Reassign the Mesh to the SkinnedMeshRenderer
		SkinnedMeshRenderer.sharedMesh = mesh;

		// Make sure the renderer is using a material
		if (referenceMaterial != null) {
			SkinnedMeshRenderer.sharedMaterial = referenceMaterial;
		}

		// Add the mesh back to the prefab
        AssetDatabase.AddObjectToAsset(SkinnedMeshRenderer.sharedMesh, obj);

		// Replace the prefab
        PrefabUtility.ReplacePrefab(gameObject, obj, ReplacePrefabOptions.ConnectToPrefab);
    }

	public void RecalculateBoneWeights() {
		Skeleton[] skeletons = transform.root.gameObject.GetComponentsInChildren<Skeleton>(true);
		Skeleton skeleton = null;
		foreach (Skeleton s in skeletons)
		{
			if (transform.IsChildOf(s.transform))
			{
				skeleton = s;
			}
		}
		if (skeleton != null)
		{
			skeleton.CalculateWeights(true);
			// Debug.Log("Calculated weights for " + gameObject.name);
		}
    }

	public void ResetControlPointPositions() {
		ControlPoint[] controlPoints = GetComponentsInChildren<ControlPoint>();
		if (controlPoints != null)
		{
			foreach (ControlPoint controlPoint in controlPoints)
			{
				controlPoint.ResetPosition();
				// Debug.Log("Reset Control Point Positions for " + gameObject.name);
			}
		}
    }
#endif
}
