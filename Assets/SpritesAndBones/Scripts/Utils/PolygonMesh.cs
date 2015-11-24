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
using UnityEngine.Sprites;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using System.IO;
#endif

public class PolygonMesh {

	public PolygonCollider2D polygonCollider;
	public SpriteRenderer spriteRenderer;
	private float pixelsToUnits = 100f;

	public void CreatePolygonMesh() 
	{
		if (polygonCollider != null)
		{
			// Unparent the skin temporarily before adding the mesh
			Transform polygonParent = polygonCollider.transform.parent;
			polygonCollider.transform.parent = null;

			// Reset the rotation before creating the mesh so the UV's will align properly
			Quaternion localRotation = polygonCollider.transform.localRotation;
			polygonCollider.transform.localRotation = Quaternion.identity;

			// Reset the scale before creating the mesh so the UV's will align properly
			Vector3 localScale = polygonCollider.transform.localScale;
			polygonCollider.transform.localScale = Vector3.one;

			Vector2[] vertices2D = polygonCollider.points;

			// Use the triangulator to get indices for creating triangles
			int[] indices = Triangulator.Triangulate(vertices2D);

			// Create the Vector3 vertices
			Vector3[] vertices = new Vector3[vertices2D.Length];
			for (int i=0; i<vertices.Length; i++) {
				vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
			}

			// Create the mesh
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = indices;

			spriteRenderer = polygonCollider.GetComponent<SpriteRenderer>();

			if (spriteRenderer != null)
			{
				mesh.uv = genUV(mesh.vertices);
				mesh.RecalculateNormals();
				mesh.RecalculateBounds();
			}

			else
			{
				Vector2[] uvs = new Vector2[vertices.Length];
				Bounds bounds = polygonCollider.bounds;
				int n = 0;
				while (n < uvs.Length) {
					uvs[n] = new Vector2(vertices[n].x / bounds.size.x, vertices[n].y / bounds.size.x);
					n++;
				}
				mesh.uv = uvs;
				mesh.RecalculateNormals();
				mesh.RecalculateBounds();
			}
#if UNITY_EDITOR
			// Check if the Meshes directory exists, if not, create it.
			DirectoryInfo meshDir = new DirectoryInfo("Assets/Meshes");
			if (Directory.Exists(meshDir.FullName) == false)
			{
				Directory.CreateDirectory(meshDir.FullName);
			}
			ScriptableObjectUtility.CreateAsset(mesh, "Meshes/" + polygonCollider.gameObject.name + ".Mesh");
#endif

			// Reset the rotations of the object
			polygonCollider.transform.localRotation = localRotation;
			polygonCollider.transform.localScale = localScale;
			polygonCollider.transform.parent = polygonParent;
		}
	}

	public Vector2[] genUV (Vector3[] vertices)
	{
		if (spriteRenderer != null && polygonCollider != null)
		{
			// Get the sprite's texture dimensions as float values
			float texHeight = (float)(spriteRenderer.sprite.texture.height);
			// Debug.Log(texHeight);
			float texWidth = (float)(spriteRenderer.sprite.texture.width);
			// Debug.Log(texWidth);

			// Get the bottom left position of the sprite renderer bounds in local space
			Vector3 botLeft = polygonCollider.transform.InverseTransformPoint(new Vector3 (spriteRenderer.bounds.min.x, spriteRenderer.bounds.min.y, 0));

			// Get the sprite's texture origin from the sprite's rect as float values
			Vector2 spriteTextureOrigin;
			spriteTextureOrigin.x = (float)spriteRenderer.sprite.rect.x;
			spriteTextureOrigin.y = (float)spriteRenderer.sprite.rect.y;

			// Calculate Pixels to Units using the current sprite rect width and the sprite bounds
			pixelsToUnits = spriteRenderer.sprite.rect.width / spriteRenderer.sprite.bounds.size.x;

			Vector2[] uv = new Vector2[vertices.Length];
			for (int i = 0; i<vertices.Length; i++) {
				// Apply the bottom left and lower left offset values to the vertices before applying the pixels to units 
				// to get the pixel value
				float x = (vertices [i].x - botLeft.x) * pixelsToUnits;
				float y = (vertices [i].y - botLeft.y) * pixelsToUnits;

				// Add the sprite's origin on the texture to the vertices and divide by the dimensions to get the UV
				uv [i] = new Vector2 (((x + spriteTextureOrigin.x) / texWidth), ((y + spriteTextureOrigin.y) / texHeight));
			}
			return uv;
		}
		else
		{
			return null;
		}
	}
}
