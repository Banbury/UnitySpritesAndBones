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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class AnimationWindowUtils {
#if UNITY_EDITOR
		public static Assembly GetUnityEditorAssembly ()
		{
				return typeof(AnimationUtility).Assembly;
		}
    

    public static object GetAnimationWindow() {
        Type animationWindow = GetUnityEditorAssembly().GetType("UnityEditor.AnimationWindow");
        MethodInfo mi = animationWindow.GetMethod("GetAllAnimationWindows", BindingFlags.Static | BindingFlags.Public);
        IEnumerable windows = (IEnumerable)mi.Invoke(null, null);
        if (windows != null) {
            IEnumerator enu = windows.GetEnumerator();
            if (enu.MoveNext()) {
                return enu.Current;
            }
        }
        return null;
    }

    public static float GetCurrentTime(object animationWindow) {
        Type animationWindowType = GetUnityEditorAssembly().GetType("UnityEditor.AnimationWindow");
        PropertyInfo pi = animationWindowType.GetProperty("time");
        return (float)pi.GetValue(animationWindow, null);
    }

    public static string GetPath(this Transform current) {
        if (current.parent == null)
            return "/" + current.name;
        return current.parent.GetPath() + "/" + current.name;
    }

    public static string GetPath(this Component component) {
        return component.transform.GetPath() + "/" + component.GetType().ToString();
    }

    public static string GetPath(this GameObject gameObject) {
        return gameObject.transform.GetPath();
    }
#endif
}
