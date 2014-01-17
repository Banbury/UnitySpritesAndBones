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
using System.Collections.Generic;
using System.Threading;

[ExecuteInEditMode]
[RequireComponent(typeof(Bone))]
public class InverseKinematics : MonoBehaviour {
    [HideInInspector]
    public float influence = 1.0f;
    public int chainLength = 0;
    public Transform target;

    public Transform RootBone {
        get {
            Transform root = null;

            if (chainLength == 0) {
                root = transform.root;
            }
            else {
                int n = chainLength;
                root = transform;
                while (n-- > 0) {
                    if (root.parent == null)
                        break;
                    else
                        root = root.parent;
                }
            }
            return root;
        }
    }

    private int ChainLength {
        get {
            if (chainLength > 0)
                return chainLength;
            else {
                int n = 0;
                var parent = transform.parent;
                while (parent != null && parent.gameObject.GetComponent<Bone>() != null) {
                    n++;
                    parent = parent.parent;
                }
                return n+1;
            }
        }
    }

	// Use this for initialization
	void Start () {
	}

    void Update() {
        if (chainLength < 0)
            chainLength = 0;
    }

	/**
     * Code ported from the Gamemaker tool SK2D by Drifter
     * http://gmc.yoyogames.com/index.php?showtopic=462301
     * 
     **/
    public void resolveSK2D() {
        Transform tip = transform;

        int iterations = 20;

        for (int it = 0; it < iterations; it++) {
            int i = ChainLength;
            Transform bone = transform;
            Bone b = bone.GetComponent<Bone>();

            while (--i >= 0 && bone != null) {
                Vector3 root = bone.position;

                Vector3 root2tip = (tip.position + tip.up * b.length - root).normalized;
                Vector3 root2target = (((target != null) ? target.transform.position : (Vector3)b.Head) - root).normalized;

                float cosangle = Vector3.Dot(root2target, root2tip);

                if (cosangle < 0.99999f) {
                    Vector3 dir = Vector3.Cross(root2target, root2tip);

                    float turn = ((dir.z < 0) ? 1 : -1) * Mathf.Acos(cosangle) * Mathf.Rad2Deg;

                    float yAngle = Utils.ClampAngle(bone.rotation.eulerAngles.y);
                    if (yAngle > 90 && yAngle < 270)
                        turn *= -1;

                    bone.localRotation = Quaternion.Euler(0, 0, bone.localRotation.eulerAngles.z + turn);
                }

                bone = bone.parent;
            }
        }
    }
}
