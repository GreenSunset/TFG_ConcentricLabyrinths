using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultilevelSphericMaze : MultilevelMaze
{
    private Delaunay delaunay;

    override public int nCases { get; } = 1;
    override public Vector3 GetNormal(int index) {
        return points[index].normalized;
    }

    override public Vector3 GetNorth(int index) {
        Vector3 normal = GetNormal(index);
        Vector3 northernN = points[graph[index][0]];
        return Vector3.ProjectOnPlane(northernN, normal).normalized;
        throw new System.NotImplementedException();
    }

    override public int GetLevel(int index) {
        int levelSize = Mathf.RoundToInt(points[index].magnitude);
        return (levelSize - 1) / nCases;
    }


    override public int LevelSize(int level) {
        int minLevelSize = maxSize % nCases;
        if (minLevelSize == 0) minLevelSize = nCases;
        int levelSize = minLevelSize + level * nCases;
        return Mathf.RoundToInt(4* Mathf.PI * levelSize * levelSize);
    }

    override public int Level2Stride(int level) {
        int size = 0;
        for (int i = 0; i < level; i ++) {
            size += LevelSize(i);
        }
        return size;
    }

    protected override void GenerateGraph()
    {
        int stride = 0;
        int minLevelSize = maxSize % nCases;
        if (minLevelSize == 0) minLevelSize = nCases;
        for (int size = minLevelSize; size <= maxSize; size += nCases) {
            int prevStride = stride;
            stride = points.Count;
            int numPoints = LevelSize((size - 1) / nCases);
            Debug.Log("Size: " + size + " numPoints: " + numPoints);
            float gRatio = (1+Mathf.Sqrt(5))/2;
            // float epsilon = 3.5f;
            List<Vector2> proyected = new List<Vector2>();
            for (float i = 0; i < numPoints; i++) {
                Vector2 ti = new Vector2(i/numPoints, i/gRatio);
                // ti = new Vector2((i + epsilon)/(numPoints - 1 + 2 * epsilon), i/gRatio);
                // if (i == 0) {
                //     ti = new Vector2(0, 0);
                // }
                // if (i == numPoints - 1) {
                //     ti = new Vector2(1, 0);
                // }
                float r = Mathf.Sqrt(ti.x);
                float phi = Mathf.Acos(1 - 2*ti.x);
                float theta = 2*Mathf.PI*ti.y;
                Vector3 point = new Vector3(
                    r * Mathf.Cos(theta),
                    0,
                    r * Mathf.Sin(theta));
                point = new Vector3(
                    Mathf.Cos(theta) * Mathf.Sin(phi),
                    Mathf.Cos(phi),
                    Mathf.Sin(theta) * Mathf.Sin(phi));
                Vector2 proyectedPoint = new Vector2(point.x/(1+point.y), point.z/(1+point.y));
                proyected.Add(proyectedPoint * size);
                graph.Add(points.Count, new List<int>());
                // maze.Add(points.Count, new HashSet<int>());
                points.Add(point * size);
            }
            delaunay = new Delaunay(this, proyected, stride);
            delaunay.Triangulate();
            for (int i = stride; i < points.Count; i++) {
                Vector3 normal = GetNormal(i);
                Vector3 north = GetNorth(i);
                HashSet<int> neighbours = new HashSet<int>(graph[i]);
                graph[i] = new List<int>(neighbours);
                graph[i].Sort((a, b) => {
                    Vector3 projA = Vector3.ProjectOnPlane(points[a], normal);
                    Vector3 projB = Vector3.ProjectOnPlane(points[b], normal);
                    float angleA = Vector3.SignedAngle(north, projA, normal);
                    float angleB = Vector3.SignedAngle(north, projB, normal);
                    return angleA.CompareTo(angleB);
                });
                graph[i].Add(-1);
                graph[i].Add(-1);
            }
            for (int i = prevStride; i < stride; i++) {
                Vector3 proyectedPoint = points[i] * size / (size - 1);
                float distance = float.MaxValue;
                int closest = -1;
                for (int j = stride; j < points.Count; j++) {
                    float newDistance = Vector3.Distance(proyectedPoint, points[j]);
                    if (newDistance < distance) {
                        distance = newDistance;
                        closest = j;
                    }
                }
                graph[i][graph[i].Count - 2] = closest;
                graph[closest][graph[closest].Count - 1] = i;
            }
        }
    }

    // protected override void GenerateMaze()
    // {
    //     GenerateMazeUnrestricted();
    // }

}
