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

// AnimationEventCopier is based on AlexanderYoshi's Animation Heirarchy Editor script, 
// https://github.com/s-m-k/Unity-Animation-Hierarchy-Editor/blob/master/AnimationHierarchyEditor.cs
// this script will copy selected curves to selected animation clips.

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class AnimationEventCopier : EditorWindow {
	private static int columnWidth = 300;

	private Animator animatorObject;
	public AnimationClip copyFromClip;
	private List<AnimationClip> animationClips;

    [MenuItem("Window/Animation Event Copier")]
    static void ShowWindow() {
        EditorWindow.GetWindow<AnimationEventCopier>();
    }


	public AnimationEventCopier(){
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

		AnimationClip newClip;
		EditorGUILayout.BeginHorizontal();
		newClip = (AnimationClip)EditorGUILayout.ObjectField("Copy From Clip:",
			copyFromClip,
			typeof(AnimationClip),
			true);

		if (newClip != copyFromClip) {
			copyFromClip = newClip;
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
			if (GUILayout.Button("Add Events to Clips", GUILayout.Width(columnWidth*0.5f))) {
				Debug.Log("Added Events to clips");
				AddEvents();
			}

			EditorGUILayout.EndHorizontal();

		} else {
			GUILayout.Label("Please select an Animation Clip to Copy To");
			EditorGUILayout.EndHorizontal();
		}

		if (copyFromClip == null) {
			GUILayout.Label("Please select an Animation Clip to Copy From");
		}
	}

	void OnInspectorUpdate() {
		this.Repaint();
	}

	void AddEvents() {
		if (animationClips.Count > 0) {

			float fProgress = 0.0f;

			AssetDatabase.StartAssetEditing();
			
			for ( int iCurrentClip = 0; iCurrentClip < animationClips.Count; iCurrentClip++ )
			{
				AnimationClip animationClip =  animationClips[iCurrentClip];
				Undo.RecordObject(animationClip, "Animation Events Copier Change");

				AnimationEvent[] animationEvents = AnimationUtility.GetAnimationEvents(animationClip);
				AnimationEvent[] newEvents = AnimationUtility.GetAnimationEvents(copyFromClip);

				List<AnimationEvent> addedEvents = new List<AnimationEvent>();
				if (animationEvents != null && animationEvents.Length > 0) {
					for (int i = 0; i < animationEvents.Length; i++) {
						bool replaceEvent = false;
						foreach (AnimationEvent animEvent in newEvents) {
							if (animationEvents[i].functionName == animEvent.functionName && animationEvents[i].time == animEvent.time) {
								Debug.Log ("Animation Event " + animationEvents[i].functionName + " already exists at time " + animationEvents[i].time);
								replaceEvent = true;
							}
						}
						if (!replaceEvent) {
							addedEvents.Add(animationEvents[i]);
						}
					}
				}

				foreach (AnimationEvent animEvent in newEvents) {
					addedEvents.Add(animEvent);
				}

				newEvents = addedEvents.ToArray();
				// AnimationUtility.SetAnimationEvents(animationClip, null);
				AnimationUtility.SetAnimationEvents(animationClip, newEvents);

				// Update the progress meter
				float fChunk = 1f / animationClips.Count;
				fProgress = (iCurrentClip * fChunk) + fChunk * ((float) iCurrentClip / (float) animationClips.Count);

				EditorUtility.DisplayProgressBar(
					"Animation Event Copier Progress", 
					"Copying Animation Events to animation clips. . .",
					fProgress);

			}
			AssetDatabase.StopAssetEditing();
			EditorUtility.ClearProgressBar();

			this.Repaint();
		}
	}
}
