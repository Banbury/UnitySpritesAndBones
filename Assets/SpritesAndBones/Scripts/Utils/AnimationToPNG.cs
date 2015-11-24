using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/*
The MIT License (MIT)

Copyright (c) 2014 Brad Nelson and Play-Em Inc.

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

// AnimationToPNG is based on Twinfox and bitbutter's Render Particle to Animated Texture Scripts, 
// this script will render out an animation that is played when "Play" is pressed in the editor.

/* 
Basically this is a script you can attach to any gameobject in the 
scene, but you have to reference a Black Camera and White Camera both of 
which should be set to orthographic, but this script should have it 
covered. There is a prefab called AnimationToPNG which should include 
everything needed to run the script. 

If you have Unity Pro, you can use Render Texture, which can accurately 
render the transparent background for your animations easily in full 
resolution of the camera. Just check the box for the variable 
"useRenderTexture" to use RenderTextures instead. If you are using Unity 
Free, then leave this unchecked and you will have a split area using 
half of the screen width to render the animations. 

You can change the "animationName" to a string of your choice for a 
prefix for the output file names, if it is left empty then no filename 
will be added. 

The destination folder is relative to the Project Folder root, so you 
can change the string to a folder name of your choice and it will be 
created. If it already exists, it will simply create a new folder with a 
number incremented as to how many of those named folders exist. 

Choose how many frames per second the animation will run by changing the 
"frameRate" variable, and how many frames of the animation you wish to 
capture by changing the "framesToCapture" variable. 

Once "Play" is pressed in the Unity Editor, it should output all the 
animation frames to PNGs output in the folder you have chosen, and will 
stop capturing after the number of frames you wish to capture is 
completed. 
 */ 

public class AnimationToPNG : MonoBehaviour {

	// Animation Name to be the prefix for the output filenames
	public string animationName = "";

	// Default folder name where you want the animations to be output
	public string folder = "PNG_Animations";

	// Framerate at which you want to play the animation
	public int frameRate = 25;

	// How many frames you want to capture during the animation
	public int framesToCapture = 25;

	// White Camera
	private Camera whiteCam;

	// Black Camera
	private Camera blackCam;

	// Pixels to World Unit size
	public float pixelsToWorldUnit = 74.48275862068966f;

	// If you have Unity Pro you can use a RenderTexture which will render the full camera width, otherwise it will only render half
	private bool useRenderTexture = false;

	private int videoframe = 0; // how many frames we've rendered

	private float originaltimescaleTime; // track the original time scale so we can freeze the animation between frames

	private string realFolder = ""; // real folder where the output files will be

	private bool done = false; // is the capturing finished?

	private bool readyToCapture = false;  // Make sure all the camera setup is complete before capturing

	private float cameraSize; // Size of the orthographic camera established from the current screen resolution and the pixels to world unit

	private Texture2D texb; // black camera texture

	private Texture2D texw; // white camera texture

	private Texture2D outputtex; // final output texture

	private RenderTexture blackCamRenderTexture; // black camera render texure

	private RenderTexture whiteCamRenderTexture; // white camera render texure

