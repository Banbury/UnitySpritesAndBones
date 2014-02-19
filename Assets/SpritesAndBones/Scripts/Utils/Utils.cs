using UnityEngine;

public static class Utils {
    public static float ClampAngle(float x) {
        return x - 360 * Mathf.Floor(x / 360);
    }

    public static float GetWeight(this BoneWeight bw, int index) {
        if (bw.boneIndex0 == index && bw.weight0 > 0) {
            return bw.weight0;
        }
        if (bw.boneIndex1 == index && bw.weight1 > 0) {
            return bw.weight1;
        }
        if (bw.boneIndex2 == index && bw.weight2 > 0) {
            return bw.weight2;
        }
        if (bw.boneIndex3 == index && bw.weight3 > 0) {
            return bw.weight3;
        }
        return 0;
    }

    public static BoneWeight SetWeight(this BoneWeight bw, int index, float value) {
        if (bw.boneIndex0 == index || bw.weight0 == 0) {
            bw.boneIndex0 = index;
            bw.weight0 = value;
        }
        else if (bw.boneIndex1 == index || bw.weight1 == 0) {
            bw.boneIndex1 = index;
            bw.weight1 = value;
        }
        else if (bw.boneIndex2 == index || bw.weight2 == 0) {
            bw.boneIndex2 = index;
            bw.weight2 = value;
        }
        else if (bw.boneIndex3 == index || bw.weight3 == 0) {
            bw.boneIndex3 = index;
            bw.weight3 = value;
        }
        else {
            bw.boneIndex0 = index;
            bw.weight0 = value;
        }

        float max = bw.weight0 + bw.weight1 + bw.weight2 + bw.weight3;
        if (max > 1) {
            bw.weight0 /= max;
            bw.weight1 /= max;
            bw.weight2 /= max;
            bw.weight3 /= max;
        }

        return bw;
    }

    public static void Log(this BoneWeight bw) {
        Debug.Log(
            "Index0: " + bw.boneIndex0 + " Weight0: " + bw.weight0 + "\n" +
            "Index1: " + bw.boneIndex1 + " Weight1: " + bw.weight1 + "\n" +
            "Index2: " + bw.boneIndex2 + " Weight2: " + bw.weight2 + "\n" +
            "Index3: " + bw.boneIndex3 + " Weight3: " + bw.weight3
            );
    }

    public static Mesh Clone(this Mesh m) {
        Mesh copy = new Mesh();

        copy.vertices = m.vertices;
        copy.triangles = m.triangles;
        copy.normals = m.normals;
        copy.bindposes = m.bindposes;
        copy.bounds = m.bounds;
        copy.uv = m.uv;
        copy.uv2 = m.uv2;
        copy.boneWeights = m.boneWeights;
        //copy.colors = m.colors;
        copy.tangents = m.tangents;

        return copy;
    }

    public static BoneWeight Clone(this BoneWeight bw) {
        BoneWeight ret = new BoneWeight();
        ret.boneIndex0 = bw.boneIndex0;
        ret.boneIndex1 = bw.boneIndex1;
        ret.boneIndex2 = bw.boneIndex2;
        ret.boneIndex3 = bw.boneIndex3;
        ret.weight0 = bw.weight0;
        ret.weight1 = bw.weight1;
        ret.weight2 = bw.weight2;
        ret.weight3 = bw.weight3;

        return ret;
    }
}
