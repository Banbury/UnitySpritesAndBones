using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Pose))]
public class PoseEditor : Editor {
    private Skeleton previewSkeleton = null;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        GUILayout.Label("Rotations: " + ((Pose)target).rotations.Length);
    }

    public override bool HasPreviewGUI() {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background) {
        Pose pose = (Pose)target;

        if (previewSkeleton != null) {
            var bones = previewSkeleton.GetComponentsInChildren<Bone>();

            GL.Begin(GL.LINES);

            foreach (Bone b in bones) {
                PositionValue pv = Array.Find(pose.positions, x => x.name == b.name);

                if (pv != null) {
                    RotationValue rv = Array.Find(pose.rotations, x => x.name == b.name);

                    Vector3 v = pv.position + (rv.rotation * new Vector3(0, 1, 0)) * b.length * 40;

                    GL.Color(new Color(1, 0, 0, 1));
                    GL.Vertex(pv.position * 40 + (Vector3)r.center);
                    GL.Vertex(v + (Vector3)r.center);
                }
            }

            GL.End();
        }

        //GL.Begin(GL.LINES);

        //GameObject go = GameObject.Find("orc_3");
        //SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        //Texture2D tex = sr.sprite.texture;
        //Sprite spr = sr.sprite;
        //Rect source = new Rect(spr.rect.x / tex.width, spr.rect.y / tex.height, spr.rect.width / tex.width, spr.rect.height / tex.height);
        
        //GL.Color(Color.red);
        //GL.Vertex(new Vector3(r.x, r.y, 0));
        //GL.Vertex(new Vector3(r.x + r.width, r.y + r.height, 0));

        //GL.End();
        //Graphics.DrawTexture(r, tex, source, 0, 0, 0, 0, sr.sharedMaterial);

    }

    public override void OnPreviewSettings() {
        previewSkeleton = (Skeleton)EditorGUILayout.ObjectField("Skeleton", previewSkeleton, typeof(Skeleton), true, null);
    }
}
