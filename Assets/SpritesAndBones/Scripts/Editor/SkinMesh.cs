/*
The MIT License (MIT)

Copyright (c) 2014 Banbury & Play-Em

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Poly2Tri;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Common;

[ExecuteInEditMode]
public class SkinMesh : EditorWindow {
    private SpriteRenderer spriteRenderer;

    private Vector2[] polygon = new Vector2[0];
    private float simplify = 1.0f;
    private int divisions = 2;

	public float handleScale = 0.3f;
	public Color handleColor = Color.white;
	public Color handleColorFirst = Color.magenta;
	public Color handleColorLast = Color.red;
	public Color polyColor = Color.cyan;

	public Vector2[] points = new Vector2[0];
	public Mesh mesh = new Mesh();
	public Mesh combineMesh;
	public Mesh customLoadMesh;

	private bool meshCreated = false;

    [MenuItem("Sprites And Bones/Create Mesh")]
    protected static void ShowSkinMeshEditor() {
        var wnd = GetWindow<SkinMesh>();
        wnd.titleContent.text = "Create Mesh";
        wnd.Show();

		SceneView.onSceneGUIDelegate += wnd.OnSceneGUI;
    }

    public void OnDestroy() {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
		meshCreated = false;
    }

    public void OnGUI() {
        GUILayout.Label("Sprite", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        spriteRenderer = (SpriteRenderer)EditorGUILayout.ObjectField(spriteRenderer, typeof(SpriteRenderer), true);
        if (Selection.activeGameObject != null) {
			GameObject o = Selection.activeGameObject;
			spriteRenderer = o.GetComponent<SpriteRenderer>();
		}
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
				EditorUtility.SetDirty(this);
            }

            GUILayout.Label("Vertices: " + polygon.Length);

            EditorGUILayout.Separator();

            if (polygon.Length > 0 && GUILayout.Button("Create Mesh")) {
                CreateMesh();
				EditorUtility.SetDirty(this);
            }

			if (GUILayout.Button("Create Mesh from Sprite")) {
				SpriteMesh spriteMesh = new SpriteMesh();
				spriteMesh.spriteRenderer = spriteRenderer;
				spriteMesh.CreateSpriteMesh();
				EditorUtility.SetDirty(this);
			}

			EditorGUILayout.Separator();

			if (GUILayout.Button("Create Mesh from Polygon2D Collider")) {
				PolygonCollider2D polygonCollider = spriteRenderer.GetComponent<PolygonCollider2D>();
				if (polygonCollider == null) {
					polygonCollider = spriteRenderer.gameObject.AddComponent<PolygonCollider2D>();
				}

				PolygonMesh polygonMesh = new PolygonMesh();
				polygonMesh.polygonCollider = polygonCollider;
				polygonMesh.spriteRenderer = spriteRenderer;
				polygonMesh.CreatePolygonMesh();
				EditorUtility.SetDirty(this);
			}

			EditorGUILayout.Separator();

            GUILayout.Label("Create and Edit a Custom Polygon", EditorStyles.boldLabel);
			handleScale = EditorGUILayout.FloatField("Handle Size", handleScale);
			handleColor = EditorGUILayout.ColorField("Handle Color", handleColor);
			polyColor = EditorGUILayout.ColorField("Poly Color", polyColor);

			EditorGUILayout.Separator();

            GUILayout.Label("Ctrl + Click to Add Point, Shift + Click to Add Mid Point, Alt + Click to Remove Points", EditorStyles.whiteLabel);

			EditorGUILayout.Separator();

			if (GUILayout.Button("Create and Edit Polygon")) {
				CreatePolygon();
				EditorUtility.SetDirty(this);
			}

			EditorUtility.SetSelectedWireframeHidden(spriteRenderer, true);

			if (GUILayout.Button("Update Custom Mesh")) {
				if (spriteRenderer != null) UpdateMesh();
				EditorUtility.SetDirty(this);
			}

			if (GUILayout.Button("Save Custom Mesh")) {
				if (spriteRenderer != null) SaveMesh();
			}

			if (GUI.changed) {
				UpdateMesh();
				EditorUtility.SetDirty(this);
			}

            GUILayout.Label("Subdivide Mesh", EditorStyles.boldLabel);

			string[] subdivide = { "0", "1", "2", "3" };

            divisions = EditorGUILayout.Popup("Subdivisions", divisions, subdivide);

			if (GUILayout.Button("Subdivide Mesh")) {
				if (spriteRenderer != null && mesh != null) SubdivideMesh(divisions);
			}

			EditorGUILayout.Separator();

            GUILayout.Label("Load Custom Mesh to Edit", EditorStyles.boldLabel);

			GUILayout.Label("Adding or Deleting points Re-Triangulates Mesh", EditorStyles.whiteLabel);

			EditorGUILayout.Separator();
			customLoadMesh = (Mesh)EditorGUILayout.ObjectField(customLoadMesh, typeof(Mesh), true);

			if (GUILayout.Button("Load Custom Mesh")) {
				if (spriteRenderer != null && customLoadMesh != null) {
                    LoadMesh(customLoadMesh);
				}
			}

			EditorGUILayout.Separator();

            GUILayout.Label("Combine Meshes", EditorStyles.boldLabel);

			GUILayout.Label("Avoid Combining Complex Meshes, results will vary", EditorStyles.whiteLabel);

			EditorGUILayout.Separator();
			combineMesh = (Mesh)EditorGUILayout.ObjectField(combineMesh, typeof(Mesh), true);

			if (GUILayout.Button("Combine Meshes")) {
				if (spriteRenderer != null) CombineMesh();
			}
        }
    }

    private void LoadMesh(Mesh loadMesh) {
        if(mesh != null) mesh.Clear();
        mesh = new Mesh();
        mesh.vertices = loadMesh.vertices;
        mesh.triangles = loadMesh.triangles;
        mesh.uv = genUV(mesh.vertices);
        mesh.normals = loadMesh.normals;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshCreated = true;

        points = new Vector2[mesh.vertices.Length];
        for(int i=0; i < mesh.vertices.Length; i++) {
            points[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
        }
        EditorUtility.SetDirty(this);
    }

	private void CreatePolygon() {
		GameObject go = Selection.activeGameObject;

		if (spriteRenderer != null && spriteRenderer.sprite != null)
		{
			List<Vector2> newPoints = new List<Vector2>();
			Vector3 worldPos = SceneView.currentDrawingSceneView.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1.0f)) - spriteRenderer.transform.position;
			newPoints.Add(new Vector2(worldPos.x, worldPos.y));
			points = newPoints.ToArray();
			if (mesh != null) mesh.Clear();
			mesh = null;
			meshCreated = false;
		}
		Undo.RegisterCreatedObjectUndo(go, "Created Polygon");
	}

    private void CreateMesh() {
        Sprite sprite = spriteRenderer.sprite;

        Rect bounds = GetBounds(polygon);

        TriangleNet.Mesh tnMesh = new TriangleNet.Mesh();
        TriangleNet.Geometry.InputGeometry input = new TriangleNet.Geometry.InputGeometry();

        input.AddPolygon(polygon);

        tnMesh.Triangulate(input);

        Mesh mesh = new Mesh();
        mesh.vertices = tnMesh.Vertices.Select(p => new Vector3((float)p.X, (float)p.Y, 0)).ToArray();

        // Not sure about winding
        // If there is an interesting error, It is probably because of cw/ccw windings
        int[] tris = tnMesh.Triangles.ToUnityMeshTriangleIndices();
        mesh.triangles = tris;

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

        mesh.MarkDynamic();
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

		// Check if the Meshes directory exists, if not, create it.
		DirectoryInfo meshDir = new DirectoryInfo("Assets/Meshes");
		if (Directory.Exists(meshDir.FullName) == false)
		{
			Directory.CreateDirectory(meshDir.FullName);
		}
		ScriptableObjectUtility.CreateAsset(mesh, "Meshes/" + spriteRenderer.gameObject.name + ".Mesh");
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


	public void OnSceneGUI(SceneView sceneView) {
		if (spriteRenderer == null) return;

		Event e = Event.current;

		if (e.type == EventType.ValidateCommand && e.commandName == "UndoRedoPerformed") {
			UpdateMesh();
			sceneView.Repaint();
		}
		if (points.Length == 0) return;

		bool closed = true;
		if (points.Length < 3) {
			closed = false;
		}

		Handles.matrix = spriteRenderer.transform.localToWorldMatrix;
		Handles.color = polyColor;

		// Handles.DrawPolyLine(mesh.vertices);
		if (mesh != null) {
            int triCount = mesh.triangles.Length / 3;
			for (int i = 0; i < triCount; i++) {
				Vector3 p1 = mesh.vertices[mesh.triangles[i * 3]];
				Vector3 p2 = mesh.vertices[mesh.triangles[(i * 3) + 1]];
				Vector3 p3 = mesh.vertices[mesh.triangles[(i * 3) + 2]];
				Handles.DrawLine(p1, p2);
				Handles.DrawLine(p2, p3);
				Handles.DrawLine(p3, p1);
			}
		}
		else {
			if (points.Length > 1 && !closed) {
				Handles.DrawLine(points[0], points[1]);
			}
		}

		EditorGUI.BeginChangeCheck();

		if (e.alt) {
			RemovePoint();
		}
		else {
			if (e.shift) {
				AddMidPoint();
			}
			else if (e.control) {
				AddPoint(e);
			}

			if (points.Length > 0) {
				for (int i = 0; i < points.Length; i++) {
					Vector3 point = new Vector3(points[i].x, points[i].y, 0);
					
					Handles.color = handleColor;
					GUI.SetNextControlName("polygon point " + i);
					if (i == 0) Handles.color = handleColorFirst;
					if (i == (points.Length - 1)) Handles.color = handleColorLast;
					float size = GetHandleSize(point, 1);
					point = Handles.FreeMoveHandle(
						point, 
						Quaternion.identity, 
						size, 
						Vector3.zero, 
						Handles.CircleCap
					);

					points[i] = point;
				}
			}
		}

		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(spriteRenderer, "moved polygon point");
			// UpdateMesh();
			if (mesh != null) {
				Vector3[] vertices = new Vector3[mesh.vertices.Length];
				System.Array.Copy(mesh.vertices, vertices, mesh.vertices.Length);
				if (points.Length > 0 && vertices.Length > 0 && points.Length == vertices.Length) {
					for (int i = 0; i < points.Length; i++) {
						Vector3 point = new Vector3(points[i].x, points[i].y, 0);
						vertices[i] = point;
					}
				}
				mesh.vertices = vertices;
				mesh.RecalculateBounds();
				mesh.RecalculateNormals();
			}
			sceneView.Repaint();
			EditorUtility.SetDirty(this);
		}
	}

	void AddPoint (Event e) {
		Ray r = HandleUtility.GUIPointToWorldRay(e.mousePosition);
		Vector3 worldPos = r.origin - spriteRenderer.transform.position;
		List<Vector2> newPoints = new List<Vector2>(points);
		float size = GetHandleSize(worldPos, 0.5f);
		if (Handles.Button(worldPos, Quaternion.identity, size, size, Handles.CircleCap)) {
			newPoints.Add(new Vector3(worldPos.x, worldPos.y, 0));
			Undo.RecordObject(this, "added polygon point");
			points = newPoints.ToArray();
			UpdateMesh();
			EditorUtility.SetDirty(this);
		}
	}

	void AddMidPoint () {
		List<Vector2> newPoints = new List<Vector2>(points);
		int len = points.Length;
		if (points.Length > 1) {
			for (int i = 0; i < len; i++) {
				int n = (i+1)%len;
				Vector3 p1 = points[i];
				Vector3 p2 = points[n];
				Handles.color = Color.green;
				GUI.SetNextControlName("remove polygon point " + i);
				Vector3 mid = (p1 + p2) * 0.5f;
				float size = GetHandleSize(mid, 0.5f);
				if (Handles.Button(mid, Quaternion.identity, size, size, Handles.CircleCap)) {
					newPoints.Insert(n, new Vector3(mid.x, mid.y, mid.z));
					Undo.RecordObject(this, "added polygon point");
					points = newPoints.ToArray();
					UpdateMesh();
					EditorUtility.SetDirty(this);
					break;
				}
			}
		}
	}

	void RemovePoint (int index) {
        if (index < 0 || index >= points.Length) return;

        Undo.RecordObject(this, "removed polygon point");
		List<Vector2> newPoints = new List<Vector2>(points);
        newPoints.RemoveAt(index);
        points = newPoints.ToArray();
		if (meshCreated) UpdateMesh();
		EditorUtility.SetDirty(this);
    }

    void RemovePoint () {
		if (points.Length > 1) {
			for (int i = 0; i < points.Length; i++) {
				Handles.color = Color.red;
				float size = GetHandleSize(points[i], 1);
				GUI.SetNextControlName("remove pretty poly point " + i);
				if (Handles.Button(points[i], Quaternion.identity, size, size, Handles.CircleCap)) {
					RemovePoint(i);
					break;
				}
			}
		}
    }

    float GetHandleSize (Vector3 pos, float size) {
    	return Mathf.Min(HandleUtility.GetHandleSize(pos), 0.5f) * size * handleScale;
    }

	void UpdateMesh() {
		if (points.Length > 2 && spriteRenderer != null) {
			// Unparent the skin temporarily before adding the mesh
			Transform polygonParent = spriteRenderer.transform.parent;
			spriteRenderer.transform.parent = null;

			// Reset the rotation before creating the mesh so the UV's will align properly
			Quaternion localRotation = spriteRenderer.transform.localRotation;
			spriteRenderer.transform.localRotation = Quaternion.identity;

			// Reset the scale before creating the mesh so the UV's will align properly
			Vector3 localScale = spriteRenderer.transform.localScale;
			spriteRenderer.transform.localScale = Vector3.one;

			// Vector2[] vertices2D = points;

			// Use the triangulator to get indices for creating triangles
			// int[] indices = Triangulator.Triangulate(vertices2D);

			// Create the Vector3 vertices
			// Vector3[] vertices = new Vector3[vertices2D.Length];
			// for (int i=0; i<vertices.Length; i++) {
				// vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
			// }

			// Create the mesh
			mesh = new Mesh();
			mesh.MarkDynamic();
			// mesh.vertices = vertices;
			// mesh.triangles = indices;
			mesh = CreateMeshFromPoints(false);

			mesh.uv = genUV(mesh.vertices);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
#if UNITY_EDITOR
			// ScriptableObjectUtility.CreateAsset(mesh);
#endif

			// Reset the rotations of the object
			spriteRenderer.transform.localRotation = localRotation;
			spriteRenderer.transform.localScale = localScale;
			spriteRenderer.transform.parent = polygonParent;

			meshCreated = true;
		}
	}

	public Vector2[] genUV (Vector3[] vertices)
	{
		if (spriteRenderer != null)
		{
			// Get the sprite's texture dimensions as float values
			float texHeight = (float)(spriteRenderer.sprite.texture.height);
			// Debug.Log(texHeight);
			float texWidth = (float)(spriteRenderer.sprite.texture.width);
			// Debug.Log(texWidth);

			// Get the bottom left position of the sprite renderer bounds in local space
			Vector3 botLeft = spriteRenderer.transform.InverseTransformPoint(new Vector3 (spriteRenderer.bounds.min.x, spriteRenderer.bounds.min.y, 0));

			// Get the sprite's texture origin from the sprite's rect as float values
			Vector2 spriteTextureOrigin;
			spriteTextureOrigin.x = (float)spriteRenderer.sprite.rect.x;
			spriteTextureOrigin.y = (float)spriteRenderer.sprite.rect.y;

			Vector2[] uv = new Vector2[vertices.Length];
			for (int i = 0; i<vertices.Length; i++) {
				// Apply the bottom left and lower left offset values to the vertices before applying the pixels to units 
				// to get the pixel value
				float x = (vertices [i].x - botLeft.x) * spriteRenderer.sprite.pixelsPerUnit;
				float y = (vertices [i].y - botLeft.y) * spriteRenderer.sprite.pixelsPerUnit;

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

	void SaveMesh() {
		if (mesh != null && spriteRenderer != null) {
		#if UNITY_EDITOR
			// Unparent the skin temporarily before adding the mesh
			Transform polygonParent = spriteRenderer.transform.parent;
			spriteRenderer.transform.parent = null;

			// Reset the rotation before creating the mesh so the UV's will align properly
			Quaternion localRotation = spriteRenderer.transform.localRotation;
			spriteRenderer.transform.localRotation = Quaternion.identity;

			// Reset the scale before creating the mesh so the UV's will align properly
			Vector3 localScale = spriteRenderer.transform.localScale;
			spriteRenderer.transform.localScale = Vector3.one;

			mesh.uv = genUV(mesh.vertices);
			// Check if the Mesh directory exists, if not, create it.
			DirectoryInfo meshDir = new DirectoryInfo("Assets/Meshes");
			if (Directory.Exists(meshDir.FullName) == false)
			{
				Directory.CreateDirectory(meshDir.FullName);
			}
			ScriptableObjectUtility.CreateAsset(mesh, "Meshes/" + spriteRenderer.gameObject.name + ".Mesh");

			// Reset the rotations of the object
			spriteRenderer.transform.localRotation = localRotation;
			spriteRenderer.transform.localScale = localScale;
			spriteRenderer.transform.parent = polygonParent;
		#endif
		}
	}

	void CombineMesh() {
		if (spriteRenderer!= null && mesh != null && combineMesh != null) {
			// Unparent the skin temporarily before adding the mesh
			Transform polygonParent = spriteRenderer.transform.parent;
			spriteRenderer.transform.parent = null;

			// Reset the rotation before creating the mesh so the UV's will align properly
			Quaternion localRotation = spriteRenderer.transform.localRotation;
			spriteRenderer.transform.localRotation = Quaternion.identity;

			// Reset the scale before creating the mesh so the UV's will align properly
			Vector3 localScale = spriteRenderer.transform.localScale;
			spriteRenderer.transform.localScale = Vector3.one;

			mesh = CreateMeshFromPoints(true);
			mesh.uv = genUV(mesh.vertices);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			// Reset the rotations of the object
			spriteRenderer.transform.localRotation = localRotation;
			spriteRenderer.transform.localScale = localScale;
			spriteRenderer.transform.parent = polygonParent;

			meshCreated = true;
		}
	}

	public void SubdivideMesh(int divisions) {
		if (spriteRenderer != null && points.Length > 2) {
			// Unparent the skin temporarily before adding the mesh
			Transform polygonParent = spriteRenderer.transform.parent;
			spriteRenderer.transform.parent = null;

			// Reset the rotation before creating the mesh so the UV's will align properly
			Quaternion localRotation = spriteRenderer.transform.localRotation;
			spriteRenderer.transform.localRotation = Quaternion.identity;

			// Reset the scale before creating the mesh so the UV's will align properly
			Vector3 localScale = spriteRenderer.transform.localScale;
			spriteRenderer.transform.localScale = Vector3.one;

			//Create triangle.NET geometry
			TriangleNet.Geometry.InputGeometry geometry = new TriangleNet.Geometry.InputGeometry(points.Length);

			//Add vertices
			foreach (Vector2 point in points) {
				geometry.AddPoint(point.x,point.y);
			}

			int N = geometry.Count;
			int end = 0;
			//Add vertices
			foreach (Vector2 point in points) {
				geometry.AddPoint(point.x,point.y);
				end++;
			}

			for (int i = 0; i < end; i++) {
				geometry.AddSegment(N + i, N + ((i + 1) % end));
			}

			//Triangulate and subdivide the mesh
			TriangleNet.Mesh triangleMesh = new TriangleNet.Mesh();

			if (divisions > 0) {
				triangleMesh.behavior.MinAngle = 10;
			}

			triangleMesh.Triangulate(geometry);
			if (divisions > 0) {
				if (divisions > 1) triangleMesh.Refine(true);
				TriangleNet.Tools.Statistic stat = new TriangleNet.Tools.Statistic();
				stat.Update(triangleMesh, 1);
				// Refine by area
				if (divisions > 2) triangleMesh.Refine (stat.LargestArea / 8);

				try {
					triangleMesh.Smooth();
				} catch {
					//Debug.Log("Cannot subdivide");
				}
				triangleMesh.Renumber();
			}

			//transform vertices
			points = new Vector2[triangleMesh.Vertices.Count];
			Vector3[] vertices = new Vector3[triangleMesh.Vertices.Count];
			Vector3[] normals = new Vector3[triangleMesh.Vertices.Count];

			int n = 0;
			foreach(TriangleNet.Data.Vertex v in triangleMesh.Vertices) {

				points[n] = new Vector2((float)v.X, (float)v.Y);
				vertices[n] = new Vector3((float)v.X, (float)v.Y, 0);
				normals[n] = new Vector3(0, 0, -1);
				n++;
			}

			//transform triangles
			int[] triangles = new int[triangleMesh.Triangles.Count*3];
			n = 0;
			foreach (TriangleNet.Data.Triangle t in triangleMesh.Triangles) {
				triangles[n++] = t.P1;
				triangles[n++] = t.P0;
				triangles[n++] = t.P2;
			}

			mesh.Clear();
			mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.uv = genUV(mesh.vertices);
			mesh.normals = normals;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			// Reset the rotations of the object
			spriteRenderer.transform.localRotation = localRotation;
			spriteRenderer.transform.localScale = localScale;
			spriteRenderer.transform.parent = polygonParent;

			meshCreated = true;
		}
    }

	public Mesh CreateMeshFromPoints(bool combine) {
        if (spriteRenderer != null && points.Length > 2) {

			int pointNum = points.Length;
			if (combine && combineMesh != null) {
				pointNum = points.Length + combineMesh.vertices.Length;
			}

			//Create triangle.NET geometry
			TriangleNet.Geometry.InputGeometry geometry = new TriangleNet.Geometry.InputGeometry(pointNum);


            geometry.AddPolygon(points);


			if (combine && combineMesh != null) {
                geometry.AddPolygon(combineMesh.vertices.Select(x => (Vector2)x).ToArray());
			}

			//Triangulate
			TriangleNet.Mesh triangleMesh = new TriangleNet.Mesh();
		   
			triangleMesh.Triangulate(geometry);

			//transform vertices
			points = new Vector2[triangleMesh.Vertices.Count];
			Vector3[] vertices = new Vector3[triangleMesh.Vertices.Count];
			Vector3[] normals = new Vector3[triangleMesh.Vertices.Count];

			int n = 0;
			foreach(TriangleNet.Data.Vertex v in triangleMesh.Vertices) 
			{

				points[n] = new Vector2((float)v.X, (float)v.Y);
				vertices[n] = new Vector3((float)v.X, (float)v.Y, 0);
				normals[n]=new Vector3(0,0,-1);

				n++;
			}

			//transform triangles
            int[] triangles = triangleMesh.Triangles.ToUnityMeshTriangleIndices();

			mesh.Clear();
			mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.uv = genUV(mesh.vertices);
			mesh.normals = normals;

			return mesh;
		}
		else {
			return null;
		}
	}

}
