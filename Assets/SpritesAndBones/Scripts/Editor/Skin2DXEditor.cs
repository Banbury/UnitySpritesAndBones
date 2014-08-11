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
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Skin2DX))]
public class Skin2DXEditor : Editor {
    private Skin2DX skin;

	void OnEnable() {
	    skin = target as Skin2DX;
        skin.GetComponent<SkinnedMeshRenderer>().hideFlags = HideFlags.HideInInspector;
	}

    public override void OnInspectorGUI() {
        skin.locked = EditorGUILayout.Toggle("Locked", skin.locked);

        if (!skin.locked) {
            DrawDefaultInspector();

            if (GUILayout.Button("Regenerate")) {
                skin.BuildMesh();
            }

            if (GUI.changed) {
                skin.BuildMesh();
                EditorUtility.SetDirty(skin);
            }
        }
    }
}
