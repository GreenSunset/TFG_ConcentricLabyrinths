using System.Collections.Generic;
using UnityEngine;

public class Delaunay {
    public List<Vector2> points { get; private set; }
    
    public List<Vector2Int> Triangulate(List<Vector2> p) {
        this.points = new List<Vector2>(p);
        List<int> sortedPoints = new List<int>();
        for (int i = 0; i < points.Count; i++) {
            sortedPoints.Add(i);
        }
        sortedPoints.Sort((a, b) => {
            return Distance2(points[a], points[0]).CompareTo(Distance2(points[b], points[0]));
        });
        int minCircumRadiusIndex = 2;
        Vector2 circumCenter = FindCircumcenter(sortedPoints[0], sortedPoints[1], sortedPoints[2]);
        float circumRadius = Distance2(circumCenter, sortedPoints[0]);
        for (int i = 3; i < sortedPoints.Count; i++) {
            Vector2 centerCand = FindCircumcenter(sortedPoints[0], sortedPoints[1], sortedPoints[i]);
            float radiusCand = Distance2(centerCand, sortedPoints[0]);
            if (radiusCand < circumRadius) {
                circumRadius = radiusCand;
                minCircumRadiusIndex = i;
                circumCenter = centerCand;
            }
        }
        List<Triangle> triangles = new List<Triangle>();
        triangles.Add(new Triangle(sortedPoints[0], sortedPoints[1], sortedPoints[minCircumRadiusIndex], this));
        sortedPoints.Sort((a, b) => {
            if (a == sortedPoints[0] || a == sortedPoints[1] || a == sortedPoints[minCircumRadiusIndex]) return -1;
            return Distance2(points[a], circumCenter).CompareTo(Distance2(points[b], circumCenter));
        });

        for (int i = 3; i < sortedPoints.Count; i++) {
            int point = sortedPoints[i];
            
        }

        // for (int pIndex = 0; pIndex < points.Count; pIndex++) {
        //     Debug.Log("Point " + pIndex);
        //     List<Triangle> badTriangles = new List<Triangle>();

        //     for (int tIndex = 0; tIndex < triangles.Count; tIndex++) {
        //         Triangle triangle = triangles[tIndex];
        //         if (Distance(pIndex, triangle.circumCentre) < triangle.circumRadius) {
        //             Debug.Log("Bad triangle { " + triangle.vertices[0] + ", " + triangle.vertices[1] + ", " + triangle.vertices[2] + " }");
        //             badTriangles.Add(triangle);
        //         }
        //     }

        //     List<(int, int)> polygon = new List<(int, int)>();
        //     for (int i = 0; i < badTriangles.Count; i++) {
        //         Triangle triangle = badTriangles[i];
        //         (int, int)[] edges = new (int, int)[] {
        //             (triangle.vertices[0], triangle.vertices[1]),
        //             (triangle.vertices[1], triangle.vertices[2]),
        //             (triangle.vertices[2], triangle.vertices[0])
        //         };
        //         for (int j = 0; j < edges.Length; j++) {
        //             bool reject = false;
        //             for (int k = 0; k < badTriangles.Count; k++) {
        //                 if (k != i && ContainsEdge(triangles[k], edges[j])) {
        //                     reject = true;
        //                 }
        //             }
        //             if (!reject) {
        //                 polygon.Add(edges[j]);
        //             }
        //         }
        //     }

        //     for (int i = 0; i < badTriangles.Count; i++) triangles.Remove(badTriangles[i]);

        //     for (int i = 0; i < polygon.Count; i++) {
        //         Debug.Log("Adding triangle { " + polygon[i].Item1 + ", " + polygon[i].Item2 + ", " + pIndex + " }");
        //         triangles.Add(new Triangle(pIndex, polygon[i].Item1, polygon[i].Item2, this));
        //     }
        // }
        HashSet<Vector2Int> result = new HashSet<Vector2Int>();
        // for (int i = 0; i < triangles.Count; i++) {
        //     Triangle triangle = triangles[i];
        //     if (triangle.vertices[0] < numPoints && triangle.vertices[1] < numPoints && triangle.vertices[2] < numPoints) {
        //         result.Add(new Vector2Int(triangle.vertices[0], triangle.vertices[1]));
        //         result.Add(new Vector2Int(triangle.vertices[1], triangle.vertices[2]));
        //         result.Add(new Vector2Int(triangle.vertices[2], triangle.vertices[0]));
        //     }
        // }
        return new List<Vector2Int>(result);
    }



    public float Distance2(int a, Vector2 b) {
        return Distance2(points[a], b);
    }

    public float Distance2(Vector2 a, int b) {
        return Distance2(a, points[b]);
    }

    public float Distance2(int a, int b) {
        Vector2 A = points[a];
        Vector2 B = points[b];
        return Mathf.Pow(A.x - B.x, 2) + Mathf.Pow(A.y - B.y, 2);
    }

    public static float Distance2(Vector2 a, Vector2 b) {
        return Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2);
    }

    public float Distance(int a, Vector2 b) {
        return Mathf.Sqrt(Distance2(points[a], b));
    }

    public float Distance(Vector2 a, int b) {
        return Mathf.Sqrt(Distance2(a, points[b]));
    }

    public float Distance(int a, int b) {
        return Mathf.Sqrt(Distance2(a, b));
    }

    public static float Distance(Vector2 a, Vector2 b) {
        return Mathf.Sqrt(Distance2(a, b));
    }

    public bool IsCounterClockwise(int a, int b, int c)
    {
        return IsCounterClockwise(points[a], points[b], points[c]);
    }
    
    public static bool IsCounterClockwise(Vector2 a, Vector2 b, Vector2 c)
    {
        float result = (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
        return result > 0;
    }
    
    public Vector2 FindCircumcenter(int a, int b, int c) {
        return FindCircumcenter(points[a], points[b], points[c]);
    }

    public static Vector2 FindCircumcenter(Vector2 a, Vector2 b, Vector2 c) {
        float d = 2 * (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
        float x = ((a.x * a.x + a.y * a.y) * (b.y - c.y) + (b.x * b.x + b.y * b.y) * (c.y - a.y) + (c.x * c.x + c.y * c.y) * (a.y - b.y)) / d;
        float y = ((a.x * a.x + a.y * a.y) * (c.x - b.x) + (b.x * b.x + b.y * b.y) * (a.x - c.x) + (c.x * c.x + c.y * c.y) * (b.x - a.x)) / d;
        return new Vector2(x, y);
    }

    public static bool ContainsEdge(Triangle triangle, (int, int) edge) {
        return (edge.Item1 == triangle.vertices[0] || edge.Item1 == triangle.vertices[1] || edge.Item1 == triangle.vertices[2]) &&
          (edge.Item2 == triangle.vertices[0] || edge.Item2 == triangle.vertices[1] || edge.Item2 == triangle.vertices[2]);
    }
}