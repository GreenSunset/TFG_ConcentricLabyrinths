using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SimpleCubicMaze : SimpleMaze
{
    public SimpleCubicMaze(int size): base(size) {
        nPlanes = 6;
    }

    protected override void GenerateGraph() {
        float halfSize = (size - 1)/2f;
        float planeDistance = halfSize + 1 / Mathf.Sqrt(2);
        for (float i = -halfSize; i <= halfSize; i++)
        {
            int iInd = (int)(i + halfSize);
            for (float j = -halfSize; j <= halfSize; j++)
            {
                int jInd = (int)(j + halfSize);
                points.Add(new Vector3(i, planeDistance, j));
                points.Add(new Vector3(i, - planeDistance, j));
                points.Add(new Vector3(planeDistance, j, i));
                points.Add(new Vector3(- planeDistance, j, i));
                points.Add(new Vector3(j, i, planeDistance));
                points.Add(new Vector3(j, i, - planeDistance));
                for (int k = 0; k < 6; k++) {
                    if (!graph.ContainsKey(points.Count - 6 + k)) graph.Add(points.Count - 6 + k, new List<int>(){-1, -1, -1, -1});
                }
                if (i > -halfSize) {
                    int connection = ((iInd - 1) * size + jInd) * 6;
                    for (int k = 0; k < 6; k++) {
                        graph[points.Count - 6 + k][0] = (connection + k);
                        // AddEdge(points.Count - 6 + k, connection + k);
                    }
                }
                if (i < halfSize) {
                    int connection = ((iInd + 1) * size + jInd) * 6;
                    for (int k = 0; k < 6; k++) {
                        graph[points.Count - 6 + k][2] = (connection + k);
                        // AddEdge(points.Count - 6 + k, connection + k);
                    }
                }
                if (j > -halfSize) {
                    int connection = (iInd * size + jInd - 1) * 6;
                    for (int k = 0; k < 6; k++) {
                        graph[points.Count - 6 + k][3 -  2 * (k % 2)] = (connection + k);
                        // AddEdge(points.Count - 6 + k, connection + k);
                    }
                }
                if (j < halfSize) {
                    int connection = (iInd * size + jInd + 1) * 6;
                    for (int k = 0; k < 6; k++) {
                        graph[points.Count - 6 + k][1 + 2 * (k % 2)] = (connection + k);
                        // AddEdge(points.Count - 6 + k, connection + k);
                    }
                }
            }
            int edgeN = 6 * iInd;
            int edgeS = 6 * (iInd + size * (size - 1));
            int edgeE = 6 * (iInd * size + (size - 1));
            int edgeW = 6 * iInd * size;
            for (int k = 0; k < 6; k++) {
                if (iInd != (size - 1)) graph.Add(edgeS + k, new List<int>(){-1, -1, -1, -1});
                // else Debug.Log("Already contains S " + iInd);
                if (iInd >= size) graph.Add(edgeW + k, new List<int>(){-1, -1, -1, -1});
                // else Debug.Log("Already contains W " + iInd);
                if (k % 2 == 0) {
                    graph[edgeN + k][0] = (edgeE + (k + 3) % 6);
                    graph[edgeE + k][1] = (edgeS + (k + 4) % 6);
                    graph[edgeS + k][2] = (edgeE + (k + 2) % 6);
                    graph[edgeW + k][3] = (edgeS + (k + 5) % 6);
                } else {
                    graph[edgeN + k][0] = (edgeW + (k + 2) % 6);
                    graph[edgeW + k][1] = (edgeN + (k + 4) % 6);
                    graph[edgeS + k][2] = (edgeW + (k + 1) % 6);
                    graph[edgeE + k][3] = (edgeN + (k + 3) % 6);
                }
            }
        }
    }

    public override Vector3 GetNormal(int point) {
        switch (point % 6) {
            case 0:
                return Vector3.up;
            case 1:
                return - Vector3.up;
            case 2:
                return Vector3.right;
            case 3:
                return - Vector3.right;
            case 4:
                return Vector3.forward;
            case 5:
                return - Vector3.forward;
            default:
                return Vector3.zero;
        }
    }

    public override Quaternion GetRotation(int point) {
        switch (point % 6) {
            case 0:
                return new Quaternion(0, 0, 0, 1);
            case 1:
                return new Quaternion(1, 0, 0, 0);
            case 2:
                return new Quaternion(.5f, .5f, .5f, .5f);
            case 3:
                return new Quaternion(-.5f, .5f, -.5f, .5f);
            case 4:
                return new Quaternion(.5f, .5f, .5f, -.5f);
            case 5:
                return new Quaternion(-.5f, -.5f, .5f, -.5f);
            default:
                return Quaternion.identity;
        }
    }
}
