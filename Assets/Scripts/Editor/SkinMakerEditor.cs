using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SkinMaker))]
public class SkinMakerEditor : Editor {
    public override void OnInspectorGUI() {
        SkinMaker skin = (SkinMaker)target;

        EditorGUI.BeginChangeCheck();
        bool edit = EditorGUILayout.Toggle("Edit", skin.edit);
        if (EditorGUI.EndChangeCheck()) {
            skin.SetEditMode(edit);
        }

        if (edit) {
            EditorGUI.BeginChangeCheck();
            var image = (Texture)EditorGUILayout.ObjectField("Image", skin.image, typeof(Texture), false);

            if (EditorGUI.EndChangeCheck()) {
                skin.SetImage(image);
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate mesh")) {
                skin.GenerateMesh();
            }

            if (GUILayout.Button("Subdivide mesh")) {
                skin.SubdivideMesh();
            }

            if (GUILayout.Button("Save mesh")) {
                ScriptableObjectUtility.CreateAsset(skin.mesh);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    void OnSceneGUI() {
        EventType evt = Event.current.type;

        SkinMaker skin = (SkinMaker)target;

        List<Vector3> ctlp = skin.controlPoints.Select(v => v + skin.transform.position).ToList();
        ctlp.Add(skin.controlPoints[0] + skin.transform.position);
        Vector3[] poly = ctlp.ToArray();

        Handles.color = Color.gray;
        Handles.DrawAAPolyLine(poly);

        Vector3 p = HandleUtility.ClosestPointToPolyLine(poly);
        int selCtrl = -1;

        Handles.color = Color.green;

        for (int i = 0; i < skin.controlPoints.Length; i++) {
            Vector3 hp = Handles.FreeMoveHandle(skin.transform.position + skin.controlPoints[i], Quaternion.identity, 0.05f, Vector3.zero, Handles.RectangleCap);
            if (Vector3.Distance(hp, p) < 0.05f) {
                selCtrl = i;
            }
            hp = hp - skin.transform.position;
            skin.controlPoints[i] = new Vector3(Mathf.Clamp(hp.x, -1, 1), Mathf.Clamp(hp.y, -1, 1), Mathf.Clamp(hp.z, -1, 1));
        }

        if (Event.current.shift) {
            Handles.color = Color.blue;
            Handles.DrawWireDisc(p, Vector3.forward, 0.05f);

            if (evt == EventType.mouseDown) {
                Undo.RecordObject(skin, "Add handle");
                for (int i = 0; i < poly.Length - 1; i++) {
                    float d = (float)System.Math.Round(HandleUtility.DistancePointLine(p, ctlp[i], ctlp[i+1]), 4);
                    if (d == 0) {
                        ctlp = new List<Vector3>(skin.controlPoints);
                        ctlp.Insert(i+1, p - skin.transform.position);
                        skin.controlPoints = ctlp.ToArray();
                    }
                }
            }
        }
        else if (Event.current.control) {
            if (evt == EventType.mouseDown) {
                Undo.RecordObject(skin, "Remove handle");
                if (selCtrl != -1) {
                    if (skin.controlPoints.Length > 4) {
                        ctlp = new List<Vector3>(skin.controlPoints);
                        ctlp.RemoveAt(selCtrl);
                        skin.controlPoints = ctlp.ToArray();
                    }
                    else {
                        EditorUtility.DisplayDialog("Cannot delete control point", "There have to be at least 4 control points.", "Ok");
                    }
                }
            }
        }
    }
}
