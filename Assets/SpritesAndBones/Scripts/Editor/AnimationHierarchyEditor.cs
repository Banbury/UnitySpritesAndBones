using UnityEngine;
using UnityEditor;
using System.Collections;

public class AnimationHierarchyEditor : EditorWindow {
	private static int columnWidth = 300;

	private Animator animatorObject;
    private AnimationClip animationClip;
	private ArrayList pathsKeys;
	private Hashtable paths;
	private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Window/Animation Hierarchy Editor")]
    static void ShowWindow() {
        EditorWindow.GetWindow<AnimationHierarchyEditor>();
    }

    void OnSelectionChange() {
		if (Selection.activeObject is AnimationClip) {
			animationClip = (AnimationClip)Selection.activeObject;
			FillModel();
		} else {
			animationClip = null;
		}

		this.Repaint();
	}

	void OnGUI() {
		if (Event.current.type == EventType.ValidateCommand) {
			switch (Event.current.commandName) {
			case "UndoRedoPerformed":
				FillModel();
				break;
			}
		}

		if (animationClip != null) {
			scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Referenced Animator (Root):", GUILayout.Width(columnWidth));
			animatorObject = ((Animator)EditorGUILayout.ObjectField(
				animatorObject,
				typeof(Animator),
				true,
				GUILayout.Width(columnWidth))
			);

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(20);

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Reference path:", GUILayout.Width(columnWidth));
			GUILayout.Label("Animated properties:", GUILayout.Width(columnWidth));
			GUILayout.Label("Object:", GUILayout.Width(columnWidth));
			EditorGUILayout.EndHorizontal();

			if (paths != null) {
				foreach (string path in pathsKeys) {
					GUICreatePathItem(path);
				}
			}

			GUILayout.Space(40);
			GUILayout.EndScrollView();
		} else {
			GUILayout.Label("Please select an Animation Clip");
		}
	}

	void GUICreatePathItem(string path) {
		string newPath = "";
		GameObject obj = FindObjectInRoot(path);
		GameObject newObj;
		ArrayList properties = (ArrayList)paths[path];

		EditorGUILayout.BeginHorizontal();

		newPath = EditorGUILayout.TextField(path, GUILayout.Width(columnWidth));

		EditorGUILayout.LabelField(
			properties != null ? properties.Count.ToString() : "0",
			GUILayout.Width(columnWidth)
		);

		Color standardColor = GUI.color;

		if (obj != null) {
			GUI.color = Color.green;
		} else {
			GUI.color = Color.red;
		}

		newObj = (GameObject)EditorGUILayout.ObjectField(
			obj,
			typeof(GameObject),
			true,
			GUILayout.Width(columnWidth)
		);

		GUI.color = standardColor;

		EditorGUILayout.EndHorizontal();

		try {
			if (obj != newObj) {
				UpdatePath(path, ChildPath(newObj));
			}

			if (newPath != path) {
				UpdatePath(path, newPath);
			}
		} catch (UnityException ex) {
			Debug.LogError(ex.Message);
		}
	}

	void OnInspectorUpdate() {
		this.Repaint();
	}

	void FillModel() {
		EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(animationClip);
		
		paths = new Hashtable();
		pathsKeys = new ArrayList();
		foreach (EditorCurveBinding curveData in curves) {
			string key = curveData.path;
			
			if (paths.ContainsKey(key)) {
				((ArrayList)paths[key]).Add(curveData);
			} else {
				ArrayList newProperties = new ArrayList();
				newProperties.Add(curveData);
				paths.Add(key, newProperties);
				pathsKeys.Add(key);
			}
		}
	}

	void UpdatePath(string oldPath, string newPath) {
		if (paths[newPath] != null) {
			throw new UnityException("Path " + newPath + " already exists in that animation!");
		}

		Undo.RecordObject(animationClip, "Animation Hierarchy Change");

		//recreating all curves one by one
		//to maintain proper order in the editor - 
		//slower than just removing old curve
		//and adding a corrected one, but it's more
		//user-friendly
		foreach (string path in pathsKeys) {
			ArrayList curves = (ArrayList)paths[path];

			for (int i = 0; i < curves.Count; i++) {
				EditorCurveBinding binding = (EditorCurveBinding)curves[i];
				AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, binding);

				AnimationUtility.SetEditorCurve(animationClip, binding, null);

				if (path == oldPath) {
					binding.path = newPath;
				}

				AnimationUtility.SetEditorCurve(animationClip, binding, curve);
			}
		}

		FillModel();
		this.Repaint();
	}

	GameObject FindObjectInRoot(string path) {
		if (animatorObject == null) {
			return null;
		}

		Transform child = animatorObject.transform.Find(path);

		if (child != null) {
			return child.gameObject;
		} else {
			return null;
		}
	}

	string ChildPath(GameObject obj, bool sep = false) {
		if (animatorObject == null) {
			throw new UnityException("Please assign Referenced Animator (Root) first!");
		}

		if (obj == animatorObject.gameObject) {
			return "";
		} else {
			if (obj.transform.parent == null) {
				throw new UnityException("Object must belong to " + animatorObject.ToString() + "!");
			} else {
				return ChildPath(obj.transform.parent.gameObject, true) + obj.name + (sep ? "/" : "");
			}
		}
	}
}
