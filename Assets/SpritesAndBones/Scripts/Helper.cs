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

public class Helper : MonoBehaviour
{
	//Enum to select what kind of shape to draw for the helper
	[Tooltip("The shape of this helper.")]
	public HelperType type = HelperType.WireCube;

	//Whether to show a cross in addition to the selected shape
	[Tooltip("If checked, displays a cross (plus sign) on top of the shape selected above.")]
	public bool showCross = false;
	//Size of the cross to show, limited in the range below (further scaled by the transform's scale).	
	//If a custom editor is ever made, this is a good variable to conditionally show.
	[Tooltip("The size of the cross.	Does nothing if Show Cross is unchecked above.")]
	[Range(0.1f, 10.0f)]
	public float crossScale = 1.0f;

	//Whether to show an X in addition to the selected shape
	[Tooltip("If checked, displays an X on top of the shape selected above.")]
	public bool showX = false;
	//Size of the X to show, limited in the range below (further scaled by the transform's scale).	
	//If a custom editor is ever made, this is a good variable to selectively show.
	[Tooltip("The size of the X.	Does nothing if Show X is unchecked above.")]
	[Range(0.1f, 10.0f)]
	public float xScale = 1.0f;

	//Number of sides to draw when the type selected is Polygon.	As this only applies to the Polygon type,
	// it would be nice to conditionally hide it if a custom editor is ever made.
	[Tooltip("If you have selected Polygon, this slider determines the number of sides it has.")]
	[Range(3, 10)]
	public int polygonSides = 3;

	//Whether to lock the size of the helper so zooming in/out does not change its size in the scene view
	[Tooltip("If checked, this helper will stay the same size on your screen, even when zooming in and out. Only works with orthographic cameras.")]
	public bool freezeSize = false;

	//Allows the user to customize the color of the helper
	[Tooltip("The color of this helper's wireframe.")]
	public Color color = Color.green;

	//Used to save the orthographic camera's zoom level, when size freezing is desired.
	private float cameraSize = 0;

#if UNITY_EDITOR
	[MenuItem("Sprites And Bones/Helper")]
	public static void Create()
	{
		GameObject o = new GameObject("Helper");
		Undo.RegisterCreatedObjectUndo(o, "Create helper");
		o.AddComponent<Helper>();
	}


	/*
	 * Draws a cross (or plus sign) at the given position and scale.
	 */
	void DrawCross(Vector3 pos, Vector3 scale)
	{
		//radius, by default set so one full line in the cross is 1 unit length (before scaling).
		Vector3 length = scale * 0.5f;
		//Draws lines equal to 2 length along all three axes.
		Gizmos.DrawLine(new Vector3(pos.x - length.x, pos.y, pos.z), new Vector3(pos.x + length.x, pos.y, pos.z));
		Gizmos.DrawLine(new Vector3(pos.x, pos.y - length.y, pos.z), new Vector3(pos.x, pos.y + length.y, pos.z));
		Gizmos.DrawLine(new Vector3(pos.x, pos.y, pos.z - length.z), new Vector3(pos.x, pos.y, pos.z + length.z));
	}

	/*
	 * Draws an X-shape at the given position and scale.
	 */
	void DrawX(Vector3 pos, Vector3 scale)
	{
		//radius, set so one full line in the X is 1 unit length (approximately).
		Vector3 length = scale * 0.5f * 0.7f;
		Gizmos.DrawLine(new Vector3(pos.x - length.x, pos.y - length.y, pos.z), new Vector3(pos.x + length.x, pos.y + length.y, pos.z));
		Gizmos.DrawLine(new Vector3(pos.x - length.x, pos.y + length.y, pos.z), new Vector3(pos.x + length.x, pos.y - length.y, pos.z));
	}

	/*
	 * Draws an n-sided polygon at the given position and scale.
	 * See http://www.mathopenref.com/coordcirclealgorithm.html for an explanation of the algorithm.
	 */
	void DrawPolygon(Vector3 pos, Vector3 scale, int sides)
	{
		//Delta theta.	This controls the number of times we loop and is therefore a function
		// of 2pi and the number of sides we wish to draw.
		float dt = 2 * Mathf.PI / sides;

		//we will need the very first and very last vertices generated after the loop, so they are saved here.
		Vector3 prevPoint = Vector3.zero;
		Vector3 firstPoint = Vector3.zero;

		//Our main loop.	Each iteration calculates the next vertex position (in 2D space).
		// This could be further improved by taking in a rotation in radians and adding it to both
		// the initialization of t and its terminus, thus effectively rotating our generated polygon
		// (currently all polygons start with the first vertex at [radius, 0] (relative), so our 
		// triangle points to the right, and our square is a diamond).	
		for (float t = 0; t <= 2 * Mathf.PI; t += dt)
		{
			float x = pos.x + (scale.x / 2 * Mathf.Cos(t));
			float y = pos.y + (scale.y / 2 * Mathf.Sin(t));
			Vector3 newPoint = new Vector3(x, y, pos.z);

			//Draws the line from the last generated vertex to the one just calculated. Does not do
			// this the first iteration of the loop, as we only have one vertex to work with at that point.
			if (prevPoint != Vector3.zero)
			{
				Gizmos.DrawLine(prevPoint, newPoint);
			}
			else
			{
				//records the first point generated, as we'll need this to close the gap at the end
				firstPoint = newPoint;
			}

			//Save the newly-generated vertex for the next iteration of the loop.
			prevPoint = newPoint;
		}

		//Closes the gap and completes the polygon.
		Gizmos.DrawLine(prevPoint, firstPoint);
	}

