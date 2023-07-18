using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultilevelTetraMaze : MultilevelSolidMaze
{
    // static float angle = 70.5287793655093f; //Mathf.Acos(1/3f) * Mathf.Rad2Deg;
    // 109.4712206344907
    override public Quaternion[] faces { get; } = new Quaternion[] {
        new Quaternion(0, 0, -0.5773503f, 0.8164966f), //Quaternion.Euler(0,0,-angle),
        new Quaternion(-0.5f, 0.7071068f, -0.2886751f, 0.4082483f), //Quaternion.Euler(0,180 - angle,-angle),
        new Quaternion(0.5f, -0.7071068f, -0.2886751f, 0.4082483f), //Quaternion.Euler(0,-180 +angle,-angle),
        Quaternion.Euler(180,0,0),
    };

    override public List<List<int>> faceAdj { get; } = new List<List<int>> {
        new List<int> {2,3,-1,1},
        new List<int> {0,-1,3,2},
        new List<int> {1,3,-1,0},
        new List<int> {1,-1,0,2},
    };

    override protected float distanceFromCenter { get; } = 1 / Mathf.Sqrt(24f);
    override protected float distanceAdd { get; } = 3;
    
    override public int nCases { get; } = 3;

    override public int FaceSize(int levelSize) {
        return levelSize * (levelSize + 1) / 2;
    }

    override public int Level2Stride(int level) {
        int minLevelSize = maxSize % nCases;
        if (minLevelSize == 0) minLevelSize = nCases;
        int levelSize = level * nCases + minLevelSize;
        float squareSize = levelSize * levelSize;
        float cubeSize = squareSize * levelSize;
        return (int)((cubeSize - 3 * squareSize + 2 * (levelSize % nCases)) / 18) * nPlanes;
    }

    override public int Coord2Idx(int x, int y, int plane, int level) {
        if (x > y) {
            Debug.LogError("x can't be more than y in triangle");
        }
        int levelStride = Level2Stride(level);
        int localIdx = nPlanes * (x + FaceSize(y)) + plane;
        return levelStride + localIdx;
    }

    override public Coordinate Idx2Coord(int idx) {
        int levelSize = maxSize % nCases;
        if (levelSize == 0) levelSize = nCases;
        int levelStride = 0;
        int level = 0;
        int sum = LevelSize(level);
        while (levelStride + sum <= idx) {
            levelStride += sum;
            levelSize += nCases;
            level++;
            sum = LevelSize(level);
        }
        if (levelStride != Level2Stride(level)) {
            Debug.Log("Error: " + levelStride + " != " + Level2Stride(level));
        }
        idx -= levelStride;
        int plane = idx % nPlanes;
        idx /= nPlanes;
        int y = 0;
        for (y = 0; FaceSize(y + 1) <= idx; y++);
        int x = idx - FaceSize(y);
        return new Coordinate { x = x, y = y, plane = plane, level = level };
    }


    override protected void GenerateGraph() {
        if (points.Count > 0) {
            Clear();
        }
        int stride = 0;
        float sqrt3 = Mathf.Sqrt(3);
        int minSize = maxSize % nCases;
        if (minSize == 0) minSize = nCases;
        for (int size = minSize; size <= maxSize; size += nCases) {
            // Calculate plane distance
            float planeDistance = (size - 1 + distanceAdd) * distanceFromCenter;
            for (int iInd = 0; iInd < size; iInd++) {
                float x = iInd * sqrt3 / 2f - (size - 1) / sqrt3;
                bool notEdgeS = iInd != (size - 1);
                for (int jInd = 0; jInd <= iInd; jInd++) {
                    float z = jInd - iInd / 2f;
                    // Check edge
                    bool notEdgeE = jInd != iInd;
                    bool notEdgeW = jInd != 0;
                    for (int plane = 0; plane < nPlanes; plane++) {
                        // Add point to adj dict (check edge exception)
                        if (notEdgeS || (plane % 2 == 0 && !notEdgeW) || (plane % 2 == 1 && !notEdgeE)) {
                            graph.Add(points.Count, new List<int>(){-1, -1, -1, -1, -1, -1, -1, -1});
                        }
                        // Add plane adjacencies (check edge case)
                        if (notEdgeE) {
                            graph[points.Count][0] = points.Count - iInd * nPlanes;
                            graph[points.Count][1] = points.Count + nPlanes;
                        }
                        if (notEdgeS) {
                            graph[points.Count][2] = points.Count + (iInd + 2) * nPlanes;
                            graph[points.Count][3] = points.Count + (iInd + 1) * nPlanes;
                        }
                        if (notEdgeW) {
                            graph[points.Count][4] = points.Count - nPlanes;
                            graph[points.Count][5] = points.Count - (iInd + 1) * nPlanes;
                        }
                        // Add interlevel adjacencies (check lowest or highest level)
                        if (size < maxSize) {
                            graph[points.Count][6] = Coord2Idx(jInd + 1, iInd + 2, plane, (size - 1) / nCases + 1);
                        }
                        if (size > 3 && notEdgeE && notEdgeS && notEdgeW) {
                            graph[points.Count][7] = Coord2Idx(jInd - 1, iInd - 2, plane, (size - 1) / nCases - 1);
                        }
                        // Add Point to points list
                        points.Add(faces[plane] * new Vector3(x, planeDistance, z));
                    }
                }

                int[] edges = new int[] {
                    stride + nPlanes * (FaceSize(iInd + 1) - 1),
                    stride + nPlanes * (FaceSize(size) - 1 - iInd),
                    stride + nPlanes * (FaceSize(size - 1) + iInd),
                    stride + nPlanes * FaceSize(iInd),
                };
                for (int face = 0; face < nPlanes; face++) {
                    // Add point to adj dict (check edge exception)
                    if (notEdgeS)
                    {
                        graph.Add(edges[1 + face % 2] + face, new List<int>(){-1, -1, -1, -1, -1, -1, -1, -1});
                    }
                    for (int edgeStart = 0; edgeStart < edges.Length; edgeStart++) {
                        // Check face adjacencies
                        int neighborFace = faceAdj[face][edgeStart];
                        if (neighborFace == -1) continue;
                        int edgeEnd = faceAdj[neighborFace].IndexOf(face);
                        graph[edges[edgeStart] + face][2*(edgeStart - edgeStart/2)] = edges[edgeEnd] + neighborFace;
                    }
                }
            }
            stride += LevelSize((size - 1) / nCases);
        }
    }
}
