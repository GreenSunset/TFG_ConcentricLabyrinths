using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicMaze : Maze
{
    protected override void GenerateGraph(int size) {
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
                points.Add(new Vector3(j, i, planeDistance));
                points.Add(new Vector3(j, i, - planeDistance));
                points.Add(new Vector3(planeDistance, j, i));
                points.Add(new Vector3(- planeDistance, j, i));
                if (j > -halfSize) {
                    Vector2Int gridBase = new Vector2Int(iInd * size + jInd, iInd * size + jInd - 1) * 6;
                    for (int k = 0; k < 6; k++) {
                        edges.Add(gridBase + new Vector2Int(k, k));
                    }
                }
                if (i > -halfSize) {
                    Vector2Int basic = new Vector2Int(iInd * size + jInd, (iInd - 1) * size + jInd) * 6;
                    for (int k = 0; k < 6; k++) {
                        edges.Add(basic + new Vector2Int(k, k));
                    }
                }
            }
            int xStart = 6 * iInd * size;
            int xEnd = 6 * (iInd * size + (size - 1));
            int yStart = 6 * iInd;
            int yEnd = 6 * (iInd + size * (size - 1));
            for (int k = 0; k < 3; k++) {
                edges.Add(new Vector2Int(xStart + 2 * k, yEnd + (2 * ((k + 1) % 3)) + 1));
                edges.Add(new Vector2Int(xStart + 2 * k + 1, yStart + (2 * ((k + 1) % 3)) + 1));
                edges.Add(new Vector2Int(xEnd + 2 * k, yEnd + 2 * ((k + 1) % 3)));
                edges.Add(new Vector2Int(xEnd + 2 * k + 1, yStart + 2 * ((k + 1) % 3)));
            }
        }
    }

    public override Vector3 GetNormal(int point) {
        switch (point % 6) {
            case 0:
                return transform.up;
            case 1:
                return - transform.up;
            case 2:
                return transform.forward;
            case 3:
                return - transform.forward;
            case 4:
                return transform.right;
            case 5:
                return - transform.right;
            default:
                return Vector3.zero;
        }
    }
}
