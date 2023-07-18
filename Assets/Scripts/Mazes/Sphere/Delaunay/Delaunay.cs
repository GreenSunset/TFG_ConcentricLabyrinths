using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Delaunay {
    public MultilevelSphericMaze maze;
    public List<Vector2> points { get; private set; }
    public List<Triangle> triangles { get; private set; }
    public Dictionary<int, HashSet<int>> vertTriangles { get; private set; }
    public List<int> hull { get; private set; }
    
    public int stride = 0;

    public Delaunay(MultilevelSphericMaze maze, List<Vector2> points, int stride = 0) {
        triangles = new List<Triangle>();
        vertTriangles = new Dictionary<int, HashSet<int>>();
        hull = new List<int>();
        this.maze = maze;
        this.points = points;
        this.stride = stride;
    }

    public void Triangulate() {
        if (points == null) {
            return;
        }
        List<int> sortedPoints = new List<int>();
        for (int i = 0; i < points.Count; i++) {
            sortedPoints.Add(i);
        }
        sortedPoints.Sort((a, b) => {
            if (points[a].x == Mathf.Infinity || points[a].y == Mathf.Infinity || points[a].x == float.NaN || points[b].y == float.NaN) {
                return +1;
            }
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
        triangles = new List<Triangle>();
        vertTriangles = new Dictionary<int, HashSet<int>>();
        triangles.Add(new Triangle(sortedPoints[0], sortedPoints[1], sortedPoints[minCircumRadiusIndex], this));
        vertTriangles[sortedPoints[0]] = new HashSet<int>(){0};
        vertTriangles[sortedPoints[1]] = new HashSet<int>(){0};
        vertTriangles[sortedPoints[minCircumRadiusIndex]] = new HashSet<int>(){0};
        maze.graph[sortedPoints[0] + stride] = new List<int>(){sortedPoints[1] + stride, sortedPoints[minCircumRadiusIndex] + stride};
        maze.graph[sortedPoints[1] + stride] = new List<int>(){sortedPoints[0] + stride, sortedPoints[minCircumRadiusIndex] + stride};
        maze.graph[sortedPoints[minCircumRadiusIndex] + stride] = new List<int>(){sortedPoints[0] + stride, sortedPoints[1] + stride};
        hull = new List<int>();
        for (int i = 0; i < 3; i++) {
            hull.Add(triangles[0].vertices[i]);
        }
        sortedPoints.Sort((a, b) => {
            if (a == sortedPoints[0] || a == sortedPoints[1] || a == sortedPoints[minCircumRadiusIndex]) return -1;
            return Distance2(points[a], circumCenter).CompareTo(Distance2(points[b], circumCenter));
        });
        for (int i = 3; i < sortedPoints.Count - 1; i++) {
            int point = sortedPoints[i];
            AddToHull(point);
        }
        for (int i = 0; i < hull.Count; i++) {
            int hpoint = hull[i];
            int lastPoint = sortedPoints[sortedPoints.Count - 1];
            maze.graph[hpoint + stride].Add(lastPoint + stride);
            maze.graph[lastPoint + stride].Add(hpoint + stride);
        }
        Queue<int> queue = new Queue<int>();
        // Flip Triangles
        int it = 0;
        while (it < triangles.Count || queue.Count > 0) {
            if (queue.Count == 0) {
                queue.Enqueue(it);
                it++;
            }
            int index = queue.Dequeue();
            Triangle triangle = triangles[index];
            hull = new List<int>(triangle.vertices);
            for (int j = 0; j < 3; j++) {
                int prev = triangle.vertices[(j + 2) % 3];
                int vertex = triangle.vertices[j];
                List<int> vertexTriangles = new List<int>(vertTriangles[vertex]);
                int neighborT = vertexTriangles.Find(t =>  t != index && triangles[t].vertices.Contains(prev));
                int neighborP = triangles[neighborT].vertices.Find(v => v != prev && v != vertex);
                float distance = Distance(triangle.circumCentre, points[neighborP]);
                if (distance < triangle.circumRadius) {
                    Flip(index,neighborT, vertex, prev);
                    if (neighborT < index) {
                        queue.Enqueue(neighborT);
                    }
                    triangle = triangles[index];
                    hull = new List<int>(triangle.vertices);
                    j = -1;
                }
            }

        }
    }

    public void Flip(int a, int b, int common1, int common2) {
        if (a == b) {
            Debug.LogError("Flipping the same triangle " + a);
        }
        if (common1 == common2) {
            Debug.LogError("Common vertices are the same " + common1 + " and " + common2);
        }
        int aVertex = triangles[a].vertices.Find(v => v != common1 && v != common2);
        int bVertex = triangles[b].vertices.Find(v => v != common1 && v != common2);
        triangles[a] = new Triangle(aVertex, common1, bVertex, this);
        triangles[b] = new Triangle(aVertex, common2, bVertex, this);
        vertTriangles[common1].Remove(b);
        vertTriangles[aVertex].Add(b);
        vertTriangles[common2].Remove(a);
        vertTriangles[bVertex].Add(a);
        maze.graph[common1 + stride].Remove(common2 + stride);
        maze.graph[common2 + stride].Remove(common1 + stride);
        maze.graph[aVertex + stride].Add(bVertex + stride);
        maze.graph[bVertex + stride].Add(aVertex + stride);
    }

    // public void Hull2Maze() {
    //     if (hull == null || hull.Count < 3) return;
    //     maze.maze.Clear();
    //     for (int i = 0; i < hull.Count; i++) {
    //         int a = hull[i];
    //         maze.maze.Add(a + stride, new HashSet<int>());
    //         int prev = hull[(i + hull.Count - 1) % hull.Count];
    //         int next = hull[(i + 1) % hull.Count];
    //         maze.maze[a + stride].Add(next);
    //         maze.maze[a + stride].Add(prev);
    //     }
    // }

    public void AddToHull(int point) {
        Vector2 pointPos = points[point];
        int insertAt = -1;
        bool cont = false;
        vertTriangles.Add(point, new HashSet<int>());
        List<int> toRemove = new List<int>();
        for (int i = 0; i < hull.Count; i++) {
            int prev = hull[(i + hull.Count - 1) % hull.Count];
            int next = hull[i];
            if (IsLeftOf(pointPos, prev, next)) {
                if (insertAt == -1) {
                    insertAt = i;
                    cont = true;
                } else if (cont) toRemove.Add(prev);
                else toRemove.Add(next);
                maze.graph[point + stride].Add(next + stride);
                maze.graph[next + stride].Add(point + stride);
                if (!maze.graph[point + stride].Contains(prev + stride)) {
                    maze.graph[point + stride].Add(prev + stride);
                    maze.graph[prev + stride].Add(point + stride);
                }
                vertTriangles[point].Add(triangles.Count);
                vertTriangles[prev].Add(triangles.Count);
                vertTriangles[next].Add(triangles.Count);
                triangles.Add(new Triangle(point, prev, next, this));
            } else {
                cont = false;
            }
        }
        hull.Insert(insertAt, point);
        hull.RemoveAll(x => toRemove.Contains(x));
    }

    public bool IsLeftOf(Vector2 point, int a, int b) {
        Vector2 AB = points[b] - points[a];
        Vector2 AC = point - points[a];
        return Vector2.SignedAngle(AC, AB) > 0;
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
        if (float.IsNaN(a.x) || float.IsNaN(a.y) || float.IsNaN(b.x) || float.IsNaN(b.y)) return Mathf.Infinity;
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