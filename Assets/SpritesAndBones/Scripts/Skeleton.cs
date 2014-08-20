/*
The MIT License (MIT)

Copyright (c) 2013 Banbury

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
using System.Reflection;
using System.Linq;

[ExecuteInEditMode]
[SelectionBase]
public class Skeleton : MonoBehaviour {
    public bool editMode = true;
    public bool showBoneInfluence = true;

    //[HideInInspector]
    public Pose basePose;

    private Pose tempPose;

    [SerializeField]
    [HideInInspector]
    private bool _flip;
    [SerializeField]
    [HideInInspector]
    private bool _useShadows;

#if UNITY_EDITOR
    public bool flip {
        get { return _flip; }
        set {
            _flip = value;
            Flip();
        }
    }

    public bool useShadows {
        get { return _useShadows; }
        set {
            _useShadows = value;
            UseShadows();
        }
    }
#endif

    private Shader spriteShader;
    private Shader spriteShadowsShader;

#if UNITY_EDITOR
    [MenuItem("Sprites And Bones/Skeleton")]
    public static void Create() {
        Undo.IncrementCurrentGroup();

        GameObject o = new GameObject("Skeleton");
        Undo.RegisterCreatedObjectUndo(o, "Create skeleton");
        o.AddComponent<Skeleton>();

        GameObject b = new GameObject("Bone");
        Undo.RegisterCreatedObjectUndo(b, "Create Skeleton");
        b.AddComponent<Bone>();

        b.transform.parent = o.transform;

        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
    }
#endif

    // Use this for initialization
    void Start() {
        spriteShader = Shader.Find("Sprites/Default");
        spriteShadowsShader = Shader.Find("Sprites/Skeleton-Diffuse");
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
        foreach (Bone b in gameObject.GetComponentsInChildren<Bone>()) {
            InverseKinematics ik = b.GetComponent<InverseKinematics>();

            if (ik != null && !editMode && ik.enabled && ik.influence > 0) {
                ik.resolveSK2D();
            }
        }
    }

    // Update is called once per frame
    void Update() {
        // Get Shaders if they are null
        if (spriteShader == null) {
            spriteShader = Shader.Find("Sprites/Default");
        }
        if (spriteShadowsShader == null) {
            spriteShadowsShader = Shader.Find("Sprites/Skeleton-Diffuse");
        }

#if !UNITY_EDITOR
		EditorUpdate();
#else
        if (Application.isEditor) {
            foreach (Bone b in gameObject.GetComponentsInChildren<Bone>()) {
                b.editMode = editMode;
                b.showInfluence = showBoneInfluence;
            }
        }
#endif
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "man_icon.png", true);
    }

    public Pose CreatePose() {
        Pose pose = ScriptableObject.CreateInstance<Pose>();

        var bones = GetComponentsInChildren<Bone>();
        var cps = GetComponentsInChildren<ControlPoint>();

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
            controlPoints.Add(new PositionValue(cp.transform.parent.name + cp.name, cp.transform.localPosition));
        }

        pose.rotations = rotations.ToArray();
        pose.positions = positions.ToArray();
        pose.targets = targets.ToArray();
        pose.controlPoints = controlPoints.ToArray();

        return pose;
    }

    public void SavePose(string poseFileName) {
        if (poseFileName != null && poseFileName.Trim() != "") {
            ScriptableObjectUtility.CreateAsset(CreatePose(), poseFileName);
        } else {
            ScriptableObjectUtility.CreateAsset(CreatePose());
        }
    }

    public void RestorePose(Pose pose) {
        var bones = GetComponentsInChildren<Bone>();
        var cps = GetComponentsInChildren<ControlPoint>();
        Undo.RegisterCompleteObjectUndo(bones, "Assign Pose");
        Undo.RegisterCompleteObjectUndo(cps, "Assign Pose");

        foreach (RotationValue rv in pose.rotations) {
            Bone bone = bones.First(b => b.name == rv.name);
            if (bone != null) {
                bone.transform.localRotation = rv.rotation;
            } else {
                Debug.Log("This skeleton has no bone '" + bone.name + "'");
            }
        }

        foreach (PositionValue pv in pose.positions) {
            Bone bone = bones.First(b => b.name == pv.name);
            if (bone != null) {
                bone.transform.localPosition = pv.position;
            } else {
                Debug.Log("This skeleton has no bone '" + bone.name + "'");
            }
        }

        foreach (PositionValue tv in pose.targets) {
            Bone bone = bones.First(b => b.name == tv.name);

            if (bone != null) {
                InverseKinematics ik = bone.GetComponent<InverseKinematics>();

                if (ik != null) {
                    Undo.RecordObject(ik.target, "Assign Pose");
                    ik.target.transform.localPosition = tv.position;
                }
            } else {
                Debug.Log("This skeleton has no bone '" + bone.name + "'");
            }
        }

        foreach (PositionValue cpv in pose.controlPoints) {
            ControlPoint cp = cps.First(c => (c.transform.parent.name + c.name) == cpv.name);

            if (cp != null) {
                cp.transform.localPosition = cpv.position;
            }
            else {
                Debug.Log("There is no control point '" + cpv.name + "'");
            }
        }
    }

    public void SetBasePose(Pose pose) {
        basePose = pose;
    }
#endif

    public void SetEditMode(bool edit) {
#if UNITY_EDITOR
        if (!editMode && edit) {
            AnimationMode.StopAnimationMode();

            tempPose = CreatePose();
            tempPose.hideFlags = HideFlags.HideAndDontSave;

            if (basePose != null) {
                RestorePose(basePose);
            }
        } else if (editMode && !edit) {
            if (tempPose != null) {
                RestorePose(tempPose);
                Object.DestroyImmediate(tempPose);
            }
        }
#endif

        editMode = edit;
    }

#if UNITY_EDITOR
    public void CalculateWeights(bool weightToParent) {
        //find all Skin2D elements
        Skin2D[] skins = transform.GetComponentsInChildren<Skin2D>();
        Bone[] bones = transform.GetComponentsInChildren<Bone>();
        if (bones.Length == 0) {
            Debug.Log("No bones in skeleton");
            return;
        }
        foreach (Skin2D skin in skins) {
            skin.CalculateBoneWeights(bones, weightToParent);
        }
    }

    public void Flip() {
        int normal = -1;
        // Rotate the skeleton's local transform
        if (!flip) {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0.0f, transform.localEulerAngles.z);
        } else {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 180.0f, transform.localEulerAngles.z);
            normal = 1;
        }

        if (useShadows) {
            //find all SkinnedMeshRenderer elements
            SkinnedMeshRenderer[] skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skin in skins) {
                if (skin.sharedMaterial != null) {
                    if (spriteShadowsShader != null && skin.sharedMaterial.shader == spriteShadowsShader) {
                        skin.sharedMaterial.SetVector("_Normal", new Vector3(0, 0, normal));
                    }
                }
            }

            //find all SpriteRenderer elements
            SpriteRenderer[] spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in spriteRenderers) {
                if (spriteRenderer.sharedMaterial != null) {
                    if (spriteShadowsShader != null && spriteRenderer.sharedMaterial.shader == spriteShadowsShader) {
                        spriteRenderer.sharedMaterial.SetVector("_Normal", new Vector3(0, 0, normal));
                    }
                }
            }
        }
    }

    public void UseShadows() {
        //find all SpriteRenderer elements
        SkinnedMeshRenderer[] skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer skin in skins) {
            if (skin.sharedMaterial != null) {
                if (useShadows && spriteShadowsShader != null) {
                    skin.sharedMaterial.shader = spriteShadowsShader;
                } else {
                    skin.sharedMaterial.shader = spriteShader;
                }

                skin.castShadows = useShadows;
                skin.receiveShadows = useShadows;
            }
        }

        //find all SpriteRenderer elements
        SpriteRenderer[] spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer spriteRenderer in spriteRenderers) {
            if (spriteRenderer.sharedMaterial != null) {
                if (useShadows && spriteShadowsShader != null) {
                    spriteRenderer.sharedMaterial.shader = spriteShadowsShader;
                } else {
                    spriteRenderer.sharedMaterial.shader = spriteShader;
                }

                spriteRenderer.castShadows = useShadows;
                spriteRenderer.receiveShadows = useShadows;
            }
        }
    }
#endif
}
