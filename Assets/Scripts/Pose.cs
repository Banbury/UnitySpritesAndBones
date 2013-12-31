using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Pose : ScriptableObject {
    public RotationValue[] rotations = {};
    public PositionValue[] positions = {};
    public PositionValue[] targets = {};
}

[Serializable]
public class RotationValue {
    public String name;
    public Quaternion rotation;

    public RotationValue(string name, Quaternion rotation) {
        this.name = name;
        this.rotation = rotation;
    }
}

[Serializable]
public class PositionValue {
    public String name;
    public Vector3 position;

    public PositionValue(string name, Vector3 position) {
        this.name = name;
        this.position = position;
    }
}
