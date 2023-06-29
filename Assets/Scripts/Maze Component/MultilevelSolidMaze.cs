using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultilevelSolidMaze : MultilevelMaze
{
    public struct Coordinate {
        public int x;
        public int y;
        public int plane;
        public int levelSize;

        public override string ToString() {
            return "(" + x + ", " + y + ", " + plane + ", " + levelSize + ")";
        }
    }

    abstract public Quaternion[] faces { get; }

    abstract public List<List<int>> faceAdj { get; }

    public int nPlanes { get { return faces.Length; } }

    override public Vector3 GetNormal(int index) {
        return faces[index % nPlanes] * Vector3.up;
    }

    override public Vector3 GetNorth(int index) {
        return faces[index % nPlanes] * Vector3.forward;
    }

    override public int GetDepth(int index) {
        return  Idx2Coord(index, maxSize).levelSize / nCases;
    }
    
    override public int GetLevelSize(int index) {
        return  Idx2Coord(index, maxSize).levelSize;
    }

    abstract public int FaceSize(int levelSize);

    override public int LevelSize(int levelSize) {
        return FaceSize(levelSize) * nPlanes;
    }

    abstract public int Coord2Idx(int x, int y, int plane, int levelSize);

    abstract public int Coord2Idx(Coordinate coord);

    abstract public Coordinate Idx2Coord(int idx, int sizeCase);
}
