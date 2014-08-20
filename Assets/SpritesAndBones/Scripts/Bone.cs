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
#endif
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

[ExecuteInEditMode]
public class Bone : MonoBehaviour {
    public int index = 0;
    public float length = 1.0f;
    public bool snapToParent = true;
    public bool editMode = true;
    public bool showInfluence = true;
    public bool deform = false;
    public float influenceTail = 0.25f;
    public float influenceHead = 0.25f;
    public float zOrder = 0;
    public Color color = Color.cyan;

    private Bone parent;

    public Vector2 Head {
        get {
            Vector3 v = gameObject.transform.up * length;
            v.Scale(gameObject.transform.lossyScale);
            return gameObject.transform.position + v;
        }
    }

#if UNITY_EDITOR
    [MenuItem("Sprites And Bones/Bone")]
    public static Bone Create() {
        GameObject b = new GameObject("Bone");
        Undo.RegisterCreatedObjectUndo(b, "Add child bone");
        b.AddComponent<Bone>();

        if (Selection.activeGameObject != null) {
            GameObject sel = Selection.activeGameObject;
            b.transform.parent = sel.transform;

            if (sel.GetComponent<Bone>() != null) {
                Bone p = sel.GetComponent<Bone>();
                b.transform.position = p.Head;
                b.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        Skeleton skel = b.transform.root.GetComponentInChildren<Skeleton>();

        if (skel != null) {
            Bone[] bones = skel.GetComponentsInChildren<Bone>();
            int index = bones.Max(bn => bn.index) + 1;
            b.GetComponent<Bone>().index = index;
            skel.CalculateWeights(true);
        }

        Selection.activeGameObject = b;

        return b.GetComponent<Bone>();
    }

    public static void Split() {
        if (Selection.activeGameObject != null) {
            Undo.IncrementCurrentGroup();
            string undo = "Split bone";

            GameObject old = Selection.activeGameObject;
            Undo.RecordObject(old, undo);
            Bone b = old.GetComponent<Bone>();

            GameObject n1 = new GameObject(old.name + "1");
            Undo.RegisterCreatedObjectUndo(n1, undo);
            Bone b1 = n1.AddComponent<Bone>();
            b1.index = b.index;
            b1.transform.parent = b.transform.parent;
            b1.snapToParent = b.snapToParent;
            b1.length = b.length / 2;
            b1.transform.localPosition = b.transform.localPosition;
            b1.transform.localRotation = b.transform.localRotation;

            GameObject n2 = new GameObject(old.name + "2");
            Undo.RegisterCreatedObjectUndo(n2, undo);
            Bone b2 = n2.AddComponent<Bone>();
            b2.index = b.GetMaxIndex();
            b2.length = b.length / 2;
            n2.transform.parent = n1.transform;
            b2.transform.localRotation = Quaternion.Euler(0, 0, 0);
            n2.transform.position = b1.Head;

            var children = (from Transform child in b.transform select child).ToArray<Transform>();
            b.transform.DetachChildren();
            foreach (Transform child in children) {
                Undo.SetTransformParent(child, n2.transform, undo);
                child.parent = n2.transform;
            }

            Undo.DestroyObjectImmediate(old);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            Selection.activeGameObject = b2.gameObject;
        }
    }

    public void AddIK() {
        Undo.AddComponent<InverseKinematics>(gameObject);
    }
#endif

    // Use this for initialization
    void Start() {
        if (gameObject.transform.parent != null)
            parent = gameObject.transform.parent.GetComponent<Bone>();
    }

    // Update is called once per frame
    void Update() {
        transform.localRotation = Quaternion.Euler(0, 0, transform.localRotation.eulerAngles.z);

#if UNITY_EDITOR
        if (Application.isEditor && editMode && snapToParent && parent != null) {
            gameObject.transform.position = parent.Head;
        }
#endif
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (gameObject.Equals(Selection.activeGameObject)) {
            Gizmos.color = Color.yellow;
        }
        else {
            Color c = this.color;
            if (gameObject.name.ToUpper().EndsWith(".R") || gameObject.name.ToUpper().EndsWith("RIGHT")) {
                c = Utils.ColorFromInt(EditorPrefs.GetInt("BoneRightColor", Color.red.AsInt()));
            }
            else if (gameObject.name.ToUpper().EndsWith(".L") || gameObject.name.ToUpper().EndsWith("LEFT")) {
                c = Utils.ColorFromInt(EditorPrefs.GetInt("BoneLeftColor", Color.green.AsInt()));
            }
            c.a = 1;

            if (editMode) {
                Gizmos.color = new Color(c.r * 0.75f, c.g * 0.75f, c.b * 0.75f, c.a);
            }
            else {
                Gizmos.color = c;
            }
        }

        int div = 5; 

		Vector3 v = Quaternion.AngleAxis(45, Vector3.forward) * (((Vector3)Head - gameObject.transform.position) / div);
		Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + v);
		Gizmos.DrawLine(gameObject.transform.position + v, Head);

		v = Quaternion.AngleAxis(-45, Vector3.forward) * (((Vector3)Head - gameObject.transform.position) / div);
		Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + v);
		Gizmos.DrawLine(gameObject.transform.position + v, Head);

		Gizmos.DrawLine(gameObject.transform.position, Head);

		Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);

		if (deform && editMode && showInfluence) {
			Gizmos.DrawWireSphere(transform.position, influenceTail);
			Gizmos.DrawWireSphere(Head, influenceHead);
		}
    }
#endif

    public float GetInfluence(Vector2 p) {
        Vector2 wv = Head - (Vector2)transform.position;

        float dist = 0;

        if (length == 0) {
            dist = (p - (Vector2)transform.position).magnitude;
            if (dist > influenceTail)
                return 0;
            else
                return influenceTail;
        } else {
            float t = Vector2.Dot(p - (Vector2)transform.position, wv) / wv.sqrMagnitude;

            if (t < 0) {
                dist = (p - (Vector2)transform.position).magnitude;
                if (dist > influenceTail)
                    return 0;
                else
                    return (influenceTail - dist) / influenceTail;
            } else if (t > 1.0f) {
                dist = (p - Head).magnitude;
                if (dist > influenceHead)
                    return 0;
                else
                    return (influenceHead - dist) / influenceHead;
            } else {
                Vector2 proj = (Vector2)transform.position + (wv * t);
                dist = (proj - p).magnitude;

                float s = (influenceHead - influenceTail);
                float i = influenceTail + s * t;

                if (dist > i)
                    return 0;
                else
                    return (i - dist) / i;
            }
        }
    }

    internal int GetMaxIndex() {
        Bone[] bones = transform.root.GetComponentsInChildren<Bone>();

        if (bones == null || bones.Length == 0)
            return 0;

        return bones.Max(b => b.index) + 1;
    }
}
