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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

[ExecuteInEditMode]
public class Bone : MonoBehaviour {
    public int index = 0;
    public float length = 1.0f;
    public bool snapToParent = true;
    public bool editMode = true;
	[HideInInspector]
    public bool showInfluence = true;
    public bool deform = false;
    public float influenceTail = 0f;
    public float influenceHead = 0f;
    public float zOrder = 0;
	public Color color = Color.cyan;

	[SerializeField] 
	[HideInInspector]
	private bool _flipY = false;

	public bool flipY
	{
		get { return _flipY; }
		set
		{
			_flipY = value;
			FlipY();
		}
	}

	[SerializeField] 
	[HideInInspector]
	private bool _flipX = false;
	public bool flipX
	{
		get { return _flipX; }
		set
		{
			_flipX = value;
			FlipX();
		}
	}

    private Bone parent;

	private Skeleton skeleton;

	private Dictionary<Transform, Vector3> childPositions = new Dictionary<Transform, Vector3>();
	private Dictionary<Transform, Quaternion> childRotations = new Dictionary<Transform, Quaternion>();

	private Dictionary<Transform, float> renderers = new Dictionary<Transform, float>();

	private SkinnedMeshRenderer[] skins;
	private SpriteRenderer[] spriteRenderers;

    public Vector2 Head {
        get {
            Vector3 v = Vector3.up * length;
			v = transform.TransformPoint(v);
			return v;
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
			skel.CalculateWeights();
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

	public void SaveChildPosRot() {
		if (Application.isEditor && editMode) {
			Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
			foreach (Transform child in children){
				if (!child.GetComponent<Bone>()){
					childPositions[child] = new Vector3(child.position.x, child.position.y, child.position.z);
					childRotations[child] = new Quaternion(child.rotation.x, child.rotation.y, child.rotation.z, child.rotation.w);
				}
			}
		}
		else
		{
			Debug.Log("Skeleton needs to be in Edit Mode");
		}
	}

	public void LoadChildPosRot() {
		if (Application.isEditor && editMode) {
			Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
			foreach (Transform child in children){
				if (childPositions.ContainsKey(child)){
					child.position = childPositions[child];
					child.rotation = childRotations[child];
				}
			}
		}
		else
		{
			Debug.Log("Skeleton needs to be in Edit Mode");
		}
	}

	public bool HasChildPositionsSaved(){
		return (childPositions.Count > 0 && childRotations.Count > 0);
	}
    #endif

    // Use this for initialization
	void Start () {
        if (gameObject.transform.parent != null)
            parent = gameObject.transform.parent.GetComponent<Bone>();
	}
	
	// Update is called once per frame
	void Update () {
		if (gameObject.transform.parent != null && parent == null || 
			gameObject.transform.parent != null && parent != gameObject.transform.parent)
            parent = gameObject.transform.parent.GetComponent<Bone>();

		if (skeleton == null || skeleton != null && !Application.isPlaying && !transform.IsChildOf(skeleton.transform))
		{
			Skeleton[] skeletons = transform.root.GetComponentsInChildren<Skeleton>(true);
			foreach (Skeleton s in skeletons)
			{
				if (transform.IsChildOf(s.transform))
				{
					skeleton = s;
				}
			}
		}

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

			if (editMode) {
				if (skeleton != null)
				{
					if (gameObject.name.ToUpper().EndsWith(" R") || 
						gameObject.name.ToUpper().EndsWith("_R") || 
						gameObject.name.ToUpper().EndsWith(".R") || 
						gameObject.name.ToUpper().EndsWith("RIGHT"))
						{
							Gizmos.color = new Color(skeleton.colorRight.r * 0.75f, skeleton.colorRight.g * 0.75f, skeleton.colorRight.b * 0.75f, skeleton.colorRight.a);
						}
					else if (gameObject.name.ToUpper().EndsWith(" L") || 
						gameObject.name.ToUpper().EndsWith("_L") || 
						gameObject.name.ToUpper().EndsWith(".L") || 
						gameObject.name.ToUpper().EndsWith("LEFT"))
						{
							Gizmos.color = new Color(skeleton.colorLeft.r * 0.75f, skeleton.colorLeft.g * 0.75f, skeleton.colorLeft.b * 0.75f, skeleton.colorLeft.a);
						}
					else
					{
						Gizmos.color = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a);
					}
				}
				else
				{
					Gizmos.color = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a);
				}
            }
            else {
				if (skeleton != null)
				{
					if (gameObject.name.ToUpper().EndsWith(" R") || 
						gameObject.name.ToUpper().EndsWith("_R") || 
						gameObject.name.ToUpper().EndsWith(".R") || 
						gameObject.name.ToUpper().EndsWith("RIGHT"))
						{
							Gizmos.color = skeleton.colorRight;
						}
					else if (gameObject.name.ToUpper().EndsWith(" L") || 
						gameObject.name.ToUpper().EndsWith("_L") || 
						gameObject.name.ToUpper().EndsWith(".L") || 
						gameObject.name.ToUpper().EndsWith("LEFT"))
						{
							Gizmos.color = skeleton.colorLeft;
						}
					else
					{
						Gizmos.color = color;
					}
				}
				else
				{
					Gizmos.color = color;
				}
            }
        }

