/*
The MIT License (MIT)

Copyright (c) 2015 Play-Em

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

// AnimationCurvesCopier is based on AlexanderYoshi's Animation Heirarchy Editor script, 
// https://github.com/s-m-k/Unity-Animation-Hierarchy-Editor/blob/master/AnimationHierarchyEditor.cs
// this script will copy selected curves to selected animation clips.

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class AnimationCurvesCopier : EditorWindow {
	private static int columnWidth = 300;

	public AnimationClip copyFromClip;
	private List<AnimationClip> animationClips;
	private ArrayList pathsKeys;
	private Hashtable paths;
	private Hashtable selectedCurves = new Hashtable();
	private Vector2 scrollPos = Vector2.zero;
	private string filter = "";

    [MenuItem("Window/Animation Curves Copier")]
    static void ShowWindow() {
        EditorWindow.GetWindow<AnimationCurvesCopier>();
    }


	public AnimationCurvesCopier(){
		animationClips = new List<AnimationClip>();
	}

	void OnSelectionChange() {
		if (Selection.objects.Length > 1 )
		{
			// Debug.Log ("Selected Objects Length: " + Selection.objects.Length);
			animationClips.Clear();
			foreach ( Object o in Selection.objects )
			{
				if ( o is AnimationClip ) animationClips.Add((AnimationClip)o);
			}
		}
		else if (Selection.activeObject is AnimationClip) {
			animationClips.Clear();
			animationClips.Add((AnimationClip)Selection.activeObject);
		} else {
			animationClips.Clear();
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

		AnimationClip newClip;
		EditorGUILayout.BeginHorizontal();
		newClip = (AnimationClip)EditorGUILayout.ObjectField("Copy From Clip:",
			copyFromClip,
			typeof(AnimationClip),
			true);

		if (newClip != copyFromClip) {
			copyFromClip = newClip;
			FillModel();
		}

		if (animationClips.Count > 0 ) {
			if ( animationClips.Count == 1 )
			{
				animationClips[0] = (AnimationClip)EditorGUILayout.ObjectField("Copy To Clip:",
					animationClips[0],
					typeof(AnimationClip),
					true);
			}
			else
			{
				GUILayout.Label("Copy To Multiple Clips: " + animationClips.Count, GUILayout.Width(columnWidth));
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Curves to Clips", GUILayout.Width(columnWidth*0.5f))) {
				Debug.Log("Added curves to clips");
				AddCurves();
			}

			EditorGUILayout.EndHorizontal();

		} else {

			EditorGUILayout.EndHorizontal();
		}

		if (copyFromClip != null) {

			EditorGUILayout.BeginHorizontal();
			string newFilter = "";
			newFilter = EditorGUILayout.TextField("Filter by Property Name:", filter, GUILayout.Width(columnWidth));

			if (newFilter != filter) {
				filter = newFilter;
				FillModel();
			}
			if (GUILayout.Button("Select All", GUILayout.Width(columnWidth*0.5f))) {
				Debug.Log("Selected All curves");
				SelectAll(true);
			}
			if (GUILayout.Button("Select None", GUILayout.Width(columnWidth*0.5f))) {
				Debug.Log("Selected no curves");
				SelectAll(false);
			}
			EditorGUILayout.EndHorizontal();

			scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

			GUILayout.Space(20);

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Reference path:", GUILayout.Width(columnWidth*1.5f));
			GUILayout.Label("Property name:", GUILayout.Width(columnWidth*0.5f));
			GUILayout.Label("Curve:", GUILayout.Width(columnWidth*0.75f));
			EditorGUILayout.EndHorizontal();

			if (paths == null) {
				FillModel();
			}

			if (paths != null) {
				foreach (string path in pathsKeys) {
					GUICurvesToCopy(path);
				}
			}

			GUILayout.Space(40);
			GUILayout.EndScrollView();
		} else {
			GUILayout.Label("Please select an Animation Clip");
		}
	}

	void GUICurvesToCopy(string path) {
		AnimationCurve newCurve;
		ArrayList properties = (ArrayList)paths[path];

		if (properties != null) {
			for(int i = 0; i < properties.Count; i++) {
				EditorCurveBinding thisCurve = (EditorCurveBinding)properties[i];
				string propertyName = thisCurve.propertyName.ToUpper();
				if (filter == null || propertyName.Contains(filter.ToUpper())) {
					bool selected = false;
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label(ShortenedPath(path), GUILayout.Width(columnWidth*1.5f));
					
					if (!selectedCurves.ContainsKey(path + " " + thisCurve.propertyName)) {
						selectedCurves[path + " " + thisCurve.propertyName] = selected;
					} 
					selectedCurves[path + " " + thisCurve.propertyName] = EditorGUILayout.ToggleLeft(thisCurve.propertyName, (bool)selectedCurves[path + " " + thisCurve.propertyName], GUILayout.Width(columnWidth*0.5f));
					newCurve = (AnimationCurve)AnimationUtility.GetEditorCurve(copyFromClip, thisCurve);
					if (newCurve != null) {
						newCurve = (AnimationCurve)EditorGUILayout.CurveField(newCurve, GUILayout.Width(columnWidth*0.75f));
					}

					EditorGUILayout.EndHorizontal();
				}
			}
		};
	}

	void OnInspectorUpdate() {
		this.Repaint();
	}

	string ShortenedPath(string oldPath) {
		int index = oldPath.LastIndexOf("/");
		string objectName = oldPath.Substring(index + 1);
		if ((index - 1) >= 0) {
			string parentPath = oldPath.Substring(0, index);
			index = parentPath.LastIndexOf("/");
			string parentName = parentPath.Substring(index + 1);
			objectName = parentName + "/" + objectName;
		}
		index = oldPath.IndexOf("/");
		string newPath = "";
		if (index != -1) {
			newPath = oldPath.Substring(0, index);
		}
		if (!objectName.StartsWith(newPath)) {
			newPath = newPath + "/" + objectName;
		} else {
			newPath = objectName;
		}
		return newPath;
	}

	void FillModel() {
		paths = new Hashtable();
		pathsKeys = new ArrayList();
		selectedCurves = new Hashtable();

		FillModelWithCurves(AnimationUtility.GetCurveBindings(copyFromClip));
		FillModelWithCurves(AnimationUtility.GetObjectReferenceCurveBindings(copyFromClip));
	}
	
	private void FillModelWithCurves(EditorCurveBinding[] curves) {
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

	void AddCurves() {
		if (animationClips.Count > 0 && selectedCurves.Count > 0 && paths != null) {

			float fProgress = 0.0f;

			AssetDatabase.StartAssetEditing();
			
			for ( int iCurrentClip = 0; iCurrentClip < animationClips.Count; iCurrentClip++ )
			{
				AnimationClip animationClip =  animationClips[iCurrentClip];
				Undo.RecordObject(animationClip, "Animation Curves Copier Change");
				
				for ( int iCurrentPath = 0; iCurrentPath < pathsKeys.Count; iCurrentPath ++)
				{
					string path = pathsKeys[iCurrentPath] as string;
					ArrayList curves = (ArrayList)paths[path];

					for (int i = 0; i < curves.Count; i++) 
					{
						EditorCurveBinding binding = (EditorCurveBinding)curves[i];

						if (selectedCurves.ContainsKey(path + " " + binding.propertyName)) {
							if ((bool)selectedCurves[path + " " + binding.propertyName]) {

								AnimationCurve curve = AnimationUtility.GetEditorCurve(copyFromClip, binding);
								if ( curve != null )
								{
									AnimationUtility.SetEditorCurve(animationClip, binding, null);
									AnimationUtility.SetEditorCurve(animationClip, binding, curve);
								}
								else
								{
									ObjectReferenceKeyframe[] objectReferenceCurve = AnimationUtility.GetObjectReferenceCurve(copyFromClip, binding);
									AnimationUtility.SetObjectReferenceCurve(animationClip, binding, null);
									AnimationUtility.SetObjectReferenceCurve(animationClip, binding, objectReferenceCurve);
								}
							}
						}
					}

					// Update the progress meter
					float fChunk = 1f / animationClips.Count;
					fProgress = (iCurrentClip * fChunk) + fChunk * ((float) iCurrentPath / (float) pathsKeys.Count);

					EditorUtility.DisplayProgressBar(
						"Animation Curves Copier Progress", 
						"Copying curves to animation clips. . .",
						fProgress);
				}

			}
			AssetDatabase.StopAssetEditing();
			EditorUtility.ClearProgressBar();

			this.Repaint();
		}
	}

	void SelectAll(bool select) {
		if (selectedCurves != null && selectedCurves.Count > 0) {
			Hashtable newSelectedCurves = new Hashtable();
			foreach (string key in selectedCurves.Keys) {
				newSelectedCurves.Add(key, select);
			}
			selectedCurves = newSelectedCurves;
		}
	}
}
