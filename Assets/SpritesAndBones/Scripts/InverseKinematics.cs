/*
The MIT License (MIT)

Copyright (c) 2014 Banbury & Play-Em & SirKurt

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
	private Skeleton _skeleton = null;

	public Skeleton skeleton {
		get { return _skeleton; }
		set { skeleton = value; }
	}

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

	public int iterations = 20;

	[Range(0.01f, 1)]
	public float damping = 1;

	public Node[] angleLimits = new Node[0];

	Dictionary<Transform, Node> nodeCache; 
	[System.Serializable]
	public class Node
	{
		public Transform Transform;
		[Range(0,360)]
		public float from;
		[Range(0,360)]
		public float to;
	}

	void Start()
	{
		// Cache optimization
		if (angleLimits.Length > 0 && nodeCache != null && nodeCache.Count != angleLimits.Length ||
		angleLimits.Length > 0 && nodeCache == null){
			nodeCache = new Dictionary<Transform, Node>(angleLimits.Length);
			foreach (var node in angleLimits)
				if (!nodeCache.ContainsKey(node.Transform))
					nodeCache.Add(node.Transform, node);
		}
		if (_skeleton == null || _skeleton != null && !transform.IsChildOf(_skeleton.transform)) {
			Skeleton[] skeletons = transform.root.gameObject.GetComponentsInChildren<Skeleton>(true);
			foreach (Skeleton s in skeletons)
			{
				if (transform.IsChildOf(s.transform))
				{
					_skeleton = s;
					break;
				}
			}
		}
	}

    void Update() {
        if (chainLength < 0)
            chainLength = 0;
		if (!Application.isPlaying)
			Start();
    }

	/**
     * Code ported from the Gamemaker tool SK2D by Drifter
     * http://gmc.yoyogames.com/index.php?showtopic=462301
     * Angle Limit code adapted from Veli-Pekka Kokkonen's SimpleCCD http://goo.gl/6oSzDx
     **/
    public void resolveSK2D() {
        for (int it = 0; it < iterations; it++) {
            int i = ChainLength;
            Transform bone = transform;
            Bone b = bone.GetComponent<Bone>();

            while (--i >= 0 && bone != null) {
                Vector3 root = bone.position;

                // Z position can be different than 0
                Vector3 root2tip = ((Vector3)b.Head - (Vector3)(Vector2)root);
                Vector3 root2target = (((target != null) ? (Vector3)(Vector2)target.transform.position : (Vector3)b.Head) - (Vector3)(Vector2)root);

				// Calculate how much we should rotate to get to the target
				float angle = SignedAngle(root2tip, root2target, bone);

				// If you want to flip the bone on the y axis invert the angle
				float yAngle = Utils.ClampAngle(bone.rotation.eulerAngles.y);

				// If the skeleton is rotated then make sure the angle is modified accordingly
				int yModifier = (_skeleton && _skeleton.transform.localRotation.eulerAngles.y == 180.0f 
								&& _skeleton.transform.localRotation.eulerAngles.x == 0.0f) ? 1 : -1;

				if (yAngle > 90 && yAngle < 270)
				angle *= yModifier;

				// "Slows" down the IK solving
				angle *= damping;

				// Wanted angle for rotation
				angle = -(angle - bone.localRotation.eulerAngles.z);

                if(nodeCache != null && nodeCache.ContainsKey(bone))
                {
                    // Clamp angle in local space
                    var node = nodeCache[bone];
                    angle = ClampAngle(angle, node.from, node.to);
                }

				Quaternion newRotation = Quaternion.Euler(bone.localRotation.eulerAngles.x, bone.localRotation.eulerAngles.y, angle);

				if (!IsNaNRot(newRotation)) {
					bone.localRotation = newRotation;
				}

                bone = bone.parent;
            }
        }
    }

	public float SignedAngle (Vector3 a, Vector3 b, Transform t)
	{
		float angle = Vector3.Angle (a, b);

		// Use skeleton as root, change dir if the rotation is flipped
		Vector3 dir = (_skeleton && _skeleton.transform.localRotation.eulerAngles.y == 180.0f && _skeleton.transform.localRotation.eulerAngles.x == 0.0f) ? Vector3.forward : Vector3.back;
		float sign = Mathf.Sign (Vector3.Dot (dir, Vector3.Cross (a, b)));
		angle = angle * sign;
		// Flip sign if character is turned around
		angle *= Mathf.Sign(t.root.localScale.x);
		return angle;
	}

	float ClampAngle(float angle, float from, float to)
    	{
	        angle = Mathf.Abs((angle % 360) + 360) % 360;
	
	        //Check limits
	        if (from > to && (angle > from || angle < to))
	            return angle;
	        else if (to > from && (angle < to && angle > from))
	            return angle;
	
	        //Return nearest limit if not in bounds
	        return (Mathf.Abs(angle - from) < Mathf.Abs(angle - to) 
				&& Mathf.Abs(angle - from) < Mathf.Abs((angle + 360) - to)) 
				|| (Mathf.Abs(angle - from - 360) < Mathf.Abs(angle - to) 
				&& Mathf.Abs(angle - from - 360) < Mathf.Abs((angle + 360) - to)) ? from : to;
    	}

	private bool IsNaNRot(Quaternion q) 
	{
		return (float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w));
	}
}
