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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum PaintingMode {
    Add, Subtract
}

public class Weightpainter : EditorWindow {
    public SkinnedMeshRenderer skin;
    private bool isPainting = false;
    private bool isDrawing = false;
    private float brushSize = 0.5f;
    private float weight = 1.0f;
    private PaintingMode mode = PaintingMode.Add;
    private int bone = 0;

    [MenuItem("Window/Sprites/Weight painting")]
    protected static void ShowWeightpainterWindow() {
        var wnd = GetWindow<Weightpainter>();
        wnd.title = "Weight painting";

        if (Selection.activeGameObject != null) {
            SkinnedMeshRenderer skin = Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>();
            if (skin != null) {
                wnd.skin = skin;
            }
        }

        SceneView.onSceneGUIDelegate += wnd.OnSceneGUI;
        wnd.Show();
    }

    public void OnDestroy() {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    public void OnGUI() {
        skin = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Skin", skin, typeof(SkinnedMeshRenderer), true);

        if (skin != null) {
            GUI.color = (isPainting) ? Color.green : Color.white;

            if (GUILayout.Button("Paint")) {
                isPainting = !isPainting;
                if (isPainting) {
                    Selection.objects = new GameObject[] { skin.gameObject };
                }
                SceneView.currentDrawingSceneView.Repaint();
            }

            GUI.color = Color.white;

            brushSize = EditorGUILayout.FloatField("Brush size", brushSize * 2) / 2;
            weight = Mathf.Clamp(EditorGUILayout.FloatField("Weight", weight), 0, 1);
            mode = (PaintingMode)EditorGUILayout.EnumPopup("Mode", mode);

            string[] bones = skin.bones.Select(b => b.gameObject.name).ToArray();
            bone = EditorGUILayout.Popup("Bone", bone, bones);

        }
    }

    public void OnSceneGUI(SceneView sceneView) {
        if (skin != null && isPainting) {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Mesh m = skin.sharedMesh.Clone();
            m.colors = CalculateVertexColors(skin.bones, m, skin.bones[bone].GetComponent<Bone>());

            List<BoneWeight> weights = m.boneWeights.ToList();

            Event current = Event.current;

            Graphics.DrawMeshNow(m, skin.transform.position, skin.transform.rotation);

            Bone bn = skin.bones[bone].GetComponent<Bone>();

            foreach (Bone b in skin.GetComponentsInChildren<Bone>()) {
                if (bn == b)
                    Handles.color = Color.yellow;
                else
                    Handles.color = Color.gray;
                Handles.DrawLine(b.transform.position, b.Head);
            }

            Handles.color = Color.red;
            Vector3 mpos = HandleUtility.GUIPointToWorldRay(current.mousePosition).origin;
            mpos = new Vector3(mpos.x, mpos.y);
            Handles.DrawWireDisc(mpos, Vector3.forward, brushSize);

            if (isPainting) {
                if (current.type == EventType.scrollWheel && current.modifiers == EventModifiers.Control) {
                    brushSize = Mathf.Clamp(brushSize + (float)System.Math.Round(current.delta.y / 30, 2), 0, float.MaxValue);
                    Repaint();
                    current.Use();
                } else if (current.type == EventType.mouseDown && current.button == 0) {
                    isDrawing = true;
                } else if (current.type == EventType.mouseUp && current.button == 0) {
                    isDrawing = false;
                } else if (current.type == EventType.mouseDrag && isDrawing && current.button == 0) {
                    float w = weight * ((mode == PaintingMode.Subtract) ? -1 : 1);

                    for (int i = 0; i < m.vertices.Length; i++) {
                        Vector3 v = m.vertices[i];
                        float d = (v - skin.gameObject.transform.InverseTransformPoint(mpos)).magnitude;

                        if (d <= brushSize) {
                            BoneWeight bw = weights[i];
                            float vw = bw.GetWeight(bn.index);
                            vw = Mathf.Clamp(vw + (1 - d / brushSize) * w, 0, 1);
                            bw = bw.SetWeight(bn.index, vw);
                            weights[i] = bw.Clone();
                        }
                    }

                    skin.sharedMesh.boneWeights = weights.ToArray();
                    EditorUtility.SetDirty(skin.gameObject);
                    if (PrefabUtility.GetPrefabType(skin.gameObject) != PrefabType.None) {
                        AssetDatabase.SaveAssets();
                    }
                }
            }

            sceneView.Repaint();
        }
    }

    private Color[] CalculateVertexColors(Transform[] bones, Mesh m, Bone bone) {
        Color[] colors = new Color[m.vertexCount];

        for (int i = 0; i < colors.Length; i++) {
            colors[i] = Color.black;
        }

        if (bones.Any(b => b.gameObject.GetInstanceID() == bone.gameObject.GetInstanceID())) {
            for (int i = 0; i < colors.Length; i++) {
                float value = 0;

                BoneWeight bw = m.boneWeights[i];
                if (bw.boneIndex0 == bone.index)
                    value = bw.weight0;
                else if (bw.boneIndex1 == bone.index)
                    value = bw.weight1;
                else if (bw.boneIndex2 == bone.index)
                    value = bw.weight2;
                else if (bw.boneIndex3 == bone.index)
                    value = bw.weight3;

                colors[i] = Util.HSBColor.ToColor(new Util.HSBColor(0.7f - value, 1.0f, 0.5f));
            }
        }

        return colors;
    }

}
