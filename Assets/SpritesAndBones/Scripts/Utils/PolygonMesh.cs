using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
				uvs[n] = new Vector2(vertices[n].x / bounds.size.x, vertices[n].z / bounds.size.x);
				n++;
			}
			mesh.uv = uvs;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			ScriptableObjectUtility.CreateAsset(mesh);
		}
	}
}