	void OnDrawGizmos()
	{
		//Changes the color if the helper is currently selected.	Would be nice to eventually move the selected color
		// to a static variable somewhere.

		if (gameObject.Equals(Selection.activeGameObject))
		{
			Gizmos.color = Color.yellow;
		}
		else
		{
			Gizmos.color = color;
		}

		//freezeScale will either stay the same or be modified based on the scene camera and the user's input.
		Vector3 freezeScale = transform.localScale;

		//Camera.current is null if the scene camera is not active
		if (Camera.current && Camera.current.orthographic)
		{
			//capture the scene camera's zoom level (this of course assumes we're in 2D mode, or 3D ortho for some reason).
			cameraSize = Camera.current.orthographicSize;

			//multiply the scale factor by the camera's zoom level if the user has checked the box.	This keeps our shape at a 
			//constant size.
			if (freezeSize)
			{
				freezeScale *= cameraSize;
			}
		}


		switch (type)
		{
			//The original four options for helper shapes. Except for the fact that they are 3D (and meshes in some cases),
			// these are already replacable by the use of the new shape types.	They are left in to prevent errors in 
			// other scripts.
			case HelperType.Cube:
				Gizmos.DrawCube(transform.position, freezeScale);
				break;
			case HelperType.Sphere:
				Gizmos.DrawSphere(transform.position, freezeScale.x);
				break;
			case HelperType.WireCube:
				Gizmos.DrawWireCube(transform.position, freezeScale);
				break;
			case HelperType.WireSphere:
				Gizmos.DrawWireSphere(transform.position, freezeScale.x);
				break;

			//ideally a single dot, but nothing is ideal, so it's just several small polygons.
			case HelperType.Dot:
				//we scale by the camerasize regardless of whether or not the user has frozen the size, as the dot is damn tiny
				// and we may as well at least leave it visible.	
				Vector3 dotScale = new Vector3(0.025f, 0.025f, 0.025f) * cameraSize;

				//draws five small diamonds, each smaller than the last.	This looks good enough on-screen, and has the same
				//overhead as drawing a circle, so it's about as good as it can get without a custom icon (which is a waste of time).
				for (int i = 0; i < 5; ++i)
				{
					DrawPolygon(transform.position, dotScale, 4);
					dotScale *= 0.85f;
				}
				break;
			//draws the helper as three axis lines.	The reason this exists as an option is so the user can make an
			// 'asterisk' (cross + X) and have a quickly-adjustable X without fiddling with the actual transform scale.
			case HelperType.Cross:
				DrawCross(transform.position, freezeScale);
				break;
			//draws the helper as an X.	see justification for cross above.
			case HelperType.X:
				DrawX(transform.position, freezeScale);
				break;
			//draws the helper as a circle.	Could replace the wiresphere, while not looking as goofy in 3D to boot.
			case HelperType.Circle:
				//A circle is drawn as a 20-sides polygon (isocagon). It's not perfect, but feel free to increase this number
				// in your unity project if you really want a super-smooth circle.	
				// If a custom editor is ever made, having a smoothness slider for the circle would be nice (but it clutters
				// the already very crowded inspector as it is, so it has been left out).
				DrawPolygon(transform.position, freezeScale, 20);
				break;
			//draws an n-sided polygon.	The inspector limits this arbitrarily to 10 sides, but that's because shapes are
			// already pretty indistiguishable at that point, so a circle may as well be used.
			case HelperType.Polygon:
				DrawPolygon(transform.position, freezeScale, polygonSides);
				break;
			//does not display a base shape.	Probably only useful in cases where hundreds of helpers are in the scene, and even
			// then, it's only really here for completion's sake.
			case HelperType.None:
				break;
		}

		//Draws the additional optional cross and/or X on top of the selected shape.	This allows for a lot of
		// customizable markers and subtle variations.
		if (showCross)
		{
			DrawCross(transform.position, freezeScale * crossScale);
		}
		if (showX)
		{
			DrawX(transform.position, freezeScale * xScale);
		}
	}
#endif
}

public enum HelperType
{
	Cube, Sphere, WireCube, WireSphere, Dot, Cross, X, Circle, Polygon, None
}
