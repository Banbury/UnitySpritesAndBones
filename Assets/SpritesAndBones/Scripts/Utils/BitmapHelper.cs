#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion




#if UseDouble
using Scalar = System.Double;
#else
using Scalar = System.Single;
#endif

using UnityEngine;
using System;
using System.Collections.Generic;


public interface IBitmap {
    int Width { get; }
    int Height { get; }
    bool this[int x, int y] { get; }
}

public sealed class ArrayBitmap : IBitmap {
    private bool[,] bitmap;

    public ArrayBitmap(bool[,] bitmap) {
        if (bitmap == null) { throw new ArgumentNullException("bitmap"); }
        this.bitmap = bitmap;
    }

    public int Width {
        get { return bitmap.GetLength(0); }
    }

    public int Height {
        get { return bitmap.GetLength(1); }
    }

    public bool this[int x, int y] {
        get {
            return
                x >= 0 && x < Width &&
                y >= 0 && y < Height &&
                bitmap[x, y];
        }
    }

    public static IBitmap CreateFromTexture(Texture2D texture, Rect rect) {
        var bmp = new bool[texture.width, texture.height];

        Color[] pixels = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);

        int x = 0;
        int y = 0;
        int w = (int)rect.width;

        for (int i = 0; i < pixels.Length; i++) {
            y = Mathf.FloorToInt(i / w);
            x = (i - y * w);
            bmp[x, y] = (pixels[i].a != 0);
        }

        return new ArrayBitmap(bmp);
    }

}

public static class BitmapHelper {
    class BitMapSkipper {
        float xMin;
        float xMax;
        List<float>[] scans;


        public BitMapSkipper(IBitmap bitmap, List<Vector2> points) {
            FromVectors(points);
            CreateScans();
            FillScans(points);
            FormatScans(bitmap);
        }

        void FromVectors(List<Vector2> points) {
            if (points == null) { throw new ArgumentNullException("vectors"); }
            if (points.Count == 0) { throw new ArgumentOutOfRangeException("points"); }
            xMin = points[0].x;
            xMax = xMin;
            for (int index = 1; index < points.Count; ++index) {
                Vector2 current = points[index];
                if (current.x > xMax) {
                    xMax = current.x;
                }
                else if (current.x < xMin) {
                    xMin = current.x;
                }
            }
        }
        void CreateScans() {
            scans = new List<float>[(int)(xMax - xMin) + 1];
            for (int index = 0; index < scans.Length; ++index) {
                scans[index] = new List<float>();
            }
        }
        void FillScans(List<Vector2> points) {
            for (int index = 0; index < points.Count; ++index) {
                Vector2 point = points[index];
                int scanIndex = (int)(point.x - xMin);
                scans[scanIndex].Add(point.y);
            }
        }

        void FormatScans(IBitmap bitmap) {
            for (int index = 0; index < scans.Length; ++index) {
                scans[index].Sort();
                FormatScan(bitmap, index);
            }
        }
        void FormatScan(IBitmap bitmap, int x) {
            List<float> scan = scans[x];
            List<float> newScan = new List<float>();
            bool inPoly = false;
            for (int index = 0; index < scan.Count; ++index) {
                float y = scan[index];
                if (!inPoly) {
                    newScan.Add(y);
                }
                bool value = bitmap[(int)(x + xMin), (int)y + 1];
                if (value) {
                    inPoly = true;
                }
                else {
                    newScan.Add(y);
                    inPoly = false;
                }
            }
            //if (newScan.Count % 2 != 0) { throw new Exception(); }
            scans[x] = newScan;
        }

        public bool TryGetSkip(Vector2 point, out float nextY) {
            int scanIndex = (int)(point.y - xMin);
            if (scanIndex < 0 || scanIndex >= scans.Length) {
                nextY = 0;
                return false;
            }
            List<float> scan = scans[scanIndex];
            for (int index = 1; index < scan.Count; index += 2) {
                if (point.y >= scan[index - 1] &&
                    point.y <= scan[index]) {
                    nextY = scan[index];
                    return true;
                }
            }
            nextY = 0;
            return false;
        }
    }
    static readonly Vector2[] bitmapPoints = new Vector2[]{
            new Vector2 (1,1),
            new Vector2 (0,1),
            new Vector2 (-1,1),
            new Vector2 (-1,0),
            new Vector2 (-1,-1),
            new Vector2 (0,-1),
            new Vector2 (1,-1),
            new Vector2 (1,0),
        };

