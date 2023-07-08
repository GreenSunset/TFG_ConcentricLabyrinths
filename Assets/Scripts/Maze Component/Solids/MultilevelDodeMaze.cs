using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultilevelDodeMaze : MultilevelSolidMaze
{
    override public Quaternion[] faces { get; } = new Quaternion[] {
        Quaternion.Euler(0, 144, 0),
        Quaternion.Euler(180, 0, 0),

        Quaternion.Euler(0, -72, 0) * Quaternion.Euler(116.56505f, 180, 0) * Quaternion.Euler(180, -72, 0),
        Quaternion.Euler(0, -72, 0) * Quaternion.Euler(116.56505f, 180, 0),

        Quaternion.Euler(0, -144, 0) * Quaternion.Euler(116.56505f, 180, 0) * Quaternion.Euler(180, -72, 0),
        Quaternion.Euler(0, -144, 0) * Quaternion.Euler(116.56505f, 180, 0),

        Quaternion.Euler(0, 144, 0) * Quaternion.Euler(116.56505f, 180, 0) * Quaternion.Euler(180, -72, 0),
        Quaternion.Euler(0, 144, 0) * Quaternion.Euler(116.56505f, 180, 0) * Quaternion.Euler(0, 72, 0),

        Quaternion.Euler(0, 72, 0) * Quaternion.Euler(116.56505f, 180, 0) * Quaternion.Euler(180, 0, 0),
        Quaternion.Euler(0, 72, 0) * Quaternion.Euler(116.56505f, 180, 0) * Quaternion.Euler(0, 72, 0),
        
        Quaternion.Euler(116.56505f, 180, 0) * Quaternion.Euler(180, 0, 0),
        Quaternion.Euler(116.56505f, 180, 0) * Quaternion.Euler(0, 72, 0),
    };

    override public List<List<int>> faceAdj { get; } = new List<List<int>> {
        new List<int> { 6, 4, 2,10, 8},
        new List<int> {11, 3, 5, 7, 9},
        new List<int> { 4, 9, 7,10, 0},
        new List<int> { 1,11, 6, 8, 5},
        new List<int> { 6,11, 9, 2, 0},
        new List<int> { 1, 3, 8,10, 7},
        new List<int> { 8, 3,11, 4, 0},
        new List<int> { 5,10, 2, 9, 1},
        new List<int> { 0,10, 5, 3, 6},
        new List<int> { 7, 2, 4,11, 1},
        new List<int> { 0, 2, 7, 5, 8},
        new List<int> { 9, 4, 6, 3, 1},
    };

    override protected float distanceFromCenter { get; } = 1.113516364411607f;
    override protected float distanceAdd { get; } = 0.6180339887499f;

    override public int nCases { get; } = 1;

    override public int FaceSize(int levelSize) {
        if (levelSize == 0) return 0;
        return 1 + 5 * levelSize * (levelSize - 1) / 2;
    }
    
    override public int Level2Stride(int level) {
        int size = 0;
        for (int i = 0; i < level; i ++) {
            size += LevelSize(i);
        }
        return size;
    }

    override public int Coord2Idx(int x, int y, int plane, int level) {
        int levelStride = Level2Stride(level);
        int localIdx = nPlanes * (x + FaceSize(y)) + plane;
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
        int y = 0;
        for (y = 0; FaceSize(y + 1) <= idx; y++);
        int x = idx - FaceSize(y);
        return new Coordinate() {x = x, y = y, plane = plane, level = level};
    }

    override protected void GenerateGraph() {
        if (points.Count > 0) {
            Clear();
        }
        float faceProportion = 1.14f;
        int stride = 0;
        int minSize = maxSize % nCases;
        if (minSize == 0) minSize = nCases;
        for (int size = minSize; size <= maxSize; size += nCases) {
            int level = (size - 1) / nCases;
            float planeDistance = (size - 1 + distanceAdd) * distanceFromCenter * faceProportion;
            for (int plane = 0; plane < nPlanes; plane++) {
                graph.Add(points.Count, new List<int>(){-1,-1,-1,-1,-1,-1,-1,-1});
                if (size > 1) {
                    graph[points.Count][0] = points.Count + nPlanes;
                    graph[points.Count][1] = points.Count + 2 * nPlanes;
                    graph[points.Count][2] = points.Count + 3 * nPlanes;
                    graph[points.Count][3] = points.Count + 4 * nPlanes;
                    graph[points.Count][4] = points.Count + 5 * nPlanes;
                } else {
                    graph[points.Count][0] = points.Count - plane + faceAdj[plane][0];
                    graph[points.Count][1] = points.Count - plane + faceAdj[plane][1];
                    graph[points.Count][2] = points.Count - plane + faceAdj[plane][2];
                    graph[points.Count][3] = points.Count - plane + faceAdj[plane][3];
                    graph[points.Count][4] = points.Count - plane + faceAdj[plane][4];
                }
                if (size < maxSize) {
                    graph[points.Count][6] = Coord2Idx(0, 0, plane, level + 1);
                }
                if (size > nCases) {
                    graph[points.Count][7] = Coord2Idx(0, 0, plane, level - 1);
                }
                points.Add(faces[plane] * new Vector3(0, planeDistance, 0));
            }
            for (int iInd = 1; iInd < size; iInd++) {
                float z = iInd * faceProportion * 2 / 3f;
                bool notEdge = iInd != (size - 1);
                Quaternion rotation = Quaternion.identity;
                for (int side = 0, jInd = 0; side < 5; side++) {
                    float x = - iInd / 2f* faceProportion;
                    for (int plane = 0; plane < nPlanes; plane++) {
                        graph.Add(points.Count, new List<int>(){-1,-1,-1,-1,-1,-1,-1,-1});
                        if (notEdge) {
                            graph[points.Count][0] = points.Count + (side > 0 ? (5 * iInd) + side - 1 : 10 * iInd + 4) * nPlanes;
                            graph[points.Count][1] = points.Count + ((5 * iInd) + side) * nPlanes;
                            graph[points.Count][2] = points.Count + ((5 * iInd) + side + 1) * nPlanes;
                        } else {
                            int adj = faceAdj[plane][side];
                            int opposite = (faceAdj[adj].IndexOf(plane) + 1) % 5;
                            graph[points.Count][1] = points.Count - jInd * nPlanes - plane + adj + opposite * iInd * nPlanes;

                            adj = faceAdj[plane][(side + 4) % 5];
                            opposite = faceAdj[adj].IndexOf(plane);
                            graph[points.Count][0] = points.Count - jInd * nPlanes - plane + adj + opposite * iInd * nPlanes;
                        }
                        graph[points.Count][3] = points.Count + (iInd == 1 && side == 4 ? - jInd : 1) * nPlanes;
                        graph[points.Count][4] = points.Count - (5 * (iInd - 1) + side + (iInd == 1 ? 1 : 0)) * nPlanes;
                        graph[points.Count][5] = points.Count + (side > 0 ? - 1 : 5 * iInd - 1) * nPlanes;
                        if (size < maxSize) {
                            graph[points.Count][6] = Coord2Idx(jInd, iInd, plane, level + 1);
                        }
                        if (notEdge && size > nCases) {
                            graph[points.Count][7] = Coord2Idx(jInd, iInd, plane, level - 1);
                        }
                        points.Add(faces[plane] * rotation * new Vector3(x, planeDistance, z));
                    }
                    x += faceProportion;
                    jInd++;
                    for (int k = 1; k < iInd; k++, x += faceProportion, jInd++) {
                        for (int plane = 0; plane < nPlanes; plane++) {
                            graph.Add(points.Count, new List<int>(){-1,-1,-1,-1,-1,-1,-1,-1});
                            if (notEdge) {
                                graph[points.Count][0] = points.Count + ((5 * iInd) + side) * nPlanes;
                                graph[points.Count][1] = points.Count + ((5 * iInd) + side + 1) * nPlanes;
                            } else {
                                int adj = faceAdj[plane][side];
                                int opposite = faceAdj[adj].IndexOf(plane);
                                int index = (opposite + 1) * iInd - k;
                                graph[points.Count][0] = points.Count - jInd * nPlanes - plane + adj + index * nPlanes;
                            }
                            graph[points.Count][2] = points.Count + (jInd < iInd * 5 - 1 ? 1 : - jInd) * nPlanes;
                            graph[points.Count][3] = points.Count - (jInd < iInd * 5 - 1 ? 5 * (iInd - 1) + side : 10 * iInd  - 6) * nPlanes;
                            graph[points.Count][4] = points.Count - (5 * (iInd - 1) + side + 1) * nPlanes;
                            graph[points.Count][5] = points.Count - nPlanes;
                            if (size < maxSize) {
                                graph[points.Count][6] = Coord2Idx(jInd, iInd, plane, level + 1);
                            }
                            if (notEdge && size > nCases) {
                                graph[points.Count][7] = Coord2Idx(jInd, iInd, plane, level - 1);
                            }
                            points.Add(faces[plane] * rotation * new Vector3(x, planeDistance, z));
                        }
                    }
                    rotation *= Quaternion.Euler(0, 72, 0);
                }
            }
            stride += LevelSize(level);
        }
    }

}
