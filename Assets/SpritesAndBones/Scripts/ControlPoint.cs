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
#endif
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

[ExecuteInEditMode]
public class ControlPoint : MonoBehaviour {
	[SerializeField] public Color color = Color.red;
	[SerializeField] public float size = 0.01f;
	public bool getMeshSnapShot = false;

	[HideInInspector]
    public Vector3 originalPosition;
	[HideInInspector]
    public int index;
	[HideInInspector]
    public SkinnedMeshRenderer skin;

	private Skeleton skeleton;

    #if UNITY_EDITOR
    public static void CreateControlPoints(SkinnedMeshRenderer skin) {
        if (skin.sharedMesh != null)
		{
			Skin2D skin2D = skin.GetComponent<Skin2D>();
			if (skin2D != null) {
				skin2D.controlPoints = new ControlPoints.Point[skin.sharedMesh.vertices.Length];
				if (skin2D.points == null) {
					skin2D.points = skin2D.gameObject.AddComponent<ControlPoints>();
				}
			}
			for (int i = 0; i < skin.sharedMesh.vertices.Length; i++)
			{
				Vector3 originalPos = skin.sharedMesh.vertices[i];

				if (skin2D != null) {
					skin2D.controlPoints[i] = new ControlPoints.Point(originalPos);
					skin2D.controlPoints[i].index = i;
					skin2D.points.SetPoint(skin2D.controlPoints[i]);
				}

				GameObject b = new GameObject(skin.name + " Control Point");
				// Unparent the skin temporarily before adding the control point
				Transform skinParent = skin.transform.parent;
				skin.transform.parent = null;

				// Reset the rotation before creating the mesh so the UV's will align properly
				Quaternion localRotation = skin.transform.localRotation;
				skin.transform.localRotation = Quaternion.identity;

				b.transform.position = new Vector3(skin.transform.position.x + (skin.sharedMesh.vertices[i].x * skin.transform.localScale.x), skin.transform.position.y + (skin.sharedMesh.vertices[i].y * skin.transform.localScale.y), skin.transform.position.z + (skin.sharedMesh.vertices[i].z * skin.transform.localScale.z));
				b.transform.parent = skin.transform;
				ControlPoint[] points = b.transform.parent.transform.GetComponentsInChildren<ControlPoint>();
				if (points != null && points.Length > 0)
				{
					b.gameObject.name = b.gameObject.name + points.Length;
				}
				Undo.RegisterCreatedObjectUndo(b, "Add control point");
				ControlPoint controlPoint = b.AddComponent<ControlPoint>();
				controlPoint.index = i;
				controlPoint.skin = skin;
				controlPoint.originalPosition = b.transform.localPosition;

				// Reset the rotations of the object
				skin.transform.localRotation = localRotation;
				skin.transform.parent = skinParent;
			}
		}
    }
    #endif

    // Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (skin != null && skin.sharedMesh != null)
		{

			if (skin.sharedMesh.vertices[index] != transform.localPosition)
			{
				Vector3[] vertices = new Vector3[skin.sharedMesh.vertices.Length];
				System.Array.Copy(skin.sharedMesh.vertices, vertices, skin.sharedMesh.vertices.Length);
				vertices[index] = transform.localPosition;
				skin.sharedMesh.vertices = vertices;
				skin.sharedMesh.RecalculateBounds();
			}
		}
	}

#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (Selection.activeGameObject != null)
		{
			if (gameObject.Equals(Selection.activeGameObject) 
				|| gameObject.transform.parent.gameObject.Equals(Selection.activeGameObject)
				|| gameObject.transform.parent == Selection.activeGameObject.transform.parent)
			{
				if (gameObject.Equals(Selection.activeGameObject)) {
					Gizmos.color = Color.green;
				}
				else {
					Gizmos.color = color;
				}
				Gizmos.DrawSphere(gameObject.transform.position, size);
			}
		}
		if (getMeshSnapShot)
		{
			GetMeshSnapshot();
		}
    }

	public void GetMeshSnapshot()
	{

		if (transform.parent.GetComponent<SkinnedMeshRenderer>() != null)
		{
			skin = transform.parent.GetComponent<SkinnedMeshRenderer>();
			Mesh mesh = new Mesh();
			skin.BakeMesh(mesh);
			foreach (Vector3 v in mesh.vertices)
			{
				Gizmos.DrawSphere(transform.parent.TransformPoint(v), size);
			}
		}
		getMeshSnapShot = false;
	}

#endif

	public void Rename() {
		if (skin != null && !gameObject.name.Contains(skin.name)) {
			gameObject.name = skin.name + " " + gameObject.name;
		}
	}

	public void ResetPosition() {
		#if UNITY_EDITOR
		Undo.RegisterCompleteObjectUndo(transform, "Reset Control Point");
		Undo.RecordObject(transform, "Reset Control Point");
		#endif
		transform.localPosition = originalPosition;
		#if UNITY_EDITOR
		EditorUtility.SetDirty (transform);
		#endif
	}
}
