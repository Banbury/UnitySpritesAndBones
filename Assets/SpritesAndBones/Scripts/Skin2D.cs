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
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(SkinnedMeshRenderer))]
[ExecuteInEditMode()]
public class Skin2D : MonoBehaviour {
    public Skeleton skeleton;
    public Bone2DWeights boneWeights;

    private Material lineMaterial;
    private MeshFilter meshFilter;
    private GameObject lastSelected = null;

    #if UNITY_EDITOR
		[MenuItem("GameObject/Create Other/Skin 2D")]
		public static void Create ()
		{
				GameObject o = new GameObject ("Skin2D");
				Undo.RegisterCreatedObjectUndo (o, "Create Skin2D");
				o.AddComponent<Skin2D> ();
		}
    #endif
    
	// Use this for initialization
	void Start() {
#if UNITY_EDITOR
        CalculateVertexColors();
#endif
    }

	// Update is called once per frame
	void Update () {
        if (MeshFilter.sharedMesh != null && GetComponent<SkinnedMeshRenderer>().sharedMesh == null) {
            GetComponent<SkinnedMeshRenderer>().sharedMesh = MeshFilter.sharedMesh;
        }
	}

    private MeshFilter MeshFilter {
        get {
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();
            return meshFilter;
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

    public void CalculateBoneWeights() {
        Mesh mesh = new Mesh();
        mesh.name = "Generated Mesh";
        mesh.vertices = MeshFilter.sharedMesh.vertices;
        mesh.triangles = MeshFilter.sharedMesh.triangles;
        mesh.normals = MeshFilter.sharedMesh.normals;
        mesh.uv = MeshFilter.sharedMesh.uv;
        mesh.uv2 = MeshFilter.sharedMesh.uv2;
        mesh.bounds = MeshFilter.sharedMesh.bounds;

        if (skeleton != null && mesh != null) {
            boneWeights.weights = new Bone2DWeight[] { };

            Bone[] bones = skeleton.GetComponentsInChildren<Bone>();

            foreach (Bone bone in bones) {
                int i=0;

                foreach (Vector3 v in mesh.vertices) {
                    float influence = bone.GetInfluence(v + transform.position);
                    boneWeights.SetWeight(i, bone.name, bone.index, influence);
                    i++;
                }
            }

            var unitweights = boneWeights.GetUnityBoneWeights();
            mesh.boneWeights = unitweights;

            Transform[] bonesArr = bones.OrderBy(b => b.index).Select(b => b.transform).ToArray();
            Matrix4x4[] bindPoses = new Matrix4x4[bonesArr.Length];

            for (int i = 0; i < bonesArr.Length; i++) {
                bindPoses[i] = bonesArr[i].worldToLocalMatrix * transform.localToWorldMatrix;
            }

            mesh.bindposes = bindPoses;

            var renderer = GetComponent<SkinnedMeshRenderer>();
            if (renderer.sharedMesh != null && !AssetDatabase.Contains(renderer.sharedMesh.GetInstanceID()))
                Object.DestroyImmediate(renderer.sharedMesh);
            renderer.bones = bonesArr;
            renderer.sharedMesh = mesh;
        }
    }

    private void CalculateVertexColors() {
        GameObject go = Selection.activeGameObject;

        if (go == lastSelected || MeshFilter.sharedMesh == null) {
            return;
        }

        lastSelected = go;

        Mesh m = MeshFilter.sharedMesh;

        Color[] colors = new Color[m.vertexCount];

        for (int i = 0; i < colors.Length; i++) {
            colors[i] = Color.black;
        }

        if (go != null) {
            Bone bone = go.GetComponent<Bone>();

            if (bone != null) {
                SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();

                if (renderer.bones.Any(b => b.gameObject.GetInstanceID() == bone.gameObject.GetInstanceID())) {
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

                        colors[i] = HSBColor.ToColor(new HSBColor(0.7f - value, 1.0f, 0.5f));
                    }
                }
            }
        }

        m.colors = colors;
    }

    public void SaveAsPrefab() {
        string path = "Assets/" + gameObject.name + ".prefab";

        Object obj = PrefabUtility.CreateEmptyPrefab(path);

        AssetDatabase.AddObjectToAsset(GetComponent<SkinnedMeshRenderer>().sharedMesh, obj);

        PrefabUtility.ReplacePrefab(gameObject, obj, ReplacePrefabOptions.ConnectToPrefab);
    }
#endif
}
