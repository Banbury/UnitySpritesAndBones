/*
The MIT License (MIT)

Copyright (c) 2014 Banbury

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

public class Preferences {
    private static bool loaded = false;

    public static Color boneLeftColor;
    public static Color boneRightColor;

    [PreferenceItem("Sprites&Bones")]
    public static void ShowPreferences() {
        if (!loaded) {
            boneLeftColor = Utils.ColorFromInt(EditorPrefs.GetInt("BoneLeftColor", Color.green.AsInt()));
            boneRightColor = Utils.ColorFromInt(EditorPrefs.GetInt("BoneRightColor", Color.red.AsInt()));
            loaded = true;
        }

        boneLeftColor = EditorGUILayout.ColorField("Left Bone Color", boneLeftColor);
        boneRightColor = EditorGUILayout.ColorField("Right Bone Color", boneRightColor);

        if (GUI.changed) {
            EditorPrefs.SetInt("BoneLeftColor", boneLeftColor.AsInt());
            EditorPrefs.SetInt("BoneRightColor", boneRightColor.AsInt());
        }
    }
}
