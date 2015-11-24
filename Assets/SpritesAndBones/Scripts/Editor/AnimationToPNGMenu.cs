using UnityEngine;
using UnityEditor;
using System.Collections;

public class AnimationToPNGMenu  {
    [MenuItem("Sprites And Bones/Animation To PNG")]
    public static void Create() {
        GameObject o = new GameObject("Animation To PNG");
        Undo.RegisterCreatedObjectUndo(o, "Create skeleton");
        o.AddComponent<AnimationToPNG>();
    }
}
