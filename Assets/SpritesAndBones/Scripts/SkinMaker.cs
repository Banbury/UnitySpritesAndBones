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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class SkinMaker : MonoBehaviour {
    public bool edit = true;
    public Texture image;
    public Vector3[] controlPoints;

    [HideInInspector]
    public Mesh mesh;

    [HideInInspector]
    public Mesh controlMesh;


    void Reset() {
        ResetMesh();
        controlMesh.name = "ControlMesh";

        GetComponent<MeshFilter>().mesh = controlMesh;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void SetImage(Texture img) {
        image = img;

        ResetMesh();

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr.sharedMaterial == null) {
            Material mat = new Material(Shader.Find("Unlit/Transparent"));
            mat.SetTexture("_MainTex", image);

            mr.sharedMaterial = mat;
        }
        else {
            mr.sharedMaterial.SetTexture("_MainTex", image);
        }

    }

    public void ResetMesh() {
        mesh.Clear(false);
        mesh.vertices = new Vector3[] { new Vector2(-1, 1), new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, -1) };
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
        mesh.uv = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        mesh.RecalculateNormals();

        controlPoints = mesh.vertices;

        CopyMesh();
    }

    private void CopyMesh() {
        controlMesh.vertices = mesh.vertices;
        controlMesh.triangles = mesh.triangles;
        controlMesh.normals = mesh.normals;
        controlMesh.uv = mesh.uv;
    }

    public void GenerateMesh() {
        mesh.Clear();

        int[] faces = Triangulator.Triangulate(controlPoints.Select(x => (Vector2)x).ToArray());

        mesh.vertices = controlPoints;
        mesh.triangles = faces;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        List<Vector2> uv = new List<Vector2>();

        foreach (Vector3 v in controlPoints) {
            uv.Add((v + new Vector3(1, 1)) / 2);
        }

        mesh.uv = uv.ToArray();

        CopyMesh();
    }

    public void SubdivideMesh() {
        MeshHelper.Subdivide(mesh, 2);

        CopyMesh();
    }

    public void SetEditMode(bool value) {
        edit = value;

        if (edit) {
            GetComponent<MeshFilter>().mesh = mesh;
        }
        else {
            GetComponent<MeshFilter>().mesh = controlMesh;
        }
    }
}
