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
using System;
using System.Collections;
using System.Linq;

public class SpriteMesh {
#if UNITY_EDITOR

	public SpriteRenderer spriteRenderer;

	public void CreateSpriteMesh() 
	{
		if (spriteRenderer != null && spriteRenderer.sprite != null)
		{
			// Unparent the skin temporarily before adding the mesh
			Transform spriteRendererParent = spriteRenderer.transform.parent;
			spriteRenderer.transform.parent = null;

			// Reset the rotation before creating the mesh so the UV's will align properly
			Quaternion localRotation = spriteRenderer.transform.localRotation;
			spriteRenderer.transform.localRotation = Quaternion.identity;

			// Reset the scale before creating the mesh so the UV's will align properly
			Vector3 localScale = spriteRenderer.transform.localScale;
			spriteRenderer.transform.localScale = Vector3.one;

			Vector2[] vertices2D = spriteRenderer.sprite.vertices;
			int[] indices = spriteRenderer.sprite.triangles.Select(element => (int)element).ToArray();

			// Create the Vector3 vertices
			Vector3[] vertices = new Vector3[vertices2D.Length];
			for (int i=0; i<vertices.Length; i++) {
				vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
			}

			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = indices;
			Vector2[] uvs = UnityEditor.Sprites.SpriteUtility.GetSpriteUVs(spriteRenderer.sprite, false);
			mesh.uv = uvs;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			// Check if the Meshes directory exists, if not, create it.
			DirectoryInfo meshDir = new DirectoryInfo("Assets/Meshes");
			if (Directory.Exists(meshDir.FullName) == false)
			{
				Directory.CreateDirectory(meshDir.FullName);
			}
			ScriptableObjectUtility.CreateAsset(mesh, "Meshes/" + spriteRenderer.gameObject.name + ".Mesh");

			// Reset the rotations of the object
			spriteRenderer.transform.localRotation = localRotation;
			spriteRenderer.transform.localScale = localScale;
			spriteRenderer.transform.parent = spriteRendererParent;
		}
	}
#endif
}
