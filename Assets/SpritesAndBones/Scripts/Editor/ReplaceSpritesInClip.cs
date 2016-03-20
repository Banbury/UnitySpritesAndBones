using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Editor window for replacing all sprites in animation clips
public class ReplaceSpritesInClip : EditorWindow
{
	// The original sprite's text you want to replace
	private string originalSpriteText = "";
	
	// The sprite's text you want to change to
	private string replaceSpriteText = "";
	
	// Bool to track if the button was pushed
	private bool changeAllSprites = false;
	private static int columnWidth = 300;
	
	// Sprites of the clip
	private Sprite[] sprites;
	
	// Animation clips selected to change
	private List<AnimationClip> animationClips;

	// Current scroll position
	private Vector2 scrollPos = Vector2.zero;

	// Re-initialize animation clips on creation
	public ReplaceSpritesInClip(){
		animationClips = new List<AnimationClip>();
	}
	
	// On changing the selection update the animation clips to change
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
		// Make sure there is at least one animation clip selected
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

			// Iterate through the animation clips
			foreach (AnimationClip clip in animationClips) {

				// Iterate through the bindings for the current animation clip
				foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings (clip))
				{
					// If the binding is a sprite type then get the keyframes
					if (binding.propertyName == "m_Sprite") {
						// Get the keyframes for this sprite
						ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve (clip, binding);
						// Show the sprite's path and the keyframes length for this binding
						EditorGUILayout.LabelField (binding.path + "/" + binding.propertyName + ", Keys: " + keyframes.Length);
						
						// Loop through the keyframes to change the sprite
						for (int i = 0; i < keyframes.Length; i++) {
							// If the button is pushed then change the sprite
							if (changeAllSprites) {
								// The keyframe's value is the sprite
								Sprite keyframeSprite = (Sprite)keyframes[i].value;
								// If the sprite exists get the name of the sprite
								if (keyframeSprite != null) {
									string spriteName = keyframeSprite.name;
									// Replace the text in the sprite's name to the replacement text
									string newSpriteName = spriteName.Replace(originalSpriteText, replaceSpriteText);
									Debug.Log(newSpriteName);
									
									// Get all the sprites in the project
									GetAllSprites();
									
									// Make sure we have at least one sprite
									if (sprites.Length > 0) {
										// Loop through all the sprites to get the one matching the new sprite name
										foreach (Sprite sprite in sprites) {
											if (sprite != null && sprite.name == newSpriteName) {
												// Cache the time for this keyframe
												float timeForKey = keyframes[i].time;
												// Create a new ObjectReferenceKeyframe for the sprite
												keyframes[i] = new ObjectReferenceKeyframe();
												// set the time
												keyframes[i].time = timeForKey;
												// set reference for the sprite you want
												keyframes[i].value = sprite;
												// Set the new keyframes to the binding of the animation clip
												AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
												Debug.Log("Sprite changed to " + sprite.name);
												// Break the loop since we already found it
												break;
											}
										}
									}
								}
							}
							// Show the sprite in the editor window
							EditorGUILayout.ObjectField (keyframes[i].value, typeof (Sprite), false);
						}
					}
				}
			}
			// Reset the button if pushed
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

	// Get all the sprites in the project
	void GetAllSprites() {
		// Possible file extensions for the sprite images
		string[] extensions = new string[] {".png",".psd",".jpg",".bmp"};
		// Get all the files with those extensions
		string[] files = AssetDatabase.GetAllAssetPaths().Where(x=>extensions.Contains(System.IO.Path.GetExtension(x))).ToArray();

		// Create a sprite list and add sprites to this list
		List<Sprite> spriteList = new List<Sprite>();
		foreach (string filename in files) {
			Object[] objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(filename);
			spriteList.AddRange(objects.Select(x=>(x as Sprite)).ToList());
		}
		// Convert the list to an array for this window to use
		sprites = spriteList.ToArray();
	}
}