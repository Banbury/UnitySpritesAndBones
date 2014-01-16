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

public class Helper : MonoBehaviour {
    public HelperType type = HelperType.WireCube;

    #if UNITY_EDITOR
		[MenuItem("GameObject/Create Other/Helper")]
		public static void Create ()
		{
				GameObject o = new GameObject ("Helper");
				Undo.RegisterCreatedObjectUndo (o, "Create helper");
				o.AddComponent<Helper> ();
		}

		void OnDrawGizmos ()
		{
				if (gameObject.Equals (Selection.activeGameObject)) {
						Gizmos.color = Color.yellow;
				} else {
						Gizmos.color = Color.green;
				}

				switch (type) {
				case HelperType.Cube:
						Gizmos.DrawCube (transform.position, transform.localScale);
						break;
				case HelperType.Sphere:
						Gizmos.DrawSphere (transform.position, transform.localScale.x);
						break;
				case HelperType.WireCube:
						Gizmos.DrawWireCube (transform.position, transform.localScale);
						break;
				case HelperType.WireSphere:
						Gizmos.DrawWireSphere (transform.position, transform.localScale.x);
						break;
				}
		}
    #endif
}

public enum HelperType {
    Cube, Sphere, WireCube, WireSphere
}
