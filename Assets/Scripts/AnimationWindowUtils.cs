using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class AnimationWindowUtils {
    public static Assembly GetUnityEditorAssembly() {
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
}
