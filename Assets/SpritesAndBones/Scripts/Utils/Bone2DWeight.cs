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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Bone2DWeights {
    public Bone2DWeight[] weights = new Bone2DWeight[] {};

    public Bone2DWeight[] this[string name] {
        get {
            return weights.Where(b => b.boneName == name).ToArray();
        }
    }

    public int Count {
        get {
            return weights.Length;
        }
    }

    public string[] GetBones() {
        return weights.Select(b => b.boneName).ToArray();
    }

    public float GetWeight(int vertex, string bone) {
        return weights.Where(b => b.boneName == bone && b.vertex == vertex).First().weight;
    }

    public void SetWeight(int vertex, string bone, int index, float weight) {
        Bone2DWeight bw = weights.Where(b => b.boneName == bone && b.vertex == vertex).FirstOrDefault();
        if (bw != null) {
            bw.weight = weight;
        }
        else {
            bw = new Bone2DWeight(bone, index, vertex, weight);

            List<Bone2DWeight> w = new List<Bone2DWeight>(weights);
            w.Add(bw);
            weights = w.ToArray();
        }
    }

    public BoneWeight[] GetUnityBoneWeights() {
        List<BoneWeight> bweights = new List<BoneWeight>();

        var groups = weights.GroupBy(bw => bw.vertex);

        foreach (var g in groups) {
            var wv = g.Where(w => w.weight > 0).ToList();

            float max = wv.Sum(bw => bw.weight);

            if (max > 1.0f) {
                wv.ForEach(bw => bw.weight = bw.weight / max);
            }

            int i = 0;

            BoneWeight bweight = new BoneWeight(); 

            foreach (Bone2DWeight w in wv) {
                switch (i) {
                    case 0:
                        bweight.boneIndex0 = w.index;
                        bweight.weight0 = w.weight;
                        break;
                    case 1:
                        bweight.boneIndex1 = w.index;
                        bweight.weight1 = w.weight;
                        break;
                    case 2:
                        bweight.boneIndex2 = w.index;
                        bweight.weight2 = w.weight;
                        break;
                    case 3:
                        bweight.boneIndex3 = w.index;
                        bweight.weight3 = w.weight;
                        break;
                }
                i++;
            }

            bweights.Add(bweight);
        }

        return bweights.ToArray();
    }
}

[System.Serializable]
public class Bone2DWeight {
    public int index;
    public string boneName;
    public int vertex;
    public float weight;

    public Bone2DWeight(string name, int index, int vertex, float weight) {
        this.boneName = name;
        this.index = index;
        this.vertex = vertex;
        this.weight = weight;
    }

    public override string ToString() {
        return string.Format("{0}: {1} {2} {3}", index, boneName, vertex, weight);
    }
}
