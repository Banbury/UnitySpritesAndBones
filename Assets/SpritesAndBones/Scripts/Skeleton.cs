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
using System.Reflection;
using System.Linq;

[ExecuteInEditMode]
[SelectionBase]
public class Skeleton : MonoBehaviour {
    public bool editMode = true;
    public bool showBoneInfluence = true;
    public bool IK_Enabled = true;

    //[HideInInspector]
    public Pose basePose;

    private Pose tempPose;

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

	[SerializeField] 
	[HideInInspector]
	private bool _useShadows = false;

	public bool useShadows
	{
		get { return _useShadows; }
		set
		{
			_useShadows = value;
			UseShadows();
		}
	}

	[SerializeField] 
	[HideInInspector]
	private bool _useZSorting = false;

	public bool useZSorting
	{
		get { return _useZSorting; }
		set
		{
			_useZSorting = value;
			UseZSorting();
		}
	}

	public Shader spriteShader;
	public Shader spriteShadowsShader;
	public Color colorRight = new Color(255.0f/255.0f, 128.0f/255.0f, 0f, 255.0f/255.0f);
	public Color colorLeft = Color.magenta;

	[HideInInspector]
	public bool hasChildPositionsSaved = false;

	private Bone[] bones;
	private Dictionary<Transform, float> renderers = new Dictionary<Transform, float>();

	private SkinnedMeshRenderer[] skins;
	private SpriteRenderer[] spriteRenderers;

	public bool useSharedMaterial = false;

#if UNITY_EDITOR
		[MenuItem("Sprites And Bones/Skeleton")]
		public static void Create ()
		{
			Undo.IncrementCurrentGroup ();

			GameObject o = new GameObject ("Skeleton");
			Undo.RegisterCreatedObjectUndo (o, "Create skeleton");
			o.AddComponent<Skeleton> ();

			GameObject b = new GameObject ("Bone");
			Undo.RegisterCreatedObjectUndo (b, "Create Skeleton");
			b.AddComponent<Bone> ();

			b.transform.parent = o.transform;

			Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());
		}
#endif

    // Use this for initialization
	void Start () {
		// Set Default Shaders
		spriteShader = Shader.Find("Sprites/Default");
		spriteShadowsShader = Shader.Find("Sprites/Skeleton-CutOut");

		// Initialize Z-Sorting and Shadows
		skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		foreach (SkinnedMeshRenderer skin in skins)
		{
			if (skin.transform.localPosition.z != 0) {
				_useZSorting = true;
			}
			if (skin.receiveShadows) {
				_useShadows = true;
			}
		}

		spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
		foreach (SpriteRenderer spriteRenderer in spriteRenderers)
		{
			if (spriteRenderer.transform.localPosition.z != 0) {
				_useZSorting = true;
			}
			if (spriteRenderer.receiveShadows) {
				_useShadows = true;
			}
		}

		// Turn Edit mode off when playing
		if (Application.isPlaying) {
            SetEditMode(false);
        }
	}

#if UNITY_EDITOR
    void OnEnable() {
		EditorApplication.update += EditorUpdate;
    }

    void OnDisable() {
		EditorApplication.update -= EditorUpdate;
    }
