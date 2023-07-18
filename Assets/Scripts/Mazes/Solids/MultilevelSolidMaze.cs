using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultilevelSolidMaze : MultilevelMaze
{
    public struct Coordinate {
        public int x;
        public int y;
        public int plane;
        public int level;

        public override string ToString() {
            return "(" + x + ", " + y + ", " + plane + ", " + level + ")";
        }
    }

    abstract public Quaternion[] faces { get; }

    abstract public List<List<int>> faceAdj { get; }

    public int nPlanes { get { return faces.Length; } }

    abstract protected float distanceFromCenter { get; }
    abstract protected float distanceAdd { get; }
    
    override public Vector3 GetNormal(int index) {
        return faces[index % nPlanes] * Vector3.up;
    }

    override public Vector3 GetNorth(int index) {
        return faces[index % nPlanes] * Vector3.forward;
    }

    override public int GetLevel(int index) {
        return Idx2Coord(index).level;
    }

    abstract public int FaceSize(int levelSize);

    override public int LevelSize(int level) {
        int minSize = maxSize % nCases;
        if (minSize == 0) minSize = nCases;
        int levelSize = level * nCases + minSize;
        // Debug.Log("levelSize: " + levelSize + " for level " + level + " at max size " + maxSize);
        return FaceSize(levelSize) * nPlanes;
    }

    abstract public int Coord2Idx(int x, int y, int plane, int level);

    abstract public Coordinate Idx2Coord(int idx);

    [ContextMenu("Coordinate Test")]
    public void TestCoordinate() {
        for (int i = 0; i < 20; i ++) {
            Coordinate coord = Idx2Coord(i);
            int idx = Coord2Idx(coord.x, coord.y, coord.plane, coord.level);
            Debug.Log(i + " -> " + coord + " -> " + idx);
        }
    }
}
