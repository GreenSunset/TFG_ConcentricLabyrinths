using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultilevelMaze : MonoBehaviour
{

    public int entryIndex { get; protected set; } = -1;
    public int maxSize = 5;
    abstract public int nCases { get; }
    public List<Vector3> points = new List<Vector3>();
    public Dictionary<int, List<int>> graph { get; protected set; } = new Dictionary<int, List<int>>();
    public Dictionary<int, HashSet<int>> maze { get; protected set; } = new Dictionary<int, HashSet<int>>();

    abstract public Vector3 GetNormal(int index);
    abstract public Vector3 GetNorth(int index);

    abstract public int GetDepth(int index);
    abstract public int GetLevelSize(int index);

    abstract public int LevelSize(int levelSize);
    abstract public int Level2Stride(int levelSize);
    
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
                    Debug.LogError("Error: premature break at " + current);
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
                Debug.LogError("Error: premature break at " + current);
                break;
            }
        }
    }

    [ContextMenu("Average neighbor Distance")]
    public void AverageNeighborDistance() {
        float sum = 0;
        int count = 0;
        foreach (int key in graph.Keys) {
            foreach (int neighbor in graph[key]) {
                if (neighbor != -1) {
                    sum += Vector3.Distance(points[key], points[neighbor]);
                    count++;
                }
            }
        }
        Debug.Log("Average neighbor distance: " + sum / count);
    }

}
