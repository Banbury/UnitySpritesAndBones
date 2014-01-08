/**
 * Source: http://wiki.unity3d.com/index.php/MeshHelper
 * License: CC BY-SA 3.0
 * Author: ?
 **/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MeshHelper {
    static List<Vector3> vertices;
    static List<Vector3> normals;
    static List<Color> colors;
    static List<Vector2> uv;
    static List<Vector2> uv1;
    static List<Vector2> uv2;

    static List<int> indices;
    static Dictionary<uint, int> newVectices;

    static void InitArrays(Mesh mesh) {
        vertices = new List<Vector3>(mesh.vertices);
        normals = new List<Vector3>(mesh.normals);
        colors = new List<Color>(mesh.colors);
        uv = new List<Vector2>(mesh.uv);
        uv1 = new List<Vector2>(mesh.uv1);
        uv2 = new List<Vector2>(mesh.uv2);
        indices = new List<int>();
    }
    static void CleanUp() {
        vertices = null;
        normals = null;
        colors = null;
        uv = null;
        uv1 = null;
        uv2 = null;
        indices = null;
    }

    #region Subdivide4 (2x2)
    static int GetNewVertex4(int i1, int i2) {
        int newIndex = vertices.Count;
        uint t1 = ((uint)i1 << 16) | (uint)i2;
        uint t2 = ((uint)i2 << 16) | (uint)i1;
        if (newVectices.ContainsKey(t2))
            return newVectices[t2];
        if (newVectices.ContainsKey(t1))
            return newVectices[t1];

        newVectices.Add(t1, newIndex);

        vertices.Add((vertices[i1] + vertices[i2]) * 0.5f);
        if (normals.Count > 0)
            normals.Add((normals[i1] + normals[i2]).normalized);
        if (colors.Count > 0)
            colors.Add((colors[i1] + colors[i2]) * 0.5f);
        if (uv.Count > 0)
            uv.Add((uv[i1] + uv[i2]) * 0.5f);
        if (uv1.Count > 0)
            uv1.Add((uv1[i1] + uv1[i2]) * 0.5f);
        if (uv2.Count > 0)
            uv2.Add((uv2[i1] + uv2[i2]) * 0.5f);

        return newIndex;
    }


    /// <summary>
    /// Devides each triangles into 4. A quad(2 tris) will be splitted into 2x2 quads( 8 tris )
    /// </summary>
    /// <param name="mesh"></param>
    public static void Subdivide4(Mesh mesh) {
        newVectices = new Dictionary<uint, int>();

        InitArrays(mesh);

        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3) {
            int i1 = triangles[i + 0];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            int a = GetNewVertex4(i1, i2);
            int b = GetNewVertex4(i2, i3);
            int c = GetNewVertex4(i3, i1);
            indices.Add(i1); indices.Add(a); indices.Add(c);
            indices.Add(i2); indices.Add(b); indices.Add(a);
            indices.Add(i3); indices.Add(c); indices.Add(b);
            indices.Add(a); indices.Add(b); indices.Add(c); // center triangle
        }
        mesh.vertices = vertices.ToArray();
        if (normals.Count > 0)
            mesh.normals = normals.ToArray();
        if (colors.Count > 0)
            mesh.colors = colors.ToArray();
        if (uv.Count > 0)
            mesh.uv = uv.ToArray();
        if (uv1.Count > 0)
            mesh.uv1 = uv1.ToArray();
        if (uv2.Count > 0)
            mesh.uv2 = uv2.ToArray();

        mesh.triangles = indices.ToArray();

        CleanUp();
    }
    #endregion Subdivide4 (2x2)

    #region Subdivide9 (3x3)
    static int GetNewVertex9(int i1, int i2, int i3) {
        int newIndex = vertices.Count;

        // center points don't go into the edge list
        if (i3 == i1 || i3 == i2) {
            uint t1 = ((uint)i1 << 16) | (uint)i2;
            if (newVectices.ContainsKey(t1))
                return newVectices[t1];
            newVectices.Add(t1, newIndex);
        }

        // calculate new vertex
        vertices.Add((vertices[i1] + vertices[i2] + vertices[i3]) / 3.0f);
        if (normals.Count > 0)
            normals.Add((normals[i1] + normals[i2] + normals[i3]).normalized);
        if (colors.Count > 0)
            colors.Add((colors[i1] + colors[i2] + colors[i3]) / 3.0f);
        if (uv.Count > 0)
            uv.Add((uv[i1] + uv[i2] + uv[i3]) / 3.0f);
        if (uv1.Count > 0)
            uv1.Add((uv1[i1] + uv1[i2] + uv1[i3]) / 3.0f);
        if (uv2.Count > 0)
            uv2.Add((uv2[i1] + uv2[i2] + uv2[i3]) / 3.0f);
        return newIndex;
    }


    /// <summary>
    /// Devides each triangles into 9. A quad(2 tris) will be splitted into 3x3 quads( 18 tris )
    /// </summary>
    /// <param name="mesh"></param>
    public static void Subdivide9(Mesh mesh) {
        newVectices = new Dictionary<uint, int>();

        InitArrays(mesh);

        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3) {
            int i1 = triangles[i + 0];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            int a1 = GetNewVertex9(i1, i2, i1);
            int a2 = GetNewVertex9(i2, i1, i2);
            int b1 = GetNewVertex9(i2, i3, i2);
            int b2 = GetNewVertex9(i3, i2, i3);
            int c1 = GetNewVertex9(i3, i1, i3);
            int c2 = GetNewVertex9(i1, i3, i1);

            int d = GetNewVertex9(i1, i2, i3);

            indices.Add(i1); indices.Add(a1); indices.Add(c2);
            indices.Add(i2); indices.Add(b1); indices.Add(a2);
            indices.Add(i3); indices.Add(c1); indices.Add(b2);
            indices.Add(d); indices.Add(a1); indices.Add(a2);
            indices.Add(d); indices.Add(b1); indices.Add(b2);
            indices.Add(d); indices.Add(c1); indices.Add(c2);
            indices.Add(d); indices.Add(c2); indices.Add(a1);
            indices.Add(d); indices.Add(a2); indices.Add(b1);
            indices.Add(d); indices.Add(b2); indices.Add(c1);
        }

        mesh.vertices = vertices.ToArray();
        if (normals.Count > 0)
            mesh.normals = normals.ToArray();
        if (colors.Count > 0)
            mesh.colors = colors.ToArray();
        if (uv.Count > 0)
            mesh.uv = uv.ToArray();
        if (uv1.Count > 0)
            mesh.uv1 = uv1.ToArray();
        if (uv2.Count > 0)
            mesh.uv2 = uv2.ToArray();

        mesh.triangles = indices.ToArray();

        CleanUp();
    }
    #endregion Subdivide9 (3x3)


    #region Subdivide
    /// <summary>
    /// This functions subdivides the mesh based on the level parameter
    /// Note that only the 4 and 9 subdivides are supported so only those divides
    /// are possible. [2,3,4,6,8,9,12,16,18,24,27,32,36,48,64, ...]
    /// The function tried to approximate the desired level 
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="level">Should be a number made up of (2^x * 3^y)
    /// [2,3,4,6,8,9,12,16,18,24,27,32,36,48,64, ...]
    /// </param>
    public static void Subdivide(Mesh mesh, int level) {
        if (level < 2)
            return;
        while (level > 1) {
            // remove prime factor 3
            while (level % 3 == 0) {
                Subdivide9(mesh);
                level /= 3;
            }
            // remove prime factor 2
            while (level % 2 == 0) {
                Subdivide4(mesh);
                level /= 2;
            }
            // try to approximate. All other primes are increased by one
            // so they can be processed
            if (level > 3)
                level++;
        }
    }
    #endregion Subdivide

    public static Mesh DuplicateMesh(Mesh mesh) {
        return (Mesh)UnityEngine.Object.Instantiate(mesh);
    }
}