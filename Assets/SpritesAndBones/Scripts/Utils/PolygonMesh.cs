using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class PolygonMesh {

	public PolygonCollider2D polygonCollider;

	public void CreatePolygonMesh() 
	{
		if (polygonCollider != null)
		{
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
			ScriptableObjectUtility.CreateAsset(mesh);
		}
	}
}