	public void Start () {
	    useRenderTexture = Application.HasProLicense();

		// Set the playback framerate!
		// (real time doesn't influence time anymore)
		Time.captureFramerate = frameRate;

		// Create a folder that doesn't exist yet. Append number if necessary.
		realFolder = folder;
		int count = 1;
		while (Directory.Exists(realFolder)) {
			realFolder = folder + count;
			count++;
		}
		// Create the folder
		Directory.CreateDirectory(realFolder);  

		originaltimescaleTime = Time.timeScale;

		// Force orthographic camera to render out sprites per pixel size designated by pixels to world unit
		cameraSize = Screen.width / ((( Screen.width / Screen.height ) * 2 ) * pixelsToWorldUnit );

        GameObject bc = new GameObject("Black Camera");
        bc.transform.localPosition = new Vector3(0, 0, -1);
	    blackCam = bc.AddComponent<Camera>();
	    blackCam.backgroundColor = Color.black;
		blackCam.orthographic = true;
		blackCam.orthographicSize = cameraSize;
	    blackCam.tag = "MainCamera";

        GameObject wc = new GameObject("White Camera");
        wc.transform.localPosition = new Vector3(0, 0, -1);
        whiteCam = wc.AddComponent<Camera>();
        whiteCam.backgroundColor = Color.white;
        whiteCam.orthographic = true;
		whiteCam.orthographicSize = cameraSize;

		// If not using a Render Texture then set the cameras to split the screen to ensure we have an accurate image with alpha
		if (!useRenderTexture){
			// Change the camera rects to have split on screen to capture the animation properly
			blackCam.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);

			whiteCam.rect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
		}
		// Cameras are set ready to capture!
		readyToCapture = true;
	}

	void Update() {
		// If the capturing is not done and the cameras are set, then Capture the animation
		if(!done && readyToCapture){
			StartCoroutine(Capture());
		}
	}

	void LateUpdate() {
		// When we are all done capturing, clean up all the textures and RenderTextures from the scene
		if (done) {
			DestroyImmediate(texb);
			DestroyImmediate(texw);
			DestroyImmediate(outputtex);

			if (useRenderTexture) {
				//Clean Up
				whiteCam.targetTexture = null;
				RenderTexture.active = null;
				DestroyImmediate(whiteCamRenderTexture);

				blackCam.targetTexture = null;
				RenderTexture.active = null;
				DestroyImmediate(blackCamRenderTexture);
			}
		}
	}

	IEnumerator Capture () {
		if(videoframe < framesToCapture) {
			// name is "realFolder/animationName0000.png"
			// string name = realFolder + "/" + animationName + Time.frameCount.ToString("0000") + ".png";
			string filename = String.Format("{0}/" + animationName + "{1:D04}.png", realFolder, Time.frameCount);

			// Stop time
			Time.timeScale = 0;
			// Yield to next frame and then start the rendering
			yield return new WaitForEndOfFrame();

			// If we are using a render texture to make the animation frames then set up the camera render textures
			if (useRenderTexture){
				//Initialize and render textures
 				blackCamRenderTexture = new RenderTexture(Screen.width,Screen.height,24, RenderTextureFormat.ARGB32);
				whiteCamRenderTexture = new RenderTexture(Screen.width,Screen.height,24, RenderTextureFormat.ARGB32);
 
				blackCam.targetTexture = blackCamRenderTexture;
				blackCam.Render();
				RenderTexture.active = blackCamRenderTexture;
				texb = GetTex2D(true);
 
				//Now do it for Alpha Camera
				whiteCam.targetTexture = whiteCamRenderTexture;
				whiteCam.Render();
				RenderTexture.active = whiteCamRenderTexture;
				texw = GetTex2D(true);
			}
			// If not using render textures then simply get the images from both cameras
			else {
				// store 'black background' image
				texb = GetTex2D(true);

				// store 'white background' image
				texw = GetTex2D(false);
			}

			// If we have both textures then create final output texture
			if (texw && texb) {

				int width = Screen.width;
				int height = Screen.height;

				// If we are not using a render texture then the width will only be half the screen
				if (!useRenderTexture){
					width = width / 2;
				}
				outputtex = new Texture2D(width, height, TextureFormat.ARGB32, false);

				// Create Alpha from the difference between black and white camera renders
				for (int y = 0; y < outputtex.height; ++y) { // each row
					for (int x = 0; x < outputtex.width; ++x) { // each column
						float alpha;
						if (useRenderTexture){
							alpha = texw.GetPixel(x, y).r - texb.GetPixel(x, y).r;
						}
						else {
							alpha = texb.GetPixel(x + width, y).r - texb.GetPixel(x, y).r;
						}
						alpha = 1.0f - alpha;
						Color color;
						if(alpha == 0) {
							color = Color.clear;
						} 
						else {
							color = texb.GetPixel(x, y) / alpha;
						}
						color.a = alpha;
						outputtex.SetPixel(x, y, color);
					}
				}

				// Encode the resulting output texture to a byte array then write to the file
				byte[] pngShot = outputtex.EncodeToPNG();
				#if !UNITY_WEBPLAYER
				File.WriteAllBytes(filename, pngShot);
				#endif

				// Reset the time scale, then move on to the next frame.
				Time.timeScale = originaltimescaleTime;
				videoframe++;
			}

			// Debug.Log("Frame " + name + " " + videoframe);
		}
		else {
			Debug.Log("Complete! " + videoframe + " videoframes rendered (0 indexed)");
			done = true;
		}
	}

    // Get the texture from the screen, render all or only half of the camera
    private Texture2D GetTex2D(bool renderAll) {
        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        if (!renderAll) {
            width = width / 2;
        }

        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        return tex;
    }
}
 