    public static Vector2[] CreateFromBitmap(IBitmap bitmap) {
        return Reduce(CreateFromBitmap(bitmap, GetFirst(bitmap)));
    }

    public static Vector2[][] CreateManyFromBitmap(IBitmap bitmap) {
        List<BitMapSkipper> skippers = new List<BitMapSkipper>();
        List<Vector2[]> result = new List<Vector2[]>();
        foreach (Vector2 first in GetFirsts(bitmap, skippers)) {
            List<Vector2> points = CreateFromBitmap(bitmap, first);
            BitMapSkipper skipper = new BitMapSkipper(bitmap, points);
            skippers.Add(skipper);
            result.Add(Reduce(points));
        }
        return result.ToArray();
    }

    private static List<Vector2> CreateFromBitmap(IBitmap bitmap, Vector2 first) {
        Vector2 current = first;
        Vector2 last = first - new Vector2(0, 1);
        List<Vector2> result = new List<Vector2>();
        
        do {
            result.Add(current);
            current = GetNextVertex(bitmap, current, last);
            last = result[result.Count - 1];
        } while (current != first);

        if (result.Count < 3) { 
            throw new ArgumentException("The image has an area with less then 3 pixels (possibly an artifact).", "bitmap"); 
        }

        return result;
    }

    private static Vector2 GetFirst(IBitmap bitmap) {
        for (int x = bitmap.Width - 1; x > -1; --x) {
            for (int y = 0; y < bitmap.Height; ++y) {
                if (bitmap[x, y]) {
                    return new Vector2(x, y);
                }
            }
        }
        throw new ArgumentException("TODO", "bitmap");
    }

    private static IEnumerable<Vector2> GetFirsts(IBitmap bitmap, List<BitMapSkipper> skippers) {
        for (int x = bitmap.Width - 1; x > -1; --x) {
            for (int y = 0; y < bitmap.Height; ++y) {
                if (bitmap[x, y]) {
                    bool contains = false;
                    Vector2 result = new Vector2(x, y);
                    for (int index = 0; index < skippers.Count; ++index) {
                        float nextY;
                        if (skippers[index].TryGetSkip(result, out nextY)) {
                            contains = true;
                            y = (int)nextY;
                            break;
                        }
                    }
                    if (!contains) {
                        yield return result;
                    }
                }
            }
        }
    }

    private static Vector2 GetNextVertex(IBitmap bitmap, Vector2 current, Vector2 last) {
        int offset = 0;
        Vector2 point;

        for (int index = 0; index < bitmapPoints.Length; ++index) {
            point = current + bitmapPoints[index];
            if (point == last) {
                offset = index + 1;
                break;
            }
        }

        for (int index = 0; index < bitmapPoints.Length; ++index) {
            point = current + bitmapPoints[(index + offset) % bitmapPoints.Length];
            if (point.x >= 0 && point.x < bitmap.Width &&
                point.y >= 0 && point.y < bitmap.Height &&
                bitmap[(int)point.x, (int)point.y]) {
                return point;
            }
        }

        throw new ArgumentException("The image has an area with less then 3 pixels (possibly an artifact).", "bitmap");
    }

    private static Vector2[] Reduce(List<Vector2> list) {
        List<Vector2> result = new List<Vector2>(list.Count);
        Vector2 p1 = list[list.Count - 2];
        Vector2 p2 = list[list.Count - 1];
        Vector2 p3;
        for (int index = 0; index < list.Count; ++index, p2 = p3) {
            if (index == list.Count - 1) {
                if (result.Count == 0) { throw new ArgumentException("Bad Polygon"); }
                p3.x = (int)result[0].x;
                p3.y = (int)result[0].y;
            }
            else { p3 = list[index]; }
            if (!IsInLine(p1, p2, p3)) {
                result.Add(new Vector2(p2.x, p2.y));
                p1 = p2;
            }
        }
        return result.ToArray();
    }

    private static bool IsInLine(Vector2 p1, Vector2 p2, Vector2 p3) {
        float slope1 = (p1.y - p2.y);
        float slope2 = (p2.y - p3.y);
        return 0 == slope1 && 0 == slope2 ||
           ((p1.x - p2.x) / (Scalar)slope1) == ((p2.x - p3.x) / (Scalar)slope2);
    }
}
