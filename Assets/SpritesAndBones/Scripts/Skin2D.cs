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
    private MeshFilter _meshFilter;
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private GameObject lastSelected = null;

	// Prevent the bone weights from being recalculated
	public bool lockBoneWeights = false;

	// The referenced material for this skin
	public Material referenceMaterial;

	public ControlPoints.Point[] controlPoints;

	public ControlPoints points;

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
		if (GetComponent<SortingLayerExposed>() == null) {
            gameObject.AddComponent<SortingLayerExposed>();
        }
		if (!Application.isPlaying) {
			CalculateVertexColors();
		}
#endif
		if (Application.isPlaying) {
			Mesh oldMesh = skinnedMeshRenderer.sharedMesh;
			if (oldMesh != null) {
				Mesh newMesh = (Mesh)Object.Instantiate(oldMesh);
				skinnedMeshRenderer.sharedMesh = newMesh;
			}
		}
    }

    // Update is called once per frame
    void Update () {
		#if UNITY_EDITOR
		// Ensure there is a reference material for the renderer
		if (referenceMaterial == null) {
			if (skinnedMeshRenderer.sharedMaterial != null) {
				if (skinnedMeshRenderer.sharedMaterial.name.Contains(" (Instance)")) {
					string materialName = skinnedMeshRenderer.sharedMaterial.name.Replace(" (Instance)", "");
					Material material = AssetDatabase.LoadAssetAtPath("Assets/Materials/" + materialName + ".mat", typeof(Material)) as Material;
					referenceMaterial = material;
				} 
				else {
					referenceMaterial = skinnedMeshRenderer.sharedMaterial;
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
		if (skinnedMeshRenderer.sharedMaterial == null) {
			skinnedMeshRenderer.sharedMaterial = referenceMaterial;
		}
		#endif
    }

	// Update is called once per frame
	void LateUpdate () {
		if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null 
		&& controlPoints != null && controlPoints.Length > 0)
		{
			for (int i = 0; i < controlPoints.Length; i++) {
				if (points != null) {
					if (points.GetPoint(controlPoints[i]) != skinnedMeshRenderer.sharedMesh.vertices[i])
					{
						Vector3[] vertices = new Vector3[skinnedMeshRenderer.sharedMesh.vertices.Length];
						System.Array.Copy(skinnedMeshRenderer.sharedMesh.vertices, vertices, skinnedMeshRenderer.sharedMesh.vertices.Length);
						vertices[i] = points.GetPoint(controlPoints[i]);
						skinnedMeshRenderer.sharedMesh.vertices = vertices;
						skinnedMeshRenderer.sharedMesh.RecalculateBounds();
					}
				}
			}
		}
	}

    private MeshFilter meshFilter {
        get {
            if (_meshFilter == null)
                _meshFilter = GetComponent<MeshFilter>();
            return _meshFilter;
        }
    }

    private SkinnedMeshRenderer skinnedMeshRenderer {
        get {
            if (_skinnedMeshRenderer == null)
                _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            return _skinnedMeshRenderer;
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

        if (Application.isEditor && meshFilter.sharedMesh != null) {
            CalculateVertexColors();
            GL.wireframe = true;
            LineMaterial.SetPass(0);
            Graphics.DrawMeshNow(meshFilter.sharedMesh, transform.position, transform.rotation);
            GL.wireframe = false;
        }

    }

    void CreateOrReplaceAsset (Mesh mesh, string path)
    {
        var meshAsset = AssetDatabase.LoadAssetAtPath (path, typeof(Mesh)) as Mesh;
        if (meshAsset == null) {
            meshAsset = new Mesh ();
			// Hack to display mesh once saved
			CombineInstance[] combine = new CombineInstance[1];
			combine[0].mesh = mesh;
			combine[0].transform = Matrix4x4.identity;
			meshAsset.CombineMeshes(combine);

            EditorUtility.CopySerialized (mesh, meshAsset);
            AssetDatabase.CreateAsset (meshAsset, path);
        } else {
            meshAsset.Clear();
			// Hack to display mesh once saved
			CombineInstance[] combine = new CombineInstance[1];
			combine[0].mesh = mesh;
			combine[0].transform = Matrix4x4.identity;
			meshAsset.CombineMeshes(combine);

			EditorUtility.CopySerialized (mesh, meshAsset);
            AssetDatabase.SaveAssets ();
        }

    }

    public void CalculateBoneWeights(Bone[] bones) {
		CalculateBoneWeights(bones, false);
    }

    public void CalculateBoneWeights(Bone[] bones, bool weightToParent) {
		if (!lockBoneWeights) {
			if(meshFilter.sharedMesh == null)
			{
				Debug.Log("No Shared Mesh.");
				return;
			}
			Mesh mesh = new Mesh();
			mesh.name = meshFilter.sharedMesh.name;
			mesh.name = mesh.name.Replace(".Mesh", ".SkinnedMesh");
			mesh.vertices = meshFilter.sharedMesh.vertices;
			mesh.triangles = meshFilter.sharedMesh.triangles;
			mesh.normals = meshFilter.sharedMesh.normals;
			mesh.uv = meshFilter.sharedMesh.uv;
			mesh.uv2 = meshFilter.sharedMesh.uv2;
			mesh.bounds = meshFilter.sharedMesh.bounds;

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

				skinnedMeshRenderer.bones = bonesArr;

				// Check if the Meshes directory exists, if not, create it.
				if(!Directory.Exists("Assets/Meshes")) {
					AssetDatabase.CreateFolder("Assets", "Meshes");
					AssetDatabase.Refresh();
				}
				string path = "Assets/Meshes/" + mesh.name + ".asset";
				CreateOrReplaceAsset (mesh, path);
				AssetDatabase.Refresh();

				Mesh generatedMesh = AssetDatabase.LoadAssetAtPath (path, typeof(Mesh)) as Mesh;

				skinnedMeshRenderer.sharedMesh = generatedMesh;
				EditorUtility.SetDirty(skinnedMeshRenderer.gameObject);
				AssetDatabase.SaveAssets();
			}
		}
    }

    private void CalculateVertexColors() {
        GameObject go = Selection.activeGameObject;

        if (go == lastSelected || meshFilter.sharedMesh == null) {
            return;
        }

        lastSelected = go;

        Mesh m = skinnedMeshRenderer.sharedMesh;

        Color[] colors = new Color[m.vertexCount];

        for (int i = 0; i < colors.Length; i++) {
            colors[i] = Color.black;
        }

        if (go != null) {
            Bone bone = go.GetComponent<Bone>();

            if (bone != null) {
                if (skinnedMeshRenderer.bones.Any(b => b.gameObject.GetInstanceID() == bone.gameObject.GetInstanceID())) {
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

        meshFilter.sharedMesh.colors = colors;
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
		mesh.name = meshFilter.sharedMesh.name.Replace(".Mesh", ".SkinnedMesh");
		mesh.vertices = skinnedMeshRenderer.sharedMesh.vertices;
		mesh.triangles = skinnedMeshRenderer.sharedMesh.triangles;
		mesh.normals = skinnedMeshRenderer.sharedMesh.normals;
		mesh.uv = skinnedMeshRenderer.sharedMesh.uv;
		mesh.uv2 = skinnedMeshRenderer.sharedMesh.uv2;
		mesh.bounds = skinnedMeshRenderer.sharedMesh.bounds;

		// Check if the Meshes directory exists, if not, create it.
		if(!Directory.Exists("Assets/Meshes")) {
			AssetDatabase.CreateFolder("Assets", "Meshes");
			AssetDatabase.Refresh();
		}
		string meshPath = "Assets/Meshes/" + mesh.name + ".asset";
		Mesh generatedMesh = AssetDatabase.LoadMainAssetAtPath (meshPath) as Mesh;
		if (generatedMesh == null) {
			generatedMesh = new Mesh();
			// Hack to display mesh once saved
			CombineInstance[] combine = new CombineInstance[1];
			combine[0].mesh = mesh;
			combine[0].transform = Matrix4x4.identity;
			generatedMesh.CombineMeshes(combine);

			EditorUtility.CopySerialized(mesh, generatedMesh);
			AssetDatabase.CreateAsset(generatedMesh, meshPath);
			AssetDatabase.Refresh();
		}

		// Ensure there is a reference material for the renderer
		if (referenceMaterial == null) {
			if (skinnedMeshRenderer.sharedMaterial.name.Contains(" (Instance)")) {
				string materialName = skinnedMeshRenderer.sharedMaterial.name.Replace(" (Instance)", "");
				Debug.Log(materialName);
				Material material = AssetDatabase.LoadAssetAtPath("Assets/Materials/" + materialName + ".mat", typeof(Material)) as Material;
				referenceMaterial = material;
			} 
			else {
				referenceMaterial = skinnedMeshRenderer.sharedMaterial;
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

		// Make sure the skinned mesh renderer is using the stored generated mesh
		skinnedMeshRenderer.sharedMesh = generatedMesh;

		// Make sure the renderer is using a material
		if (referenceMaterial != null) {
			skinnedMeshRenderer.sharedMaterial = referenceMaterial;
		}

		// Add the mesh back to the prefab ** No Longer Using this as prefabs are less stable this way **
        // AssetDatabase.AddObjectToAsset(skinnedMeshRenderer.sharedMesh, obj);

		// Replace the prefab
        PrefabUtility.ReplacePrefab(gameObject, obj, ReplacePrefabOptions.ConnectToPrefab);
		EditorUtility.SetDirty(skinnedMeshRenderer.gameObject);
		AssetDatabase.SaveAssets();
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
		ControlPoint[] cps = GetComponentsInChildren<ControlPoint>();
		if (cps != null && cps.Length > 0) {
			for (int n = 0; n < cps.Length; n++) {
				cps[n].ResetPosition();
				// Debug.Log("Reset Control Point Positions for " + gameObject.name);
			}
		}
		if (controlPoints != null && controlPoints.Length > 0) {
			for (int i = 0; i < controlPoints.Length; i++) {
				controlPoints[i].ResetPosition();
				points.SetPoint(controlPoints[i]);
			}
		}
    }

    public void CreateControlPoints(SkinnedMeshRenderer skin) {
        if (skin.sharedMesh != null)
		{
			controlPoints = new ControlPoints.Point[skin.sharedMesh.vertices.Length];
			if (points == null) {
				points = gameObject.AddComponent<ControlPoints>();
			}
			for (int i = 0; i < skin.sharedMesh.vertices.Length; i++)
			{
				Vector3 originalPos = skin.sharedMesh.vertices[i];

				controlPoints[i] = new ControlPoints.Point(originalPos);
				controlPoints[i].index = i;
				points.SetPoint(controlPoints[i]);

				GameObject b = new GameObject(skin.name + " Control Point");
				// Unparent the skin temporarily before adding the control point
				Transform skinParent = skin.transform.parent;
				skin.transform.parent = null;

				// Reset the rotation before creating the mesh so the UV's will align properly
				Quaternion localRotation = skin.transform.localRotation;
				skin.transform.localRotation = Quaternion.identity;

				b.transform.position = new Vector3(skin.transform.position.x + (skin.sharedMesh.vertices[i].x * skin.transform.localScale.x), skin.transform.position.y + (skin.sharedMesh.vertices[i].y * skin.transform.localScale.y), skin.transform.position.z + (skin.sharedMesh.vertices[i].z * skin.transform.localScale.z));
				b.transform.parent = skin.transform;
				ControlPoint[] cps = b.transform.parent.transform.GetComponentsInChildren<ControlPoint>();
				if (cps != null && cps.Length > 0)
				{
					b.gameObject.name = b.gameObject.name + cps.Length;
				}
				Undo.RegisterCreatedObjectUndo(b, "Add control point");
				ControlPoint controlPoint = b.AddComponent<ControlPoint>();
				controlPoint.index = i;
				controlPoint.skin = skin;
				controlPoint.originalPosition = b.transform.localPosition;

				// Reset the rotations of the object
				skin.transform.localRotation = localRotation;
				skin.transform.parent = skinParent;
			}
		}
    }

	public void RemoveControlPoints() {
		ControlPoint[] cps = GetComponentsInChildren<ControlPoint>();
		if (cps != null && cps.Length > 0)
		{
			for (int i = 0; i < cps.Length; i++) {
				DestroyImmediate(cps[i].gameObject);
			}
		}

		controlPoints = null;

		if (points != null) {
			DestroyImmediate(points);
		}

		points = null;
	}
#endif
}
