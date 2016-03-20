/*
The MIT License (MIT)

Copyright (c) 2016 Play-Em

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions)

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
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// Editor window for replacing all control points in animation clips
// with new control point system
public class ReplaceControlPointsInClip : EditorWindow
{
	private bool changeAllPoints = false;
	private static int columnWidth = 300;
	private List<AnimationClip> animationClips;

	private Vector2 scrollPos = Vector2.zero;

	public ReplaceControlPointsInClip(){
		animationClips = new List<AnimationClip>();
	}
	
	void OnSelectionChange() {
		if (Selection.objects.Length > 1 )
		{
			Debug.Log ("Length? " + Selection.objects.Length);
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

	[MenuItem ("Window/Animation Replace Control Points")]
	static void Init ()
	{
		GetWindow (typeof (ReplaceControlPointsInClip));
	}

	public void OnGUI()
	{
		// Make sure we have more than one clip selected
		if (animationClips.Count > 0 ) {
			scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Animation Clip:", GUILayout.Width(columnWidth));

			if ( animationClips.Count == 1 )
			{
				animationClips[0] = ((AnimationClip)EditorGUILayout.ObjectField(
					animationClips[0],
					typeof(AnimationClip),
					true,
					GUILayout.Width(columnWidth))
					);
			} 
			else
			{
				GUILayout.Label("Multiple Anim Clips: " + animationClips.Count, GUILayout.Width(columnWidth));
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(20);

			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.LabelField ("Control Points:");

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Change Control Points")) {
				changeAllPoints = true;
			}

			EditorGUILayout.EndHorizontal();

			for (int i = 0; i < animationClips.Count; i++) {

				// Iterate through all the bindings in the animation clip
				var bindings = AnimationUtility.GetCurveBindings (animationClips[i]);
				for (int n = 0; n < bindings.Length; n++)
				{

					// If the property is the x,y,z position of the control point then edit the new position
					if (bindings[n].propertyName.Contains("m_LocalPosition") && bindings[n].path.Contains("Control Point")) {

						// Get the animation curve
						AnimationCurve curve = AnimationUtility.GetEditorCurve (animationClips[i], bindings[n]);
						// Get the curve's keyframes
						Keyframe[] keyframes = curve.keys;

						// Show the binding and the keys length it has in the editor window
						EditorGUILayout.LabelField (bindings[n].path + "/" + bindings[n].propertyName + ", Keys: " + keyframes.Length);
						// Debug.Log(binding.type);

						if (changeAllPoints) {
							// First you need to create a Editor Curve Binding
							EditorCurveBinding curveBinding = new EditorCurveBinding();
							 
							// I want to change the ControlPoints of the control point, so I put the typeof(ControlPoints) as the binding type.
							curveBinding.type = typeof(ControlPoints);

							// Regular path to the control point gameobject will be changed to the parent
							curveBinding.path = RenameControlPointPath(bindings[n].path);

							// This is the property name to change to the matching control point
							curveBinding.propertyName = GetControlPointName(bindings[n].path) + bindings[n].propertyName.Substring(bindings[n].propertyName.Length - 2);
							
							// Create a new curve from these keyframes
							curve = new AnimationCurve(keyframes);

							// Set the new curve to the animation clip
							AnimationUtility.SetEditorCurve(animationClips[i], curveBinding, curve);

							// Remove the old binding
							AnimationUtility.SetEditorCurve(animationClips[i], bindings[n], null);

							// Track progress
							float fChunk = 1f / animationClips.Count;
							float fProgress = (i * fChunk) + fChunk * ((float) n / (float) bindings.Length);
							
							EditorUtility.DisplayProgressBar(
								"Replacing Control Points with new Control Point System", 
								"How far along the animation editing has progressed.",
								fProgress);
						}

						// Loop through the keyframes and change the curve to the new control points curve
						for (int j = 0; j < keyframes.Length; j++) {
							// Show the new curve in the editor window
							EditorGUILayout.CurveField (curve, GUILayout.Width(columnWidth*0.75f));
						}
					}
				}
			}

			EditorUtility.ClearProgressBar();

			// Reset the button
			changeAllPoints = false;
			GUILayout.Space(40);
			GUILayout.EndScrollView();
		} else {
			GUILayout.Label("Please select an Animation Clip");
		}
	}

	string RenameControlPointPath(string oldPath) {
		int index = oldPath.LastIndexOf("/");
		string newPath = oldPath.Substring(0, index);
		return newPath;
	}

	string GetControlPointName(string oldPath) {
		int index = oldPath.LastIndexOf("/");
		string controlPointName = oldPath.Substring(index + 1);
		index = controlPointName.LastIndexOf(" ");
		string cpName = controlPointName.Substring(index + 1);
		cpName = cpName.Replace("Point", "cp");
		return cpName;
	}

	void OnInspectorUpdate() {
		this.Repaint();
	}
}