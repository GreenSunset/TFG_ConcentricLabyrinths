using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{
    public bool showNeighbours = false;
    public GameObject wallPrefab;
    public List<Vector3> points = new List<Vector3>();
    public List<Vector2Int> edges = new List<Vector2Int>();
    public Dictionary<int, HashSet<int>> maze = new Dictionary<int, HashSet<int>>();

    void Start()
    {
        GenerateMaze();
        for (int i = 0; i < edges.Count; i++)
        {
            Vector2Int edge = edges[i];
            if (maze.ContainsKey(edge.x) && maze[edge.x].Contains(edge.y)) continue;
            Vector3 point1 = points[edge.x] + transform.position;
            Vector3 point2 = points[edge.y] + transform.position;
            Vector3 direction = point2 - point1;
            Vector3 position = point1 + direction * 0.5f;
            // GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
            // wall.transform.Rotate(0, Vector3.SignedAngle(Vector3.forward, direction, Vector3.up), 0);
        }
    }

    [ContextMenu("Clear")]
    public void ClearMaze()
    {
        points.Clear();
        edges.Clear();
        maze.Clear();
    }

    [ContextMenu("Generate Maze")]
    public void GenerateMaze()
    {
        ClearMaze();
        int size = 10;
        GenerateGraph(size);
        GenerateMaze(size);
    }

    protected virtual void GenerateGraph(int size) {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                points.Add(new Vector3(i - (size-1)/2f, 0, j - (size-1)/2f));
                if (i > 0) edges.Add(new Vector2Int(i * size + j, (i - 1) * size + j));
                if (j > 0) edges.Add(new Vector2Int(i * size + j, i * size + j - 1));
            }
        }
    }

    protected virtual void GenerateMaze(int size) {
        if (points.Count == 0 || edges.Count == 0) return;
        Dictionary<int, HashSet<int>> graph = new Dictionary<int, HashSet<int>>();
        foreach (var edge in edges)
        {
            if (!graph.ContainsKey(edge.x)) graph.Add(edge.x, new HashSet<int>());
            if (!graph.ContainsKey(edge.y)) graph.Add(edge.y, new HashSet<int>());
            graph[edge.x].Add(edge.y);
            graph[edge.y].Add(edge.x);
        }
        List<int> visited = new List<int>();
        List<int> stack = new List<int>();
        int current = Random.Range(0, points.Count);
        visited.Add(current);
        while (visited.Count < points.Count)
        {
            List<int> neighbors = new List<int>(graph[current]);
            neighbors.RemoveAll(x => visited.Contains(x));
            if (neighbors.Count > 0)
            {
                int next = neighbors[Random.Range(0, neighbors.Count)];
                stack.Add(current);
                if (!maze.ContainsKey(current)) maze.Add(current, new HashSet<int>());
                if (!maze.ContainsKey(next)) maze.Add(next, new HashSet<int>());
                maze[current].Add(next);
                maze[next].Add(current);
                current = next;
                visited.Add(current);
            }
            else
            {
                current = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (maze == null) return;
        for (int i = 0; showNeighbours && i < edges.Count; i++)
        {
            Vector2Int edge = edges[i];
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(points[edge.x] + transform.position, points[edge.y] + transform.position);
        }
        for (int i = 0; i < points.Count; i++) {
            if (!maze.ContainsKey(i)) continue;
            Gizmos.color = Color.green;
            foreach (var neighbour in maze[i])
            {
                Gizmos.DrawLine(points[i] + transform.position, points[neighbour] + transform.position);
            }
        }
        Gizmos.color = Color.red;
        foreach (var point in points)
        {
            Gizmos.DrawSphere(point + transform.position, 0.1f);
        }
    }
}
