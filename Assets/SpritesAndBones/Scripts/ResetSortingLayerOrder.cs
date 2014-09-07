/*
The MIT License (MIT)

Copyright (c) 2014 Banbury & Play-Em

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

public class ResetSortingLayerOrder : MonoBehaviour {

    [MenuItem("Sprites And Bones/Reset Sorting Order")]

	public static void Reset ()
	{
        #if UNITY_EDITOR
		if (Selection.activeGameObject != null) {
			GameObject o = Selection.activeGameObject;
			if (o.renderer != null)
			{
				o.renderer.sortingLayerName = "Default";
				o.renderer.sortingOrder = 0;
			}
			Transform[] children = o.GetComponentsInChildren<Transform>(true);
			foreach(Transform child in children) {
				if (child.gameObject.renderer != null)
				{
					child.gameObject.renderer.sortingLayerName = "Default";
					child.gameObject.renderer.sortingOrder = 0;
				}
			}
		}
		#endif
    }

}
