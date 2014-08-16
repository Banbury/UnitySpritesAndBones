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

using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using Poly2Tri;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;

public class SpriteMesh {
#if UNITY_EDITOR
	public static Mesh CreateSpriteMesh(Transform owner, Sprite sprite) {
		if (owner != null && sprite != null) {
			// Unparent the skin temporarily before adding the mesh
			Transform parent = owner.parent;
			owner.parent = null;

			// Reset the rotation before creating the mesh so the UV's will align properly
			Quaternion localRotation = owner.localRotation;
			owner.localRotation = Quaternion.identity;

			// Reset the scale before creating the mesh so the UV's will align properly
			Vector3 localScale = owner.localScale;
            owner.localScale = Vector3.one;

			Vector2[] vertices2D = UnityEditor.Sprites.DataUtility.GetSpriteMesh(sprite, false);
            int[] indices = UnityEditor.Sprites.DataUtility.GetSpriteIndices(sprite, false).Select(element => (int)element).ToArray();

			// Create the Vector3 vertices
			Vector3[] vertices = new Vector3[vertices2D.Length];
			for (int i=0; i<vertices.Length; i++) {
				vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
			}

			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = indices;
			Vector2[] uvs = UnityEditor.Sprites.DataUtility.GetSpriteUVs(sprite, false);
			mesh.uv = uvs;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			// Reset the rotations of the object
            owner.localRotation = localRotation;
            owner.localScale = localScale;
            owner.parent = parent;

		    return mesh;
		}

	    return null;
	}

    public static void CreateSpriteMeshAsset(Transform owner, Sprite sprite) {
        Mesh mesh = CreateSpriteMesh(owner, sprite);
        if (mesh != null) {
            ScriptableObjectUtility.CreateAsset(mesh);
        }
    }

    public static Mesh CreateSpriteMeshPoly(Sprite sprite) {
        if (sprite != null) {
            Vector2[] poly = CreatePolygon(sprite);
            Mesh mesh = CreateMesh(sprite, poly);
            return mesh;
        }

        return null;
    }

    private static Vector2[] CreatePolygon(Sprite sprite) {
        Rect r = sprite.rect;
        Texture2D tex = sprite.texture;
        IBitmap bmp = ArrayBitmap.CreateFromTexture(tex, new Rect(r.x, r.y, r.width, r.height));
        Vector2[] polygon = BitmapHelper.CreateFromBitmap(bmp);
        polygon = SimplifyTools.DouglasPeuckerSimplify(new Vertices(polygon), 1.0f).ToArray();
        return polygon;
    }

    private static Mesh CreateMesh(Sprite sprite, Vector2[] polygon) {
        if (sprite != null && polygon != null) {
            Rect bounds = GetBounds(polygon);

            DTSweepContext ctx = new DTSweepContext();
            Polygon poly = new Polygon(polygon.Select(p => new PolygonPoint(p.x, p.y)));

            ctx.PrepareTriangulation(poly);
            DTSweep.Triangulate(ctx);

            List<Vector2> verts = new List<Vector2>();
            List<int> tris = new List<int>();

            foreach (DelaunayTriangle tri in poly.Triangles) {
                verts.AddRange(tri.Points.Reverse().Select(p => new Vector2(p.Xf, p.Yf)));
                for (int i = 0; i < 3; i++) {
                    tris.Add(tris.Count);
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = verts.Select(x => (Vector3)x).ToArray();
            mesh.triangles = tris.ToArray();

            List<Vector2> uv = new List<Vector2>();

            Vector3 lower = new Vector3(bounds.x, bounds.y);
            Vector3 size = new Vector3(bounds.xMax, bounds.yMax) - lower;

            Rect uvBounds = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);

            float scalex = sprite.bounds.size.x / bounds.width;
            float scaley = sprite.bounds.size.y / bounds.height;

            Vector3[] scaled = mesh.vertices;

            for (int i = 0; i < mesh.vertices.Length; i++) {
                Vector3 v = scaled[i];
                Vector3 rel = v - lower;
                uv.Add(new Vector2(rel.x / size.x * uvBounds.width, rel.y / size.y * uvBounds.height) +
                       new Vector2(uvBounds.x, uvBounds.y));

                scaled[i] = new Vector3(v.x * scalex, v.y * scaley, v.z) - ((Vector3)bounds.center * scalex) +
                            sprite.bounds.center;
            }

            mesh.vertices = scaled;
            mesh.uv = uv.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();

            return mesh;
        }

        return null;
    }

    public static Bone2DWeights CalculateBoneWeights(Transform owner, Bone[] bones, Mesh mesh) {
        if (bones != null) {
            Bone2DWeights boneWeights = new Bone2DWeights();
            boneWeights.weights = new Bone2DWeight[] { };

            int vIndex = 0;
            foreach (Vector3 v in mesh.vertices) {
                Bone closest = FindClosestBone(v, bones);
                if (closest != null) {
                    boneWeights.SetWeight(vIndex++, closest.name, Array.IndexOf(bones, closest), 1f);
                }
            }


            BoneWeight[] unitweights = boneWeights.GetUnityBoneWeights();
            mesh.boneWeights = unitweights;

            Transform[] bonesArr = bones.Select(b => b.transform).ToArray();
            Matrix4x4[] bindPoses = new Matrix4x4[bonesArr.Length];

            for (int i = 0; i < bonesArr.Length; i++) {
                bindPoses[i] = bonesArr[i].worldToLocalMatrix * owner.localToWorldMatrix;
            }

            mesh.bindposes = bindPoses;

            return boneWeights;
        }

        return null;
    }

    private static Rect GetBounds(IEnumerable<Vector2> poly) {
        float bx1 = poly.Min(p => p.x);
        float by1 = poly.Min(p => p.y);
        float bx2 = poly.Max(p => p.x);
        float by2 = poly.Max(p => p.y);

        return new Rect(bx1, by1, bx2 - bx1, by2 - by1);
    }


    private static Bone FindClosestBone(Vector3 v, Bone[] bones) {
        Bone bmin = null;
        float minDist = float.MaxValue;

        foreach (Bone b in bones.Where(b => b.deform)) {
            float dist = (b.transform.localPosition - v).sqrMagnitude;
            if (dist < minDist) {
                minDist = dist;
                bmin = b;
            }
        }

        return bmin;
    }
#endif
}
