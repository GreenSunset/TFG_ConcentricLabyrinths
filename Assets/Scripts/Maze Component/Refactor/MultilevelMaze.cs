using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultilevelMaze : MonoBehaviour
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
    abstract public int nCases { get; }

    public int entryIndex { get; protected set; } = -1;

    public int maxSize = 5;
    public int nPlanes { get { return faces.Length; } }
    public List<Vector3> points = new List<Vector3>();
    public Dictionary<int, List<int>> graph { get; protected set; } = new Dictionary<int, List<int>>();
    public Dictionary<int, HashSet<int>> maze { get; protected set; } = new Dictionary<int, HashSet<int>>();

    abstract public int FaceSize(int levelSize);

    public int LevelSize(int levelSize) {
        return FaceSize(levelSize) * nPlanes;
    }

    abstract public int Level2Stride(int levelSize);

    abstract public int Coord2Idx(int x, int y, int plane, int levelSize);

    abstract public int Coord2Idx(Coordinate coord);

    abstract public Coordinate Idx2Coord(int idx, int sizeCase);

    [ContextMenu("Generate")]
    public void Generate() {
        Clear();
        GenerateGraph();
        GenerateMaze();
    }

    public void Clear() {
        points.Clear();
        graph.Clear();
        maze.Clear();
        entryIndex = -1;
    }

    abstract protected void GenerateGraph();

    // Currently generates level by level, need to add difficulty modifier
    virtual protected void GenerateMaze() {
        if (graph.Count == 0) {
            return;
        }
        HashSet<int> visited = new HashSet<int>();
        int size = maxSize;
        int i = 0;
        int accounted = 0;
        while (visited.Count < points.Count) {
            i++;
            if (size < 1) {
                Debug.LogError("Error: size < 1");
                break;
            }
            if (i > 1000) {
                Debug.LogError("Error: infinite loop");
                break;
            }
            Stack<int> stack = new Stack<int>();
            int current = Random.Range(Level2Stride(size), points.Count - visited.Count);
            // Debug.Log(current);
            int entry = graph[current][graph[current].Count - 2];
            if (entry != -1) {
                maze[entry].Add(current);
                maze.Add(current, new HashSet<int>(){entry});
            } else {
                entryIndex = current;
            }
            accounted = visited.Count;
            visited.Add(current);
            while ((visited.Count - accounted) < LevelSize(size)) {
                List<int> neighbors = graph[current].GetRange(0, graph[current].Count - 2);
                neighbors.RemoveAll(x => visited.Contains(x) || x == -1);
                if (neighbors.Count > 0) {
                    int next = neighbors[Random.Range(0, neighbors.Count)];
                    stack.Push(current);
                    if (!maze.ContainsKey(current)) maze.Add(current, new HashSet<int>());
                    if (!maze.ContainsKey(next)) maze.Add(next, new HashSet<int>());
                    maze[current].Add(next);
                    maze[next].Add(current);
                    current = next;
                    visited.Add(current);
                }
                else if (stack.Count > 0) {
                    current = stack.Pop();
                }
                else {
                    Debug.LogError("Error: premature break at " + Idx2Coord(current, size));
                    break;
                }
            }
            size -= nCases;
        }
    }

    // DEBUG: Desired behaviour at max difficulty. Any maze should be able to execute this.
    virtual protected void GenerateMazeUnrestricted() {
        if (graph.Count == 0) {
            return;
        }
        HashSet<int> visited = new HashSet<int>();
        Stack<int> stack = new Stack<int>();
        int current = Random.Range(0, points.Count);
        visited.Add(current);
        while (visited.Count < points.Count) {
            List<int> neighbors = new List<int>(graph[current]);
            neighbors.RemoveAll(x => visited.Contains(x) || x == -1);
            if (neighbors.Count > 0) {
                int next = neighbors[Random.Range(0, neighbors.Count)];
                stack.Push(current);
                if (!maze.ContainsKey(current)) maze.Add(current, new HashSet<int>());
                if (!maze.ContainsKey(next)) maze.Add(next, new HashSet<int>());
                maze[current].Add(next);
                maze[next].Add(current);
                current = next;
                visited.Add(current);
            }
            else if (stack.Count > 0) {
                current = stack.Pop();
            }
            else {
                Debug.LogError("Error: premature break at " + Idx2Coord(current, maxSize));
                break;
            }
        }
    }
}
