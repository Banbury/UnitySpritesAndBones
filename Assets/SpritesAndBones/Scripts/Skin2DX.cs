/*
The MIT License (MIT)

Copyright (c) 2014 Banbury

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[ExecuteInEditMode()]
public class Skin2DX : MonoBehaviour {
    public Sprite sprite;
    public Bone root;
    public Material material;

    [HideInInspector]
    public bool locked = false;

    [HideInInspector]
    public Bone2DWeights boneWeights;

#if UNITY_EDITOR
    public void Start() {
    }

    public void Reset() {
        CleanUp();
    }

    public void OnValidate() {
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer.sharedMesh == null) {
            BuildMesh();
        }
    }

    public void BuildMesh() {
        if (PrefabUtility.GetPrefabType(gameObject) == PrefabType.None && sprite != null && root != null) {
            SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

            CleanUp();
            Mesh mesh = SpriteMesh.CreateSpriteMeshPoly(sprite);
            mesh.name = "Generated Mesh";
            Bone[] bones = root.GetComponentsInChildren<Bone>();
            boneWeights = SpriteMesh.CalculateBoneWeights(transform, bones, mesh);

            skinnedMeshRenderer.rootBone = root.transform;
            skinnedMeshRenderer.bones = bones.Select(b => b.transform).ToArray();
            skinnedMeshRenderer.sharedMesh = mesh;

            if (material != null) {
                skinnedMeshRenderer.sharedMaterial = material;
            }
            else {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.mainTexture = sprite.texture;
                skinnedMeshRenderer.sharedMaterial = mat;
            }

            AssetDatabase.SaveAssets();
        }
    }

    private void CleanUp() {
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer.sharedMesh != null && !AssetDatabase.Contains(skinnedMeshRenderer.sharedMesh.GetInstanceID()))
            DestroyImmediate(skinnedMeshRenderer.sharedMesh);
    }


#endif
}
