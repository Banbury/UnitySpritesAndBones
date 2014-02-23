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

[ExecuteInEditMode]
public class Skeleton : MonoBehaviour {
    public bool editMode = true;
    public bool showBoneInfluence = true;

    //[HideInInspector]
    public Pose basePose;

    private Pose tempPose;

#if UNITY_EDITOR
		[MenuItem("GameObject/Create Other/Skeleton")]
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
	void Update () {
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

        List<RotationValue> rotations = new List<RotationValue>();
        List<PositionValue> positions = new List<PositionValue>();
        List<PositionValue> targets = new List<PositionValue>();

        foreach (Bone b in bones) {
            rotations.Add(new RotationValue(b.name, b.transform.localRotation));
            positions.Add(new PositionValue(b.name, b.transform.localPosition));

            if (b.GetComponent<InverseKinematics>() != null) {
                targets.Add(new PositionValue(b.name, b.GetComponent<InverseKinematics>().target.localPosition));
            }
        }

        pose.rotations = rotations.ToArray();
        pose.positions = positions.ToArray();
        pose.targets = targets.ToArray();

        return pose;
    }
		
    public void SavePose(string poseFileName) {
		if(poseFileName!=null && poseFileName.Trim()!=""){
        	ScriptableObjectUtility.CreateAsset(CreatePose(),poseFileName);
		}
		else{
			ScriptableObjectUtility.CreateAsset(CreatePose());
		}
    }

    public void RestorePose(Pose pose) {
        var bones = GetComponentsInChildren<Bone>();
        Undo.RegisterCompleteObjectUndo(bones, "Assign Pose");

        foreach (RotationValue rv in pose.rotations) {
            System.Array.Find<Bone>(bones, b => b.name == rv.name).transform.localRotation = rv.rotation;
        }

        foreach (PositionValue pv in pose.positions) {
            System.Array.Find<Bone>(bones, b => b.name == pv.name).transform.localPosition = pv.position;
        }

        foreach (PositionValue tv in pose.targets) {
            Bone bone = System.Array.Find<Bone>(bones, b => b.name == tv.name);
            InverseKinematics ik = bone.GetComponent<InverseKinematics>();

            if (ik != null) {
                Undo.RecordObject(ik.target, "Assign Pose");
                ik.target.transform.localPosition = tv.position;
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
}
