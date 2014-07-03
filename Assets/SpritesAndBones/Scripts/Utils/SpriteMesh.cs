using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class SpriteMesh {

	public SpriteRenderer spriteRenderer;

	public void CreateSpriteMesh() 
	{
		if (spriteRenderer != null && spriteRenderer.sprite != null)
		{
			Vector2[] vertices2D = UnityEditor.Sprites.DataUtility.GetSpriteMesh(spriteRenderer.sprite, false);
			int[] indices = Array.ConvertAll<ushort, int>(UnityEditor.Sprites.DataUtility.GetSpriteIndices(spriteRenderer.sprite, false),  element => (int)element);

			// Create the Vector3 vertices
			Vector3[] vertices = new Vector3[vertices2D.Length];
			for (int i=0; i<vertices.Length; i++) {
				vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
			}

			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = indices;
			Vector2[] uvs = UnityEditor.Sprites.DataUtility.GetSpriteUVs(spriteRenderer.sprite, false);
			mesh.uv = uvs;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			ScriptableObjectUtility.CreateAsset(mesh);
		}
	}
}
