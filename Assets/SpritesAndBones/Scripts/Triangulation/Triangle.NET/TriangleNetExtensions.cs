using UnityEngine;
using System.Collections;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Data;
using System.Collections.Generic;

public static class TriangleNetExtensions{
    /// <summary> Inserts points and segments of the given polygon to the input geometry </summary>
    static public void AddPolygon(this InputGeometry input, IList<Vector2> polygon){
        int inputCount = input.Count;

        input.AddPoint(polygon[0].x, polygon[0].y);
        for(int i = 1, j = 0; i < polygon.Count; j = i++ ) {
            input.AddPoint(polygon[i].x, polygon[i].y);
            input.AddSegment(inputCount + j, inputCount + i);
        }
        input.AddSegment(input.Count - 1, inputCount);
    }

    /// <summary> Converts the triangle array returned from Triangle.Net to an index array for Unity Mesh triangles </summary>
    static public int[] ToUnityMeshTriangleIndices(this ICollection<Triangle> triangles) {
        int[] tris = new int[triangles.Count * 3];
        int n = 0;
        foreach(var t in triangles) {
            tris[n++] = t.P1;
            tris[n++] = t.P0;
            tris[n++] = t.P2;
        }
        return tris;
    }
}