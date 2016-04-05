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
	private InverseKinematics[] iks;
	private Dictionary<Transform, float> renderers = new Dictionary<Transform, float>();

	private SkinnedMeshRenderer[] skins;
	private Skin2D[] skin2Ds;
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
		for( int i = 0; i < skins.Length; i++)
		{
			if (skins[i].transform.localPosition.z != 0) {
				_useZSorting = true;
			}
			if (skins[i].receiveShadows) {
				_useShadows = true;
			}
		}

		spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
		for( int n = 0; n < spriteRenderers.Length; n++)
		{
			if (spriteRenderers[n].transform.localPosition.z != 0) {
				_useZSorting = true;
			}
			if (spriteRenderers[n].receiveShadows) {
				_useShadows = true;
			}
		}

		skin2Ds = transform.GetComponentsInChildren<Skin2D>(true);

		// Turn Edit mode off when playing
		if (Application.isPlaying) {
            SetEditMode(false);

			// Instance the materials for the skeleton on play
			useSharedMaterial = false;
			int normal = -1;
			if ((int)transform.localEulerAngles.x == 0 && (int)transform.localEulerAngles.y == 180 || (int)transform.localEulerAngles.x == 180 && (int)transform.localEulerAngles.y == 0)
			{
				normal = 1;
				// Debug.Log("Changing normals for " + name);
			}

			ChangeRendererNormals(normal);
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
			if (iks != null)
			{
				for (int i=0; i < iks.Length; i++) {
					if (iks[i] != null && !editMode && iks[i].enabled && iks[i].influence > 0 && iks[i].gameObject.activeInHierarchy) {
						iks[i].resolveSK2D();
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
			iks = gameObject.GetComponentsInChildren<InverseKinematics>(true);
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
        var skin2Ds = GetComponentsInChildren<Skin2D>(includeDisabled);

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

        // Use bone parent name + control point name for the search
        foreach (Skin2D skin in skin2Ds) {
			if (skin.controlPoints != null && skin.controlPoints.Length > 0) {
				for (int c = 0; c < skin.controlPoints.Length; c++) {
					string index = "";
					if (c > 0) {
						index = c.ToString();
					}
					controlPoints.Add(new PositionValue(skin.name + " Control Point" + index, skin.points.GetPoint(skin.controlPoints[c])));
				}
			}
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
        var skin2Ds = GetComponentsInChildren<Skin2D>();
		#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(bones, "Assign Pose");
        Undo.RegisterCompleteObjectUndo(cps, "Assign Pose");
        Undo.RegisterCompleteObjectUndo(skin2Ds, "Assign Pose");
		#endif

        if (bones.Length > 0)
		{
			for( int i = 0; i < pose.rotations.Length; i++) {
				bool hasRot = false;
				for( int b = 0; b < bones.Length; b++) {
					if (bones[b].name == pose.rotations[i].name) {
						#if UNITY_EDITOR
						Undo.RecordObject(bones[b].transform, "Assign Pose");
						#endif
						bones[b].transform.localRotation = pose.rotations[i].rotation;
						#if UNITY_EDITOR
						EditorUtility.SetDirty (bones[b].transform);
						#endif
						hasRot = true;
					}
				}
				if (!hasRot) {
					Debug.Log("This skeleton has no bone '" + pose.rotations[i].name + "'");
				}
			}

			for( int j = 0; j < pose.positions.Length; j++) {
				bool hasPos = false;
				for( int o = 0; o < bones.Length; o++) {
					if (bones[o].name == pose.positions[j].name) {
						#if UNITY_EDITOR
						Undo.RecordObject(bones[o].transform, "Assign Pose");
						#endif
						bones[o].transform.localPosition = pose.positions[j].position;
						#if UNITY_EDITOR
						EditorUtility.SetDirty (bones[o].transform);
						#endif
						hasPos = true;
					}
				}
				if (!hasPos) {
					Debug.Log("This skeleton has no bone '" + pose.positions[j].name + "'");
				}
			}

			for( int k = 0; k < pose.targets.Length; k++) {
				bool hasTarget = false;
				for( int n = 0; n < bones.Length; n++) {
					if (bones[n].name == pose.targets[k].name) {
						InverseKinematics ik = bones[n].GetComponent<InverseKinematics>();

						if (ik != null) {
							#if UNITY_EDITOR
							Undo.RecordObject(ik.target, "Assign Pose");
							#endif
							ik.target.transform.localPosition = pose.targets[k].position;
							#if UNITY_EDITOR
							EditorUtility.SetDirty (ik.target.transform);
							#endif
						}
						else {
							Debug.Log("This skeleton has no ik for bone '" + bones[n].name + "'");
						}
						hasTarget = true;
					}
				}
				if (!hasTarget) {
					Debug.Log("This skeleton has no bone '" + pose.targets[k].name + "'");
				}
			}
		}

        if (pose.controlPoints.Length > 0) {
			for( int l = 0; l < pose.controlPoints.Length; l++) {
				bool hasControlPoint = false;
				if (cps.Length > 0)
				{
					for( int c = 0; c < cps.Length; c++) {
						if (cps[c].name == pose.controlPoints[l].name) {
							#if UNITY_EDITOR
							Undo.RecordObject(cps[c].transform, "Assign Pose");
							#endif
							cps[c].transform.localPosition = pose.controlPoints[l].position;
							#if UNITY_EDITOR
							EditorUtility.SetDirty (cps[c].transform);
							#endif
							hasControlPoint = true;
						}
					}
				}
				if (skin2Ds.Length > 0)
				{
					for( int s = 0; s < skin2Ds.Length; s++) {
						if (skin2Ds[s].points != null && skin2Ds[s].controlPoints != null 
						&& skin2Ds[s].controlPoints.Length > 0 
						&& pose.controlPoints[l].name.StartsWith(skin2Ds[s].name + " Control Point")){
							#if UNITY_EDITOR
							Undo.RecordObject(skin2Ds[s], "Assign Pose");
							Undo.RecordObject(skin2Ds[s].points, "Assign Pose");
							#endif
							int index = GetControlPointIndex(pose.controlPoints[l].name);
							skin2Ds[s].controlPoints[index].position = pose.controlPoints[l].position;
							skin2Ds[s].points.SetPoint(skin2Ds[s].controlPoints[index]);
							#if UNITY_EDITOR
							EditorUtility.SetDirty (skin2Ds[s]);
							EditorUtility.SetDirty (skin2Ds[s].points);
							#endif
							hasControlPoint = true;
							// Debug.Log("Found " + pose.controlPoints[l].name + " set to " + index + skin2Ds[s].points.GetPoint(skin2Ds[s].controlPoints[index]));
						}
					}
				}
				if (!hasControlPoint) {
					Debug.Log("There is no control point '" + pose.controlPoints[l].name + "'");
				}
			}
		}
    }

	// Get the index from the control point name
	int GetControlPointIndex(string controlPointName) {
		int index = controlPointName.LastIndexOf(" ");
		string cpName = controlPointName.Substring(index + 1);
		cpName = cpName.Replace("Point", "");
		int cpIndex = 0;
		if (cpName != "") {
			cpIndex = int.Parse(cpName);
		}
		return cpIndex;
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
		skin2Ds = transform.GetComponentsInChildren<Skin2D>(true);
		for( int i = 0; i < skins.Length; i++) {
			bool skinActive = skin2Ds[i].gameObject.activeSelf;
			skin2Ds[i].gameObject.SetActive(true);
			skin2Ds[i].CalculateBoneWeights(bones, weightToParent);
			skin2Ds[i].gameObject.SetActive(skinActive);
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

		if ((int)transform.localEulerAngles.x == 0 && (int)transform.localEulerAngles.y == 180 || (int)(int)transform.localEulerAngles.x == 180 && (int)transform.localEulerAngles.y == 0)
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

		if ((int)transform.localEulerAngles.x == 0 && (int)transform.localEulerAngles.y == 180 || (int)transform.localEulerAngles.x == 180 && (int)transform.localEulerAngles.y == 0)
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
			for( int i = 0; i < skins.Length; i++) {
				renderers[skins[i].transform] = skins[i].transform.position.z;
			}

			//find all SpriteRenderer elements
			SpriteRenderer[] spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
			for( int j = 0; j < spriteRenderers.Length; j++) {
				renderers[spriteRenderers[j].transform] = spriteRenderers[j].transform.position.z;
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
			for( int i = 0; i < skins.Length; i++) {
				if (spriteShadowsShader != null && skins[i].material.shader == spriteShadowsShader)
				{
					if (!useSharedMaterial) {
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
				if (spriteShadowsShader != null && spriteRenderers[j].material.shader == spriteShadowsShader)
				{
					if (!useSharedMaterial) {
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

	public void UseShadows ()
	{
		//find all SpriteRenderer elements
		skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		
		for( int i = 0; i < skins.Length; i++) {
			if (useShadows && spriteShadowsShader != null)
			{
				if (!useSharedMaterial) {
					#if UNITY_EDITOR
					Undo.RecordObject(skins[i].material.shader, "Use Shadows");
					#endif
					skins[i].material.shader = spriteShadowsShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (skins[i].material.shader);
					#endif
				} else {
					#if UNITY_EDITOR
					Undo.RecordObject(skins[i].sharedMaterial.shader, "Use Shadows");
					#endif
					skins[i].sharedMaterial.shader = spriteShadowsShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (skins[i].sharedMaterial.shader);
					#endif
				}
			}
			else
			{
				if (!useSharedMaterial) {
					#if UNITY_EDITOR
					Undo.RecordObject(skins[i].material.shader, "Use Shadows");
					#endif
					skins[i].material.shader = spriteShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (skins[i].material.shader);
					#endif
				} else {
					#if UNITY_EDITOR
					Undo.RecordObject(skins[i].sharedMaterial.shader, "Use Shadows");
					#endif
					skins[i].sharedMaterial.shader = spriteShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (skins[i].sharedMaterial.shader);
					#endif
				}
			}

			if (useShadows){
				skins[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
			}
			else {
				skins[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
			skins[i].receiveShadows = useShadows;
		}

		//find all SpriteRenderer elements
		spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
		
		for( int j = 0; j < spriteRenderers.Length; j++) {
			if (useShadows && spriteShadowsShader != null)
			{
				if (!useSharedMaterial) {
					#if UNITY_EDITOR
					Undo.RecordObject(spriteRenderers[j].material.shader, "Use Shadows");
					#endif
					spriteRenderers[j].material.shader = spriteShadowsShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (spriteRenderers[j].material.shader);
					#endif
				} else {
					#if UNITY_EDITOR
					Undo.RecordObject(spriteRenderers[j].sharedMaterial.shader, "Use Shadows");
					#endif
					spriteRenderers[j].sharedMaterial.shader = spriteShadowsShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (spriteRenderers[j].sharedMaterial.shader);
					#endif
				}
			}
			else
			{
				if (!useSharedMaterial) {
					#if UNITY_EDITOR
					Undo.RecordObject(spriteRenderers[j].material.shader, "Use Shadows");
					#endif
					spriteRenderers[j].material.shader = spriteShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (spriteRenderers[j].material.shader);
					#endif
				} else {
					#if UNITY_EDITOR
					Undo.RecordObject(spriteRenderers[j].sharedMaterial.shader, "Use Shadows");
					#endif
					spriteRenderers[j].sharedMaterial.shader = spriteShader;
					#if UNITY_EDITOR
					EditorUtility.SetDirty (spriteRenderers[j].sharedMaterial.shader);
					#endif
				}
			}

			if (useShadows){
				spriteRenderers[j].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
			}
			else {
				spriteRenderers[j].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
			spriteRenderers[j].receiveShadows = useShadows;
		}
	}

	public void UseZSorting ()
	{
		//find all SpriteRenderer elements
		skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		
		for( int i = 0; i < skins.Length; i++) {
			if (useZSorting)
			{
				#if UNITY_EDITOR
				Undo.RecordObject(skins[i].transform, "Use Z Sorting");
				#endif
				float z = skins[i].sortingOrder / -10000f;
				skins[i].transform.localPosition = new Vector3(skins[i].transform.localPosition.x, skins[i].transform.localPosition.y, z);
				skins[i].sortingLayerName = "Default";
				skins[i].sortingOrder = 0;
				#if UNITY_EDITOR
				EditorUtility.SetDirty (skins[i].transform);
				#endif
			}
			else
			{
				#if UNITY_EDITOR
				Undo.RecordObject(skins[i].transform, "Use Z Sorting");
				#endif
				int sortLayer = Mathf.RoundToInt(skins[i].transform.localPosition.z * -10000);
				skins[i].transform.localPosition = new Vector3(skins[i].transform.localPosition.x, skins[i].transform.localPosition.y, 0);
				skins[i].sortingLayerName = "Default";
				skins[i].sortingOrder = sortLayer;
				#if UNITY_EDITOR
				EditorUtility.SetDirty (skins[i].transform);
				#endif
			}
		}

		//find all SpriteRenderer elements
		spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
		
		for( int j = 0; j < spriteRenderers.Length; j++) {
			if (useZSorting)
			{
				#if UNITY_EDITOR
				Undo.RecordObject(spriteRenderers[j].transform, "Use Z Sorting");
				#endif
				float z = spriteRenderers[j].sortingOrder / -10000f;
				spriteRenderers[j].transform.localPosition = new Vector3(spriteRenderers[j].transform.localPosition.x, spriteRenderers[j].transform.localPosition.y, z);
				spriteRenderers[j].sortingLayerName = "Default";
				spriteRenderers[j].sortingOrder = 0;
				#if UNITY_EDITOR
				EditorUtility.SetDirty (spriteRenderers[j].transform);
				#endif
			}
			else
			{
				#if UNITY_EDITOR
				Undo.RecordObject(spriteRenderers[j].transform, "Use Z Sorting");
				#endif
				int sortLayer = Mathf.RoundToInt(spriteRenderers[j].transform.localPosition.z * -10000);
				spriteRenderers[j].transform.localPosition = new Vector3(spriteRenderers[j].transform.localPosition.x, spriteRenderers[j].transform.localPosition.y, 0);
				spriteRenderers[j].sortingLayerName = "Default";
				spriteRenderers[j].sortingOrder = sortLayer;
				#if UNITY_EDITOR
				EditorUtility.SetDirty (spriteRenderers[j].transform);
				#endif
			}
		}
	}
}
