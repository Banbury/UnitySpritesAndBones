using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Poly2Tri;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Common;
using System.IO;

public class SkinMesh : EditorWindow {
    private SpriteRenderer spriteRenderer;

    private Vector2[] polygon = new Vector2[0];
    private float simplify = 1.0f;

    [MenuItem("Window/Sprites/Create Mesh")]
    protected static void ShowSkinMeshEditor() {
        var wnd = GetWindow<SkinMesh>();
        wnd.title = "Create Mesh From Sprite";
        wnd.Show();
    }

    public void OnGUI() {
        GUILayout.Label("Sprite", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        spriteRenderer = (SpriteRenderer)EditorGUILayout.ObjectField(spriteRenderer, typeof(SpriteRenderer), true);
        if (EditorGUI.EndChangeCheck()) {
            polygon = new Vector2[0]; 
        }

        if (spriteRenderer != null) {
            EditorGUILayout.Separator();

            GUILayout.Label("Simplify", EditorStyles.boldLabel);

            simplify = EditorGUILayout.FloatField("Vertex Dist.", simplify);

            if (GUILayout.Button("Generate Polygon")) {
                Rect r = spriteRenderer.sprite.rect;
                Texture2D tex = spriteRenderer.sprite.texture;
                IBitmap bmp = ArrayBitmap.CreateFromTexture(tex, new Rect(r.x, r.y, r.width, r.height));
                polygon = BitmapHelper.CreateFromBitmap(bmp);
                polygon = SimplifyTools.DouglasPeuckerSimplify(new Vertices(polygon), simplify).ToArray();
            }

            GUILayout.Label("Vertices: " + polygon.Length);

            EditorGUILayout.Separator();

            if (polygon.Length > 0 && GUILayout.Button("Create Mesh")) {
                CreateMesh();
            }
        }
    }

    private void CreateMesh() {
        Sprite sprite = spriteRenderer.sprite;

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

        Rect uv_bounds = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height, sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);

        float scalex = sprite.bounds.size.x / bounds.width;
        float scaley = sprite.bounds.size.y / bounds.height;

        Vector3[] scaled = mesh.vertices;

        for (int i = 0; i < mesh.vertices.Length; i++) {
            Vector3 v = scaled[i];
            Vector3 rel = v - lower;
            uv.Add(new Vector2(rel.x / size.x * uv_bounds.width, rel.y / size.y * uv_bounds.height) + new Vector2(uv_bounds.x, uv_bounds.y));

            scaled[i] = new Vector3(v.x * scalex, v.y * scaley, v.z) - ((Vector3)bounds.center * scalex) + sprite.bounds.center;
        }

        mesh.vertices = scaled;
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        //GameObject go = new GameObject();
        //MeshFilter mf = go.AddComponent<MeshFilter>();
        //mf.sharedMesh = mesh;
        //MeshRenderer mr = go.AddComponent<MeshRenderer>();
        //mr.sharedMaterial = spriteRenderer.sharedMaterial;

        ScriptableObjectUtility.CreateAsset(mesh);
    }

    private static Rect GetBounds(IEnumerable<Vector2> poly) {
        float bx1 = poly.Min(p => p.x);
        float by1 = poly.Min(p => p.y);
        float bx2 = poly.Max(p => p.x);
        float by2 = poly.Max(p => p.y);

        return new Rect(bx1, by1, bx2 - bx1, by2 - by1);
    }

    private static bool PointInPoly(Vector2 p, IList<Vector2> poly) {
        int i, j = poly.Count() - 1;
        bool oddNodes = false;

        for (i = 0; i < poly.Count; i++) {
            if ((poly[i].y < p.y && poly[j].y >= p.y
            || poly[j].y < p.y && poly[i].y >= p.y)
            && (poly[i].x <= p.x || poly[j].x <= p.x)) {
                oddNodes ^= (poly[i].x + (p.y - poly[i].y) / (poly[j].y - poly[i].y) * (poly[j].x - poly[i].x) < p.x);
            }
            j = i;
        }

        return oddNodes;
    }

	public bool IsBadTriangle(DelaunayTriangle triangle) {
		// Finde die längste Seite
		// Berechne den Cosinus des Winkels direkt gegenüber (law of cosine)
		// Das Dreieck ist schlecht, wenn der Wert <= -0.5 ist.
		return false;
	}

}
