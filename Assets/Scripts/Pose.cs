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
