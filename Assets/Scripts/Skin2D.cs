using UnityEngine;
using UnityEditor;
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

    [MenuItem("GameObject/Create Other/Skin 2D")]
    public static void Create() {
        GameObject o = new GameObject("Skin2D");
        Undo.RegisterCreatedObjectUndo(o, "Create Skin2D");
        o.AddComponent<Skin2D>();
    }
    
	// Use this for initialization
	void Start() {
        meshFilter = GetComponent<MeshFilter>();

#if UNITY_EDITOR
        lineMaterial = new Material(Shader.Find("Lines/Colored Blended"));
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;

        CalculateVertexColors();
#endif
    }

	// Update is called once per frame
	void Update () {
	}

    void OnDrawGizmos() {
#if UNITY_EDITOR
        if (Application.isEditor && meshFilter.sharedMesh != null) {
            CalculateVertexColors();
            GL.wireframe = true;
            lineMaterial.SetPass(0);
            Graphics.DrawMeshNow(meshFilter.sharedMesh, transform.position, transform.rotation);
            GL.wireframe = false;
        }
#endif
    }

    public void CalculateBoneWeights() {
        Mesh mesh = new Mesh();
        mesh.name = "Generated Mesh";
        mesh.vertices = meshFilter.sharedMesh.vertices;
        mesh.triangles = meshFilter.sharedMesh.triangles;
        mesh.normals = meshFilter.sharedMesh.normals;
        mesh.uv = meshFilter.sharedMesh.uv;
        mesh.uv2 = meshFilter.sharedMesh.uv2;
        mesh.bounds = meshFilter.sharedMesh.bounds;

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

        if (go == lastSelected || meshFilter.sharedMesh == null) {
            return;
        }

        lastSelected = go;

        Mesh m = meshFilter.sharedMesh;

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
}
