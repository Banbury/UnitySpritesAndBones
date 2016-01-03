using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Editor window for replacing all sprites in animation clips
public class ReplaceSpritesInClip : EditorWindow
{
	private string originalSpriteText = "";
	private string replaceSpriteText = "";
	private bool changeAllSprites = false;
	private static int columnWidth = 300;
	private Sprite[] sprites;
	private List<AnimationClip> animationClips;

	private Vector2 scrollPos = Vector2.zero;

	public ReplaceSpritesInClip(){
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

	[MenuItem ("Window/Animation Replace Sprites")]
	static void Init ()
	{
		GetWindow (typeof (ReplaceSpritesInClip));
	}

	public void OnGUI()
	{
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
			
			EditorGUILayout.LabelField ("Sprites:");

			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			
			GUILayout.Label("Original Sprite Text:", GUILayout.Width(columnWidth));
			GUILayout.Label("Replacement Sprite Text:", GUILayout.Width(columnWidth));

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			originalSpriteText = EditorGUILayout.TextField(originalSpriteText, GUILayout.Width(columnWidth));
			replaceSpriteText = EditorGUILayout.TextField(replaceSpriteText, GUILayout.Width(columnWidth));
			if (GUILayout.Button("Replace All Sprites")) {
				changeAllSprites = true;
			}

			EditorGUILayout.EndHorizontal();

			foreach (AnimationClip clip in animationClips) {
				// First you need to create e Editor Curve Binding
				EditorCurveBinding curveBinding = new EditorCurveBinding();
				 
				// I want to change the sprites of the sprite renderer, so I put the typeof(SpriteRenderer) as the binding type.
				curveBinding.type = typeof(SpriteRenderer);
				// Regular path to the gameobject that will be changed (empty string means root)
				curveBinding.path = "";
				// This is the property name to change the sprite of a sprite renderer
				curveBinding.propertyName = "m_Sprite";
				 
				// An array to hold the object keyframes
				ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[10];
	 
				foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings (clip))
				{
					if (binding.propertyName == "m_Sprite") {
						ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve (clip, binding);
						EditorGUILayout.LabelField (binding.path + "/" + binding.propertyName + ", Keys: " + keyframes.Length);
						for (int i = 0; i < keyframes.Length; i++) {
							if (changeAllSprites) {
								Sprite keyframeSprite = (Sprite)keyframes[i].value;
								if (keyframeSprite != null) {
									string spriteName = keyframeSprite.name;
									string newSpriteName = spriteName.Replace(originalSpriteText, replaceSpriteText);
									Debug.Log(newSpriteName);
									GetAllSprites();
									if (sprites.Length > 0) {
										bool foundSprite = false;
										foreach (Sprite sprite in sprites) {
											if (sprite != null && sprite.name == newSpriteName) {
												float timeForKey = keyframes[i].time;
												keyframes[i] = new ObjectReferenceKeyframe();
												// set the time
												keyframes[i].time = timeForKey;
												// set reference for the sprite you want
												keyframes[i].value = sprite;
												AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
												Debug.Log("Sprite changed to " + sprite.name);
												break;
											}
										}
									}
								}
							}
							EditorGUILayout.ObjectField (keyframes[i].value, typeof (Sprite), false);
						}
					}
				}
			}
			changeAllSprites = false;
			GUILayout.Space(40);
			GUILayout.EndScrollView();
		} else {
			GUILayout.Label("Please select an Animation Clip");
		}
	}

	void OnInspectorUpdate() {
		this.Repaint();
	}

	void GetAllSprites() {

		string[] extensions = new string[] {".png",".psd",".jpg",".bmp"};
		string[] files = AssetDatabase.GetAllAssetPaths().Where(x=>extensions.Contains(System.IO.Path.GetExtension(x))).ToArray();

		List<Sprite> spriteList = new List<Sprite>();
		foreach (string filename in files) {
			Object[] objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(filename);
			spriteList.AddRange(objects.Select(x=>(x as Sprite)).ToList());
		}
		sprites = spriteList.ToArray();
	}
}