#endif

    private void EditorUpdate() {
		if (IK_Enabled) {
			if (bones != null)
			{
				for (int i=0; i<bones.Length; i++) {
					if (bones[i] != null)
					{
						InverseKinematics ik = bones[i].GetComponent<InverseKinematics>();

						if (ik != null && !editMode && ik.enabled && ik.influence > 0 && ik.gameObject.activeInHierarchy) {
							ik.resolveSK2D();
						}
					}
				}
			}
		}
    }
	
	// Update is called once per frame
	void Update () {
		// Get Shaders if they are null
		if (spriteShader == null)
		{
			spriteShader = Shader.Find("Sprites/Default");
		}
		if (spriteShadowsShader == null)
		{
			spriteShadowsShader = Shader.Find("Sprites/Skeleton-CutOut");
		}

		if (bones == null || bones != null && bones.Length <= 0 || Application.isEditor){
			bones = gameObject.GetComponentsInChildren<Bone>(true);
		}

		#if UNITY_EDITOR
			EditorUpdate();
		#endif
	}

	void LateUpdate () {

	#if !UNITY_EDITOR
		EditorUpdate();
	#endif
        if (Application.isEditor && bones != null) {
            for (int i=0; i<bones.Length; i++) {
                if (bones[i] != null)
				{
					bones[i].editMode = editMode;
					bones[i].showInfluence = showBoneInfluence;
				}
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "man_icon.png", true);
    }

    public Pose CreatePose(bool includeDisabled) {
        Pose pose = ScriptableObject.CreateInstance<Pose>();

        var bones = GetComponentsInChildren<Bone>(includeDisabled);
        var cps = GetComponentsInChildren<ControlPoint>(includeDisabled);

        List<RotationValue> rotations = new List<RotationValue>();
        List<PositionValue> positions = new List<PositionValue>();
        List<PositionValue> targets = new List<PositionValue>();
        List<PositionValue> controlPoints = new List<PositionValue>();

        foreach (Bone b in bones) {
            rotations.Add(new RotationValue(b.name, b.transform.localRotation));
            positions.Add(new PositionValue(b.name, b.transform.localPosition));

            if (b.GetComponent<InverseKinematics>() != null) {
                targets.Add(new PositionValue(b.name, b.GetComponent<InverseKinematics>().target.localPosition));
            }
        }

        // Use bone parent name + control point name for the search
        foreach (ControlPoint cp in cps) {
            controlPoints.Add(new PositionValue(cp.name, cp.transform.localPosition));
        }

        pose.rotations = rotations.ToArray();
        pose.positions = positions.ToArray();
        pose.targets = targets.ToArray();
        pose.controlPoints = controlPoints.ToArray();

        return pose;
    }

    public Pose CreatePose() {
        return CreatePose(true);
    }

    public void SavePose(string poseFileName) {
		if(!Directory.Exists("Assets/Poses")) {
			AssetDatabase.CreateFolder("Assets", "Poses");
			AssetDatabase.Refresh();
		}
		if (poseFileName != null && poseFileName.Trim() != "") {
            ScriptableObjectUtility.CreateAsset(CreatePose(), "Poses/" + poseFileName);
        } else {
            ScriptableObjectUtility.CreateAsset(CreatePose());
        }
    }
#endif

    public void RestorePose(Pose pose) {
        var bones = GetComponentsInChildren<Bone>();
        var cps = GetComponentsInChildren<ControlPoint>();
		#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(bones, "Assign Pose");
        Undo.RegisterCompleteObjectUndo(cps, "Assign Pose");
		#endif

        if (bones.Length > 0)
		{
			foreach (RotationValue rv in pose.rotations) {
				Bone bone = bones.FirstOrDefault(b => b.name == rv.name);
				if (bone != null) {
					#if UNITY_EDITOR
					Undo.RecordObject(bone.transform, "Assign Pose");
					#endif
					bone.transform.localRotation = rv.rotation;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (bone.transform);
					#endif
				} else {
					Debug.Log("This skeleton has no bone '" + rv.name + "'");
				}
			}

			foreach (PositionValue pv in pose.positions) {
				Bone bone = bones.FirstOrDefault(b => b.name == pv.name);
				if (bone != null) {
					#if UNITY_EDITOR
					Undo.RecordObject(bone.transform, "Assign Pose");
					#endif
					bone.transform.localPosition = pv.position;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (bone.transform);
					#endif
					
				} else {
					Debug.Log("This skeleton has no bone '" + pv.name + "'");
				}
			}

			foreach (PositionValue tv in pose.targets) {
				Bone bone = bones.FirstOrDefault(b => b.name == tv.name);

				if (bone != null) {
					InverseKinematics ik = bone.GetComponent<InverseKinematics>();

					if (ik != null) {
						#if UNITY_EDITOR
						Undo.RecordObject(ik.target, "Assign Pose");
						#endif
						ik.target.transform.localPosition = tv.position;
						#if UNITY_EDITOR
						EditorUtility.SetDirty (ik.target.transform);
						#endif
					}
				} else {
					Debug.Log("This skeleton has no bone '" + tv.name + "'");
				}
			}
		}

        if (cps.Length > 0)
		{
			foreach (PositionValue cpv in pose.controlPoints) {
				ControlPoint cp = cps.FirstOrDefault(c => (c.name) == cpv.name || (c.name) == c.transform.parent.name + c.name);

				if (cp != null) {
					#if UNITY_EDITOR
					Undo.RecordObject(cp.transform, "Assign Pose");
					#endif
					cp.transform.localPosition = cpv.position;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (cp.transform);
					#endif
				}
				else {
					Debug.Log("There is no control point '" + cpv.name + "'");
				}
			}
		}
    }

    public void SetBasePose(Pose pose) {
        basePose = pose;
    }

    public void SetEditMode(bool edit) {
#if UNITY_EDITOR
        if (!editMode && edit) {
            AnimationMode.StopAnimationMode();

            tempPose = CreatePose();
            tempPose.hideFlags = HideFlags.HideAndDontSave;

            if (basePose != null) {
                RestorePose(basePose);
            }
        }
        else if (editMode && !edit) {
            if (tempPose != null) {
                RestorePose(tempPose);
                Object.DestroyImmediate(tempPose);
            }
        }
#endif

        editMode = edit;
    }

#if UNITY_EDITOR
	public void CalculateWeights ()
	{
		CalculateWeights(false);
	}

	public void CalculateWeights (bool weightToParent)
	{
		if(bones == null || bones.Length == 0) {
			Debug.Log("No bones in skeleton");
			return;
		}
		//find all Skin2D elements
		Skin2D[] skins = transform.GetComponentsInChildren<Skin2D>(true);
		foreach(Skin2D skin in skins) {
			bool skinActive = skin.gameObject.activeSelf;
			skin.gameObject.SetActive(true);
			skin.CalculateBoneWeights(bones, weightToParent);
			skin.gameObject.SetActive(skinActive);
		}
	}
#endif

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
			if (useZSorting)
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
			if (useZSorting)
			{
				MoveRenderersPositions();
			}
		}
		else
		{
			renderers = new Dictionary<Transform, float>();
			// Get the new positions for the renderers from the rotation of this transform
			if (useZSorting)
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
			if (useZSorting)
			{
				MoveRenderersPositions();
			}
		}

		if (transform.localEulerAngles.x == 0.0f && transform.localEulerAngles.y == 180.0f || transform.localEulerAngles.x == 180.0f && transform.localEulerAngles.y == 0.0f)
		{
			normal = 1;
		}

		ChangeRendererNormals(normal);
	}

	public void FlipX ()
	{
		int normal = -1;

		// Rotate the skeleton's local transform
		if (!flipX)
		{
			renderers = new Dictionary<Transform, float>();
			// Get the new positions for the renderers from the rotation of this transform
			if (useZSorting)
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
			if (useZSorting)
			{
				MoveRenderersPositions();
			}
		}
		else
		{
			renderers = new Dictionary<Transform, float>();
			// Get the new positions for the renderers from the rotation of this transform
			if (useZSorting)
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
			if (useZSorting)
			{
				MoveRenderersPositions();
			}
		}

		if (transform.localEulerAngles.x == 0.0f && transform.localEulerAngles.y == 180.0f || transform.localEulerAngles.x == 180.0f && transform.localEulerAngles.y == 0.0f)
		{
			normal = 1;
		}
		
		ChangeRendererNormals(normal);

	}

	public Dictionary<Transform, float> GetRenderersZ()
	{
		renderers = new Dictionary<Transform, float>();
		if (useZSorting)
		{
			//find all SkinnedMeshRenderer elements
			skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			foreach(SkinnedMeshRenderer skin in skins) {
				renderers[skin.transform] = skin.transform.position.z;
			}

			//find all SpriteRenderer elements
			SpriteRenderer[] spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
			foreach(SpriteRenderer spriteRenderer in spriteRenderers) {
				renderers[spriteRenderer.transform] = spriteRenderer.transform.position.z;
			}
		}
		return renderers;
	}

	public void ChangeRendererNormals(int normal)
	{
		if (useShadows)
		{
			//find all SkinnedMeshRenderer elements
			skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			foreach(SkinnedMeshRenderer skin in skins) {
				if (spriteShadowsShader != null && skin.material.shader == spriteShadowsShader)
				{
					if (!useSharedMaterial) {
						#if UNITY_EDITOR
						Undo.RecordObject(skin.material, "Change Render Normals");
						#endif
						skin.material.SetVector("_Normal", new Vector3(0, 0, normal));
						#if UNITY_EDITOR
						EditorUtility.SetDirty (skin.material);
						#endif
					} else {
						#if UNITY_EDITOR
						Undo.RecordObject(skin.sharedMaterial, "Change Render Normals");
						#endif
						skin.sharedMaterial.SetVector("_Normal", new Vector3(0, 0, normal));
						#if UNITY_EDITOR
						EditorUtility.SetDirty (skin.sharedMaterial);
						#endif
					}
				}
			}

			//find all SpriteRenderer elements
			spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
			foreach(SpriteRenderer spriteRenderer in spriteRenderers) {
				if (spriteShadowsShader != null && spriteRenderer.material.shader == spriteShadowsShader)
				{
					if (!useSharedMaterial) {
						#if UNITY_EDITOR
						Undo.RecordObject(spriteRenderer.material, "Change Render Normals");
						#endif
						spriteRenderer.material.SetVector("_Normal", new Vector3(0, 0, normal));
						#if UNITY_EDITOR
						EditorUtility.SetDirty (spriteRenderer.material);
						#endif
					} else {
						#if UNITY_EDITOR
						Undo.RecordObject(spriteRenderer.sharedMaterial, "Change Render Normals");
						#endif
						spriteRenderer.sharedMaterial.SetVector("_Normal", new Vector3(0, 0, normal));
						#if UNITY_EDITOR
						EditorUtility.SetDirty (spriteRenderer.sharedMaterial);
						#endif
					}
				}
			}
		}
	}

	public void UseShadows ()
	{
		//find all SpriteRenderer elements
		skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		
		foreach(SkinnedMeshRenderer skin in skins) {
			if (useShadows && spriteShadowsShader != null)
			{
				if (!useSharedMaterial) {
					#if UNITY_EDITOR
					Undo.RecordObject(skin.material.shader, "Use Shadows");
					#endif
					skin.material.shader = spriteShadowsShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (skin.material.shader);
					#endif
				} else {
					#if UNITY_EDITOR
					Undo.RecordObject(skin.sharedMaterial.shader, "Use Shadows");
					#endif
					skin.sharedMaterial.shader = spriteShadowsShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (skin.sharedMaterial.shader);
					#endif
				}
			}
			else
			{
				if (!useSharedMaterial) {
					#if UNITY_EDITOR
					Undo.RecordObject(skin.material.shader, "Use Shadows");
					#endif
					skin.material.shader = spriteShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (skin.material.shader);
					#endif
				} else {
					#if UNITY_EDITOR
					Undo.RecordObject(skin.sharedMaterial.shader, "Use Shadows");
					#endif
					skin.sharedMaterial.shader = spriteShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (skin.sharedMaterial.shader);
					#endif
				}
			}

			if (useShadows){
				skin.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
			}
			else {
				skin.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
			skin.receiveShadows = useShadows;
		}

		//find all SpriteRenderer elements
		spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
		
		foreach(SpriteRenderer spriteRenderer in spriteRenderers) {
			if (useShadows && spriteShadowsShader != null)
			{
				if (!useSharedMaterial) {
					#if UNITY_EDITOR
					Undo.RecordObject(spriteRenderer.material.shader, "Use Shadows");
					#endif
					spriteRenderer.material.shader = spriteShadowsShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (spriteRenderer.material.shader);
					#endif
				} else {
					#if UNITY_EDITOR
					Undo.RecordObject(spriteRenderer.sharedMaterial.shader, "Use Shadows");
					#endif
					spriteRenderer.sharedMaterial.shader = spriteShadowsShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (spriteRenderer.sharedMaterial.shader);
					#endif
				}
			}
			else
			{
				if (!useSharedMaterial) {
					#if UNITY_EDITOR
					Undo.RecordObject(spriteRenderer.material.shader, "Use Shadows");
					#endif
					spriteRenderer.material.shader = spriteShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (spriteRenderer.material.shader);
					#endif
				} else {
					#if UNITY_EDITOR
					Undo.RecordObject(spriteRenderer.sharedMaterial.shader, "Use Shadows");
					#endif
					spriteRenderer.sharedMaterial.shader = spriteShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (spriteRenderer.sharedMaterial.shader);
					#endif
				}
			}

			if (useShadows){
				spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
			}
			else {
				spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
			spriteRenderer.receiveShadows = useShadows;
		}
	}

	public void UseZSorting ()
	{
		//find all SpriteRenderer elements
		skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		
		foreach(SkinnedMeshRenderer skin in skins) {
			if (useZSorting)
			{
				#if UNITY_EDITOR
				Undo.RecordObject(skin.transform, "Use Z Sorting");
				#endif
				float z = skin.sortingOrder / -10000f;
				skin.transform.localPosition = new Vector3(skin.transform.localPosition.x, skin.transform.localPosition.y, z);
				skin.sortingLayerName = "Default";
				skin.sortingOrder = 0;
				#if UNITY_EDITOR
				EditorUtility.SetDirty (skin.transform);
				#endif
			}
			else
			{
				#if UNITY_EDITOR
				Undo.RecordObject(skin.transform, "Use Z Sorting");
				#endif
				int sortLayer = Mathf.RoundToInt(skin.transform.localPosition.z * -10000);
				skin.transform.localPosition = new Vector3(skin.transform.localPosition.x, skin.transform.localPosition.y, 0);
				skin.sortingLayerName = "Default";
				skin.sortingOrder = sortLayer;
				#if UNITY_EDITOR
				EditorUtility.SetDirty (skin.transform);
				#endif
			}
		}

		//find all SpriteRenderer elements
		spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
		
		foreach(SpriteRenderer spriteRenderer in spriteRenderers) {
			if (useZSorting)
			{
				#if UNITY_EDITOR
				Undo.RecordObject(spriteRenderer.transform, "Use Z Sorting");
				#endif
				float z = spriteRenderer.sortingOrder / -10000f;
				spriteRenderer.transform.localPosition = new Vector3(spriteRenderer.transform.localPosition.x, spriteRenderer.transform.localPosition.y, z);
				spriteRenderer.sortingLayerName = "Default";
				spriteRenderer.sortingOrder = 0;
				#if UNITY_EDITOR
				EditorUtility.SetDirty (spriteRenderer.transform);
				#endif
			}
			else
			{
				#if UNITY_EDITOR
				Undo.RecordObject(spriteRenderer.transform, "Use Z Sorting");
				#endif
				int sortLayer = Mathf.RoundToInt(spriteRenderer.transform.localPosition.z * -10000);
				spriteRenderer.transform.localPosition = new Vector3(spriteRenderer.transform.localPosition.x, spriteRenderer.transform.localPosition.y, 0);
				spriteRenderer.sortingLayerName = "Default";
				spriteRenderer.sortingOrder = sortLayer;
				#if UNITY_EDITOR
				EditorUtility.SetDirty (spriteRenderer.transform);
				#endif
			}
		}
	}
}