		int div = 5;

        Vector3 HeadProjected = new Vector3(Head.x, Head.y, gameObject.transform.position.z);
		Vector3 v = Quaternion.AngleAxis(45, Vector3.forward) * ((Head - (Vector2)gameObject.transform.position) / div);
		Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + v);
        Gizmos.DrawLine(gameObject.transform.position + v, HeadProjected);

		v = Quaternion.AngleAxis(-45, Vector3.forward) * ((Head - (Vector2)gameObject.transform.position) / div);
		Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + v);
        Gizmos.DrawLine(gameObject.transform.position + v, HeadProjected);

        Gizmos.DrawLine(gameObject.transform.position, HeadProjected);

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
        }
        else {
            float t = Vector2.Dot(p - (Vector2)transform.position, wv) / wv.sqrMagnitude;

            if (t < 0) {
                dist = (p - (Vector2)transform.position).magnitude;
                if (dist > influenceTail)
                    return 0;
                else
                    return (influenceTail - dist) / influenceTail;
            }
            else if (t > 1.0f) {
                dist = (p - Head).magnitude;
                if (dist > influenceHead)
                    return 0;
                else
                    return (influenceHead - dist) / influenceHead;
            }
            else {
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
        Bone[] bones = transform.root.GetComponentsInChildren<Bone>(true);

        if (bones == null || bones.Length == 0)
            return 0;

        return bones.Max(b => b.index) + 1;
    }

	private void MoveRenderersPositions(){
		foreach (Transform renderer in renderers.Keys){
			#if UNITY_EDITOR
			Undo.RecordObject(renderer, "Move Render Position");
			#endif
			renderer.position = new Vector3(renderer.position.x, renderer.position.y, (float)renderers[renderer]);
			#if UNITY_EDITOR
			EditorUtility.SetDirty (renderer);
			#endif
		}
	}

	public void FlipY ()
	{
		int normal = -1;
		// Rotate the skeleton's local transform
		if (!flipY)
		{
			renderers = new Dictionary<Transform, float>();
			// Get the new positions for the renderers from the rotation of this transform
			if (skeleton != null && skeleton.useShadows)
			{
				renderers = GetRenderersZ();
			}
			#if UNITY_EDITOR
			Undo.RecordObject(transform, "Flip Y");
			#endif
			transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0.0f, transform.localEulerAngles.z);
			#if UNITY_EDITOR
			EditorUtility.SetDirty (transform);
			#endif
			if (skeleton != null && skeleton.useShadows)
			{
				MoveRenderersPositions();
			}
		}
		else
		{
			renderers = new Dictionary<Transform, float>();
			// Get the new positions for the renderers from the rotation of this transform
			if (skeleton != null && skeleton.useShadows)
			{
				renderers = GetRenderersZ();
			}
			// Get the new positions for the renderers from the rotation of this transform
			#if UNITY_EDITOR
			Undo.RecordObject(transform, "Flip Y");
			#endif
			transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 180.0f, transform.localEulerAngles.z);
			#if UNITY_EDITOR
			EditorUtility.SetDirty (transform);
			#endif
			if (skeleton != null && skeleton.useShadows)
			{
				MoveRenderersPositions();
			}
		}

		if ((int)transform.localEulerAngles.x == 0 && (int)transform.localEulerAngles.y == 180 || (int)transform.localEulerAngles.x == 180 && (int)transform.localEulerAngles.y == 0)
		{
			normal = 1;
		}

		if (skeleton != null && skeleton.useShadows)
		{
			ChangeRendererNormals(normal);
		}
	}

	public void FlipX ()
	{
		int normal = -1;

		// Rotate the skeleton's local transform
		if (!flipX)
		{
			renderers = new Dictionary<Transform, float>();
			// Get the new positions for the renderers from the rotation of this transform
			if (skeleton != null && skeleton.useShadows)
			{
				renderers = GetRenderersZ();
			}
			#if UNITY_EDITOR
			Undo.RecordObject(transform, "Flip X");
			#endif
			transform.localEulerAngles = new Vector3(0.0f, transform.localEulerAngles.y, transform.localEulerAngles.z);
			#if UNITY_EDITOR
			EditorUtility.SetDirty (transform);
			#endif
			if (skeleton != null && skeleton.useShadows)
			{
				MoveRenderersPositions();
			}
		}
		else
		{
			renderers = new Dictionary<Transform, float>();
			// Get the new positions for the renderers from the rotation of this transform
			if (skeleton != null && skeleton.useShadows)
			{
				renderers = GetRenderersZ();
			}
			#if UNITY_EDITOR
			Undo.RecordObject(transform, "Flip X");
			#endif
			transform.localEulerAngles = new Vector3(180.0f, transform.localEulerAngles.y, transform.localEulerAngles.z);
			#if UNITY_EDITOR
			EditorUtility.SetDirty (transform);
			#endif
			if (skeleton != null && skeleton.useShadows)
			{
				MoveRenderersPositions();
			}
		}

		if ((int)transform.localEulerAngles.x == 0 && (int)transform.localEulerAngles.y == 180 || (int)transform.localEulerAngles.x == 180 && (int)transform.localEulerAngles.y == 0)
		{
			normal = 1;
		}

		if (skeleton != null)
		{
			ChangeRendererNormals(normal);
		}
	}

	public Dictionary<Transform, float> GetRenderersZ()
	{
		renderers = new Dictionary<Transform, float>();
		if (skeleton != null)
		{
			//find all SkinnedMeshRenderer elements
			skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			for( int i = 0; i < skins.Length; i++) {
				if (skeleton.spriteShadowsShader != null && skins[i].material.shader == skeleton.spriteShadowsShader)
				{
					renderers[skins[i].transform] = skins[i].transform.position.z;
				}
			}

			//find all SpriteRenderer elements
			spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
			for( int j = 0; j < spriteRenderers.Length; j++) {
				if (skeleton.spriteShadowsShader != null && spriteRenderers[j].material.shader == skeleton.spriteShadowsShader)
				{
					renderers[spriteRenderers[j].transform] = spriteRenderers[j].transform.position.z;
				}
			}
		}
		return renderers;
	}

	public void ChangeRendererNormals(int normal)
	{
		if (skeleton != null)
		{
			//find all SkinnedMeshRenderer elements
			skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			for( int i = 0; i < skins.Length; i++) {
				if (skeleton.spriteShadowsShader != null && skins[i].material.shader == skeleton.spriteShadowsShader)
				{
					if (!skeleton.useSharedMaterial) {
						#if UNITY_EDITOR
						Undo.RecordObject(skins[i].material, "Change Render Normals");
						#endif
						skins[i].material.SetVector("_Normal", new Vector3(0, 0, normal));
						#if UNITY_EDITOR
						EditorUtility.SetDirty (skins[i].material);
						#endif
					} else {
						#if UNITY_EDITOR
						Undo.RecordObject(skins[i].sharedMaterial, "Change Render Normals");
						#endif
						skins[i].sharedMaterial.SetVector("_Normal", new Vector3(0, 0, normal));
						#if UNITY_EDITOR
						EditorUtility.SetDirty (skins[i].sharedMaterial);
						#endif
					}
				}
			}

			//find all SpriteRenderer elements
			spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
			for( int j = 0; j < spriteRenderers.Length; j++) {
				if (skeleton.spriteShadowsShader != null && spriteRenderers[j].material.shader == skeleton.spriteShadowsShader)
				{
					if (!skeleton.useSharedMaterial) {
						#if UNITY_EDITOR
						Undo.RecordObject(spriteRenderers[j].material, "Change Render Normals");
						#endif
						spriteRenderers[j].material.SetVector("_Normal", new Vector3(0, 0, normal));
						#if UNITY_EDITOR
						EditorUtility.SetDirty (spriteRenderers[j].material);
						#endif
					} else {
						#if UNITY_EDITOR
						Undo.RecordObject(spriteRenderers[j].sharedMaterial, "Change Render Normals");
						#endif
						spriteRenderers[j].sharedMaterial.SetVector("_Normal", new Vector3(0, 0, normal));
						#if UNITY_EDITOR
						EditorUtility.SetDirty (spriteRenderers[j].sharedMaterial);
						#endif
					}
				}
			}
		}
	}
}
