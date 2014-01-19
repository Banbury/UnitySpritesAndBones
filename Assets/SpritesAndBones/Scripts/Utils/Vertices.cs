using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace FarseerPhysics.Common {
    public class Vertices : List<Vector2> {
        public Vertices() {
        }

        public Vertices(int capacity) {
            Capacity = capacity;
        }

        public Vertices(ref Vector2[] vector2) {
            for (int i = 0; i < vector2.Length; i++) {
                Add(vector2[i]);
            }
        }

        public Vertices(IList<Vector2> vertices) {
            for (int i = 0; i < vertices.Count; i++) {
                Add(vertices[i]);
            }
        }

        public int NextIndex(int index) {
            if (index == Count - 1) {
                return 0;
            }
            return index + 1;
        }

        /// <summary>
        /// Gets the previous index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int PreviousIndex(int index) {
            if (index == 0) {
                return Count - 1;
            }
            return index - 1;
        }

        /// <summary>
        /// Gets the signed area.
        /// </summary>
        /// <returns></returns>
        public float GetSignedArea() {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++) {
                int j = (i + 1) % Count;
                area += this[i].x * this[j].y;
                area -= this[i].y * this[j].x;
            }
            area /= 2.0f;
            return area;
        }

        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <returns></returns>
        public float GetArea() {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++) {
                int j = (i + 1) % Count;
                area += this[i].x * this[j].y;
                area -= this[i].y * this[j].x;
            }
            area /= 2.0f;
            return (area < 0 ? -area : area);
        }

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCentroid() {
            // Same algorithm is used by Box2D

            Vector2 c = Vector2.zero;
            float area = 0.0f;

            const float inv3 = 1.0f / 3.0f;
            Vector2 pRef = new Vector2(0.0f, 0.0f);
            for (int i = 0; i < Count; ++i) {
                // Triangle vertices.
                Vector2 p1 = pRef;
                Vector2 p2 = this[i];
                Vector2 p3 = i + 1 < Count ? this[i + 1] : this[0];

                Vector2 e1 = p2 - p1;
                Vector2 e2 = p3 - p1;

                //float D = MathUtils.Cross(e1, e2);
                float D = Vector3.Cross(e1, e2).magnitude;

                float triangleArea = 0.5f * D;
                area += triangleArea;

                // Area weighted centroid
                c += triangleArea * inv3 * (p1 + p2 + p3);
            }

            // Centroid
            c *= 1.0f / area;
            return c;
        }

        /// <summary>
        /// Translates the vertices with the specified vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public void Translate(ref Vector2 vector) {
            for (int i = 0; i < Count; i++)
                this[i] = this[i] + vector;
        }

        /// <summary>
        /// Scales the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public void Scale(ref Vector2 value) {
            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Scale(this[i], value);
        }

        /// <summary>
        /// Rotate the vertices with the defined value in radians.
        /// </summary>
        /// <param name="value">The amount to rotate by in radians.</param>
        public void Rotate(float value) {
            Quaternion rotationMatrix = Quaternion.Euler(0, 0, value);

            for (int i = 0; i < Count; i++)
                this[i] = rotationMatrix * this[i];
        }

        /// <summary>
        /// Assuming the polygon is simple; determines whether the polygon is convex.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvex() {
            bool isPositive = false;

            for (int i = 0; i < Count; ++i) {
                int lower = (i == 0) ? (Count - 1) : (i - 1);
                int middle = i;
                int upper = (i == Count - 1) ? (0) : (i + 1);

                float dx0 = this[middle].x - this[lower].x;
                float dy0 = this[middle].y - this[lower].y;
                float dx1 = this[upper].x - this[middle].x;
                float dy1 = this[upper].y - this[middle].y;

                float cross = dx0 * dy1 - dx1 * dy0;
                // Cross product should have same sign
                // for each vertex if poly is convex.
                bool newIsP = (cross >= 0) ? true : false;
                if (i == 0) {
                    isPositive = newIsP;
                }
                else if (isPositive != newIsP) {
                    return false;
                }
            }
            return true;
        }

        public bool IsCounterClockWise() {
            return (GetSignedArea() > 0.0f);
        }

        /// <summary>
        /// Forces counter clock wise order.
        /// </summary>
        public void ForceCounterClockWise() {
            // the sign of the 'area' of the polygon is all
            // we are interested in.
            float area = GetSignedArea();
            if (area > 0) {
                Reverse();
            }
        }

        /// <summary>
        /// Check for edge crossings
        /// </summary>
        /// <returns></returns>
        public bool IsSimple() {
            for (int i = 0; i < Count; ++i) {
                int iplus = (i + 1 > Count - 1) ? 0 : i + 1;
                Vector2 a1 = new Vector2(this[i].x, this[i].y);
                Vector2 a2 = new Vector2(this[iplus].x, this[iplus].y);
                for (int j = i + 1; j < Count; ++j) {
                    int jplus = (j + 1 > Count - 1) ? 0 : j + 1;
                    Vector2 b1 = new Vector2(this[j].x, this[j].y);
                    Vector2 b2 = new Vector2(this[jplus].x, this[jplus].y);

                    Vector2 temp;

                    if (LineIntersect2(a1, a2, b1, b2, out temp)) {
                        return false;
                    }
                }
            }
            return true;
        }

        private class PolyNode {
            private const int MaxConnected = 32;

            /*
             * Given sines and cosines, tells if A's angle is less than B's on -Pi, Pi
             * (in other words, is A "righter" than B)
             */
            public PolyNode[] connected = new PolyNode[MaxConnected];
            public int nConnected;
            public Vector2 position;

            public PolyNode() {
                nConnected = 0;
            }

            public PolyNode(Vector2 pos) {
                position = pos;
                nConnected = 0;
            }

            private bool IsRighter(float sinA, float cosA, float sinB, float cosB) {
                if (sinA < 0) {
                    if (sinB > 0 || cosA <= cosB) return true;
                    else return false;
                }
                else {
                    if (sinB < 0 || cosA <= cosB) return false;
                    else return true;
                }
            }

            //Fix for obnoxious behavior for the % operator for negative numbers...
            private int remainder(int x, int modulus) {
                int rem = x % modulus;
                while (rem < 0) {
                    rem += modulus;
                }
                return rem;
            }

            public void AddConnection(PolyNode toMe) {
                // Ignore duplicate additions
                for (int i = 0; i < nConnected; ++i) {
                    if (connected[i] == toMe) return;
                }
                connected[nConnected] = toMe;
                ++nConnected;
            }

            public void RemoveConnection(PolyNode fromMe) {
                int foundIndex = -1;
                for (int i = 0; i < nConnected; ++i) {
                    if (fromMe == connected[i]) {
                        //.position == connected[i].position){
                        foundIndex = i;
                        break;
                    }
                }

                --nConnected;
                //printf("nConnected: %d\n",nConnected);
                for (int i = foundIndex; i < nConnected; ++i) {
                    connected[i] = connected[i + 1];
                }
            }

            private void RemoveConnectionByIndex(int index) {
                --nConnected;
                //printf("New nConnected = %d\n",nConnected);
                for (int i = index; i < nConnected; ++i) {
                    connected[i] = connected[i + 1];
                }
            }

            private bool IsConnectedTo(PolyNode me) {
                bool isFound = false;
                for (int i = 0; i < nConnected; ++i) {
                    if (me == connected[i]) {
                        //.position == connected[i].position){
                        isFound = true;
                        break;
                    }
                }
                return isFound;
            }

            public PolyNode GetRightestConnection(PolyNode incoming) {
                if (nConnected == 1) {
                    //b2Assert(false);
                    // Because of the possibility of collapsing nearby points,
                    // we may end up with "spider legs" dangling off of a region.
                    // The correct behavior here is to turn around.
                    return incoming;
                }
                Vector2 inDir = position - incoming.position;

                inDir.Normalize();

                PolyNode result = null;
                for (int i = 0; i < nConnected; ++i) {
                    if (connected[i] == incoming) continue;
                    Vector2 testDir = connected[i].position - position;
                    testDir.Normalize();
                    /*
                    if (testLengthSqr < COLLAPSE_DIST_SQR) {
                        printf("Problem with connection %d\n",i);
                        printf("This node has %d connections\n",nConnected);
                        printf("That one has %d\n",connected[i].nConnected);
                        if (this == connected[i]) printf("This points at itself.\n");
                    }*/

                    float myCos = Vector2.Dot(inDir, testDir);
                    float mySin = Cross(inDir, testDir);
                    if (result != null) {
                        Vector2 resultDir = result.position - position;
                        resultDir.Normalize();
                        float resCos = Vector2.Dot(inDir, resultDir);
                        float resSin = Cross(inDir, resultDir);
                        if (IsRighter(mySin, myCos, resSin, resCos)) {
                            result = connected[i];
                        }
                    }
                    else {
                        result = connected[i];
                    }
                }

                //if (B2_POLYGON_REPORT_ERRORS && result != null)
                //{
                //    printf("nConnected = %d\n", nConnected);
                //    for (int i = 0; i < nConnected; ++i)
                //    {
                //        printf("connected[%d] @ %d\n", i, (int)connected[i]);
                //    }
                //}

                return result;
            }

            public PolyNode GetRightestConnection(Vector2 incomingDir) {
                Vector2 diff = position - incomingDir;
                PolyNode temp = new PolyNode(diff);
                PolyNode res = GetRightestConnection(temp);

                return res;
            }
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Count; i++) {
                builder.Append(this[i].ToString());
                if (i < Count - 1) {
                    builder.Append(" ");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Projects to axis.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        public void ProjectToAxis(ref Vector2 axis, out float min, out float max) {
            // To project a point on an axis use the dot product
            float dotProduct = Vector2.Dot(axis, this[0]);
            min = dotProduct;
            max = dotProduct;

            for (int i = 0; i < Count; i++) {
                dotProduct = Vector2.Dot(this[i], axis);
                if (dotProduct < min) {
                    min = dotProduct;
                }
                else {
                    if (dotProduct > max) {
                        max = dotProduct;
                    }
                }
            }
        }

        public static float Cross(Vector2 a, Vector2 b) {
            return a.x * b.y - a.y * b.x;
        }

        public static bool LineIntersect2(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out Vector2 intersectionPoint) {
            intersectionPoint = Vector2.zero;

            if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1)
                return false;

            float x1 = a0.x;
            float y1 = a0.y;
            float x2 = a1.x;
            float y2 = a1.y;
            float x3 = b0.x;
            float y3 = b0.y;
            float x4 = b1.x;
            float y4 = b1.y;

            //AABB early exit
            if (Math.Max(x1, x2) < Math.Min(x3, x4) || Math.Max(x3, x4) < Math.Min(x1, x2))
                return false;

            if (Math.Max(y1, y2) < Math.Min(y3, y4) || Math.Max(y3, y4) < Math.Min(y1, y2))
                return false;

            float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3));
            float ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3));
            float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            if (Math.Abs(denom) < 1.192092896e-07f) {
                //Lines are too close to parallel to call
                return false;
            }
            ua /= denom;
            ub /= denom;

            if ((0 < ua) && (ua < 1) && (0 < ub) && (ub < 1)) {
                intersectionPoint.x = (x1 + ua * (x2 - x1));
                intersectionPoint.y = (y1 + ua * (y2 - y1));
                return true;
            }

            return false;
        }
    }
}