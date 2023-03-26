using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphericMaze : Maze
{
    protected override void GenerateGraph(int size) {
        int numPoints = Mathf.RoundToInt(4* Mathf.PI * size * size);
        float dlong = Mathf.PI * (3-Mathf.Sqrt(5));
        float dz = 2f / numPoints;
        float longtitude = 0;
        float z = 1 - dz/2 - numPoints/2 * dz;
        List<Vector2> projectedPoints = new List<Vector2>();
        for (int i = 0; i < numPoints / 2; i++) {
            float radius = Mathf.Sqrt(1-z*z);
            Vector3 point = new Vector3(Mathf.Cos(longtitude)*radius, Mathf.Sin(longtitude)*radius, z);
            points.Add(new Vector3(point.x/(1-z), 0, point.y /(1-z))*size);
            // points.Add(point * size);
            projectedPoints.Add(new Vector2(point.x/(1-z), point.y/(1-z)));
            z -= dz;
            longtitude += dlong;
        }
        Delaunay delaunay = new Delaunay();
        List<Vector2Int> dEdges = delaunay.Triangulate(projectedPoints);
        for (int i = 0; i < dEdges.Count; i++) {
            edges.Add(new Vector2Int(dEdges[i].x, dEdges[i].y));
            edges.Add(new Vector2Int(dEdges[i].y, dEdges[i].x));
        }
    }
}
