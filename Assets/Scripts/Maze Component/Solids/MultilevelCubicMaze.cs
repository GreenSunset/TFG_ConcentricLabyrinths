using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultilevelCubicMaze : MultilevelSolidMaze
{
    override public Quaternion[] faces { get; } = new Quaternion[] {
        Quaternion.Euler(0,90,0),
        Quaternion.Euler(0,180,180),
        Quaternion.Euler(90,0,-90),
        Quaternion.Euler(180,0,90),
        Quaternion.Euler(90,0,0),
        Quaternion.Euler(0,90,-90),
    };

    override public List<List<int>> faceAdj { get; } = new List<List<int>> {
        new List<int> {4, 2, 5, 3},
        new List<int> {3, 5, 2, 4},
        new List<int> {4, 1, 5, 0},
        new List<int> {0, 5, 1, 4},
        new List<int> {3, 1, 2, 0},
        new List<int> {0, 2, 1, 3},
    };
    override protected float distanceFromCenter { get; } = 1 / 2f;
    override protected float distanceAdd { get; } = Mathf.Sqrt(2f);

    override public int nCases { get; } = 2;

    override public int FaceSize(int levelSize) {
        return levelSize * levelSize;
    }
    
    override public int Level2Stride(int level) {
        //For cubes 
        int n = level + 1;
        if (maxSize % nCases == 0) {
            return 4 * n * (n - 1) * (2 * n - 1);
        }
        else {
            return 2 * (n - 1) * (2 * n - 1) * (2 * n - 3);
        }
    }

    override public int Coord2Idx(int x, int y, int plane, int level) {
        int levelStride = Level2Stride(level);
        
        int minSize = maxSize % nCases;
        if (minSize == 0) minSize = nCases;
        int levelSize = level * nCases + minSize;
        int localIdx = nPlanes * (x + y * levelSize) + plane;
        return levelStride + localIdx;
    }

    override public Coordinate Idx2Coord(int idx) {
        int levelSize = maxSize % nCases;
        if (levelSize == 0) levelSize = nCases;
        // Debug.Log("levelSize: " + levelSize + " for level 0 at max size " + maxSize);
        int level = 0;
        int levelStride = 0;
        int sum = LevelSize(level);
        while (levelStride + sum <= idx) {
            levelStride += sum;
            levelSize += nCases;
            level++;
            sum = LevelSize(level);
        }
        if (levelStride != Level2Stride(level)) {
            Debug.LogWarning("Error: " + levelStride + " != " + Level2Stride(level));
        }
        idx -= levelStride;
        int plane = idx % nPlanes;
        idx /= nPlanes;
        int y = idx / levelSize;
        int x = idx % levelSize;
        return new Coordinate { x = x, y = y, plane = plane, level = level };
    }

    override protected void GenerateGraph() {
        if (points.Count > 0) {
            Clear();
        }
        int stride = 0;
        int minSize = maxSize % nCases;
        if (minSize == 0) minSize = nCases;
        for (int size = minSize; size <= maxSize; size += nCases) {
            float halfSize = (size - 1) * distanceFromCenter;
            float planeDistance = halfSize + distanceAdd * distanceFromCenter;
            for (int iInd = 0; iInd < size; iInd++) {
                float x = iInd - halfSize;
                bool notEdgeN = iInd != 0;
                bool notEdgeS = iInd != (size - 1);
                for (int jInd = 0; jInd < size; jInd++) {
                    float z = jInd - halfSize;
                    bool notEdgeE = jInd != (size - 1);
                    bool notEdgeW = jInd != 0;
                    for (int plane = 0; plane < nPlanes; plane++) {
                        if (notEdgeS || !notEdgeE) {
                            graph.Add(points.Count, new List<int>(){-1, -1, -1, -1, -1, -1});
                        }
                        if (notEdgeN) {
                            graph[points.Count][0] = points.Count - nPlanes * size;
                        }
                        if (notEdgeE) {
                            graph[points.Count][1] = points.Count + nPlanes;
                        }
                        if (notEdgeS) {
                            graph[points.Count][2] = points.Count + nPlanes * size;
                        }
                        if (notEdgeW) {
                            graph[points.Count][3] = points.Count - nPlanes;
                        }
                        if (size < maxSize) {
                            graph[points.Count][4] = Coord2Idx(jInd + 1, iInd + 1, plane, (size - 1) / nCases + 1);
                        }
                        if (size > nCases && notEdgeE && notEdgeN && notEdgeS && notEdgeW) {
                            graph[points.Count][5] = Coord2Idx(jInd - 1, iInd - 1, plane, (size - 1) / nCases - 1);
                        }
                        points.Add(faces[plane] * new Vector3(x, planeDistance, z));
                    }
                }
                int[] edges = new int[] {
                    stride + nPlanes * iInd,
                    stride + nPlanes * (iInd * size + (size - 1)),
                    stride + nPlanes * (iInd + size * (size - 1)),
                    stride + nPlanes * iInd * size,
                };
                for (int face = 0; face < nPlanes; face++) {
                    if (notEdgeS) {
                        graph.Add(edges[2] + face, new List<int>(){-1, -1, -1, -1, -1, -1});
                    }
                    for (int edgeStart = 0; edgeStart < edges.Length; edgeStart++) {
                        int neighborFace = faceAdj[face][edgeStart];
                        int edgeEnd = faceAdj[neighborFace].IndexOf(face);
                        graph[edges[edgeStart] + face][edgeStart] = edges[edgeEnd] + neighborFace;
                    }
                }
            }
            stride += LevelSize((size - 1) / nCases);
        }
    }
}
