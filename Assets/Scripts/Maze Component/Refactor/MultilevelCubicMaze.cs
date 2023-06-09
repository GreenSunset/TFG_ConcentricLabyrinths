using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultilevelCubicMaze : MultilevelMaze
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

    override public int nCases { get; } = 2;

    override public int FaceSize(int levelSize) {
        return levelSize * levelSize;
    }
    
    override public int Level2Stride(int levelSize) {
        //For cubes 
        int n = (levelSize + 1) / nCases;
        if (levelSize % nCases == 0) {
            return 4 * n * (n - 1) * (2 * n - 1);
        }
        else {
            return 2 * (n - 1) * (2 * n - 1) * (2 * n - 3);
        }
    }

    override public int Coord2Idx(int x, int y, int plane, int levelSize) {
        int levelStride = Level2Stride(levelSize);
        int localIdx = nPlanes * (x + y * levelSize) + plane;
        return levelStride + localIdx;
    }

    override public int Coord2Idx(Coordinate coord) {
        int levelStride = Level2Stride(coord.levelSize);
        int localIdx = nPlanes * (coord.x + coord.y * coord.levelSize) + coord.plane;
        return levelStride + localIdx;
    }

    override public Coordinate Idx2Coord(int idx, int sizeCase) {
        int levelSize = 2 - (sizeCase % nCases);
        int levelStride = 0;
        int sum = LevelSize(levelSize);
        while (levelStride + sum <= idx) {
            levelStride += sum;
            levelSize += nCases;
            sum = LevelSize(levelSize);
        }
        if (levelStride != Level2Stride(levelSize)) {
            Debug.Log("Error: " + levelStride + " != " + Level2Stride(levelSize));
        }
        idx -= levelStride;
        int plane = idx % nPlanes;
        idx /= nPlanes;
        int y = idx / levelSize;
        int x = idx % levelSize;
        return new Coordinate { x = x, y = y, plane = plane, levelSize = levelSize };
    }

    override protected void GenerateGraph() {
        if (points.Count > 0) {
            Clear();
        }
        int stride = 0;
        for (int size = maxSize % nCases; size <= maxSize; size += nCases) {
            float halfSize = (size - 1)/2f;
            float planeDistance = halfSize + 1 / Mathf.Sqrt(2);
            for (float x = -halfSize; x <= halfSize; x++) {
                int iInd = (int)(x + halfSize);
                bool notEdgeN = iInd != 0;
                bool notEdgeS = iInd != (size - 1);
                for (float z = -halfSize; z <= halfSize; z++) {
                    int jInd = (int)(z + halfSize);
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
                            graph[points.Count][4] = Coord2Idx(jInd + 1, iInd + 1, plane, size + 2);
                        }
                        if (size > 2 && notEdgeE && notEdgeN && notEdgeS && notEdgeW) {
                            graph[points.Count][5] = Coord2Idx(jInd - 1, iInd - 1, plane, size - 2);
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
            stride += LevelSize(size);
        }
    }
}
