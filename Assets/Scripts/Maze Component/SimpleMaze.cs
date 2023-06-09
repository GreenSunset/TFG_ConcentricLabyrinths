using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SimpleMaze
{
    [Range(1, 100)]
    public int size = 10;
    public int nPlanes {get; protected set;} = 1;
    public Vector3Int entryIndex {get; protected set;} = new Vector3Int(0, 0, 0);
    public Vector3Int exitIndex {get; protected set;} = new Vector3Int(0, 0, 0);
    public List<Vector3> points = new List<Vector3>();
    public Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();
    public Dictionary<int, HashSet<int>> maze = new Dictionary<int, HashSet<int>>();

    public SimpleMaze(int size = 1) {
        this.size = size;
    }

    public bool AreValidCoordinates(Vector3Int coordinates) {
        return coordinates.x >= 0 && coordinates.x < size && coordinates.y >= 0 && coordinates.y < size && coordinates.z >= 0 && coordinates.z < nPlanes;
    }

    public Vector3Int Index2Coordinates(int index) {
        int z = index % nPlanes;
        index /= nPlanes;
        int y = index % size;
        index /= size;
        int x = index;
        if (x < 0 || x >= size)
            Debug.LogWarning("Index out of range: " + index);
        return new Vector3Int(x, y, z);
    }

    public int Coordinates2Index(Vector3Int coordinates) {
        if (coordinates.x < 0 || coordinates.x >= size)
            Debug.LogWarning("Coordinates X out of range: " + coordinates);
        if (coordinates.y < 0 || coordinates.y >= size)
            Debug.LogWarning("Coordinates Y out of range: " + coordinates);
        if (coordinates.z < 0 || coordinates.z >= nPlanes)
            Debug.LogWarning("Coordinates Z out of range: " + coordinates);
        return nPlanes * (coordinates.x * size + coordinates.y) + coordinates.z;
    }

    public void ClearMaze()
    {
        points.Clear();
        graph.Clear();
        maze.Clear();
    }

    [ContextMenu("Generate Maze")]
    public void GenerateMaze(Vector3Int exit)
    {
        ClearMaze();
        GenerateGraph();
        GenerateMaze(size, exit);
    }

    public virtual Vector3 GetNormal(int point) {
        return Vector3.up;
    }
    
    public virtual Quaternion GetRotation(int point) {
        return Quaternion.identity;
    }

    public virtual Mesh GenerateMesh() {
        if (points.Count == 0 || graph.Count == 0) return new Mesh();

        float r = 0.2f;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        // List<Vector3> normals = new List<Vector3>();
        Dictionary<int, List<int>> pointVertexMap = new Dictionary<int, List<int>>();
        List<Vector3> templateVertices = new List<Vector3>() {
            // new Vector3(-r, 0, 0),
            // new Vector3(0, 0, r),
            // new Vector3(0, 0, 0),
            new Vector3(-r, -r, -r),
            new Vector3(-r, -r, r),
            new Vector3(r, -r, r),
            new Vector3(r, -r, -r),
            new Vector3(-r, r, -r),
            new Vector3(-r, r, r),
            new Vector3(r, r, r),
            new Vector3(r, r, -r),
            
            new Vector3(-r, -r, -r),
            new Vector3(-r, -r, r),
            new Vector3(r, -r, r),
            new Vector3(r, -r, -r),
            new Vector3(-r, r, -r),
            new Vector3(-r, r, r),
            new Vector3(r, r, r),
            new Vector3(r, r, -r),
            new Vector3(-r, -r, -r),
            new Vector3(-r, -r, r),
            new Vector3(r, -r, r),
            new Vector3(r, -r, -r),
            new Vector3(-r, r, -r),
            new Vector3(-r, r, r),
            new Vector3(r, r, r),
            new Vector3(r, r, -r),
        };
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[i];
            Vector3 normal = GetNormal(i);
            Quaternion rotation = GetRotation(i);

            // Generate vertices around the point
            pointVertexMap.Add(i, new List<int>());
            for (int j = 0; j < templateVertices.Count; j++)
            {
                float angle = Vector3.Angle(Vector3.up, normal);
                Vector3 vertex = Quaternion.Inverse(rotation) * templateVertices[j];
                vertex += point;
                vertices.Add(vertex);
                // normals.Add(normal);
                pointVertexMap[i].Add(vertices.Count - 1);
            }

            if (i != Coordinates2Index(exitIndex) && i != Coordinates2Index(entryIndex)) {
                triangles.Add(templateVertices.Count * i);
                triangles.Add(templateVertices.Count * i + 1);
                triangles.Add(templateVertices.Count * i + 2);
                triangles.Add(templateVertices.Count * i + 2);
                triangles.Add(templateVertices.Count * i + 3);
                triangles.Add(templateVertices.Count * i + 0);
            }
            for (int startL = 0; startL < graph[i].Count; startL++) {
                int startR = (startL + 1) % 4;
                int neighbor = graph[i][startL];
                int next = graph[i][startR];
                if (neighbor < 0) continue;
                int endR = graph[neighbor].IndexOf(i);
                int endL = (endR + 1) % 4;
                if ((i < neighbor && i < next)) {
                    int cornerNext = (graph[next].IndexOf(i) + 1) % 4;
                    int last = graph[next][cornerNext];
                    if (i < last) {
                        int cornerLast = (graph[last].IndexOf(next) + 1) % 4;
                        triangles.Add(templateVertices.Count * i + startR + 4);
                        triangles.Add(templateVertices.Count * neighbor + endR + 4);
                        triangles.Add(templateVertices.Count * next + cornerNext + 4);
                        
                        triangles.Add(templateVertices.Count * neighbor + endR + 4);
                        triangles.Add(templateVertices.Count * last + cornerLast + 4);
                        triangles.Add(templateVertices.Count * next + cornerNext + 4);
                    }
                }
                if (neighbor > i) {
                    if (maze[i].Contains(graph[i][startL])) {
                        triangles.Add(templateVertices.Count * i + startL);
                        triangles.Add(templateVertices.Count * neighbor + endL);
                        triangles.Add(templateVertices.Count * i + startR);

                        triangles.Add(templateVertices.Count * i + startR);
                        triangles.Add(templateVertices.Count * neighbor + endL);
                        triangles.Add(templateVertices.Count * neighbor + endR);

                        triangles.Add(templateVertices.Count * i + startL + 4 + 8 * (1 + startL % 2));
                        triangles.Add(templateVertices.Count * neighbor + endL + 8 * (1 + endL % 2));
                        triangles.Add(templateVertices.Count * i + startL + 8 * (1 + startL % 2));

                        triangles.Add(templateVertices.Count * i + startL + 4 + 8 * (1 + startL % 2));
                        triangles.Add(templateVertices.Count * neighbor + endL + 4 + 8 * (1 + endL % 2));
                        triangles.Add(templateVertices.Count * neighbor + endL + 8 * (1 + endL % 2));
                        
                        triangles.Add(templateVertices.Count * i + startR + 4 + 8 * (1 + startL % 2));
                        triangles.Add(templateVertices.Count * i + startR + 8 * (1 + startL % 2));
                        triangles.Add(templateVertices.Count * neighbor + endR + 8 * (1 + endL % 2));
                        
                        triangles.Add(templateVertices.Count * i + startR + 4 + 8 * (1 + startL % 2));
                        triangles.Add(templateVertices.Count * neighbor + endR + 8 * (1 + endL % 2));
                        triangles.Add(templateVertices.Count * neighbor + endR + 4 + 8 * (1 + endL % 2));

                    }
                    else {
                        triangles.Add(templateVertices.Count * i + startL + 4);
                        triangles.Add(templateVertices.Count * neighbor + endL + 4);
                        triangles.Add(templateVertices.Count * i + startR + 4);

                        triangles.Add(templateVertices.Count * i + startR + 4);
                        triangles.Add(templateVertices.Count * neighbor + endL + 4);
                        triangles.Add(templateVertices.Count * neighbor + endR + 4);

                        triangles.Add(templateVertices.Count * i + startL + 8 * (1 + startR % 2));
                        triangles.Add(templateVertices.Count * i + startL + 4 + 8 * (1 + startR % 2));
                        triangles.Add(templateVertices.Count * i + startR + 4 + 8 * (1 + startR % 2));

                        triangles.Add(templateVertices.Count * i + startL + 8 * (1 + startR % 2));
                        triangles.Add(templateVertices.Count * i + startR + 4 + 8 * (1 + startR % 2));
                        triangles.Add(templateVertices.Count * i + startR + 8 * (1 + startR % 2));
                        
                        triangles.Add(templateVertices.Count * neighbor + endL + 8 * (1 + endR % 2));
                        triangles.Add(templateVertices.Count * neighbor + endR + 4 + 8 * (1 + endR % 2));
                        triangles.Add(templateVertices.Count * neighbor + endL + 4 + 8 * (1 + endR % 2));

                        triangles.Add(templateVertices.Count * neighbor + endL + 8 * (1 + endR % 2));
                        triangles.Add(templateVertices.Count * neighbor + endR + 8 * (1 + endR % 2));
                        triangles.Add(templateVertices.Count * neighbor + endR + 4 + 8 * (1 + endR % 2));
                        
                    }
                }
            }
        }
            
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
    
    protected virtual void GenerateGraph() {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                points.Add(new Vector3(i - (size-1)/2f, 0, j - (size-1)/2f));
                graph.Add(i * size + j, new List<int>());
                if (i > 0) graph[i * size + j].Add((i-1) * size + j);
                if (j > 0) graph[i * size + j].Add(i * size + j - 1);
                if (i < size - 1) graph[i * size + j].Add((i+1) * size + j);
                if (j < size - 1) graph[i * size + j].Add(i * size + j + 1);
            }
        }
    }

    protected virtual void GenerateMaze(int size, Vector3Int exit) {
        if (points.Count == 0 || this.graph.Count == 0) return;
        exitIndex = exit;
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
        // entryIndex = exit + new Vector3Int(exit.x == (size - 1) ? -1 : 1, exit.y == (size - 1) ? -1 : 1, 0);
        entryIndex = new Vector3Int(Random.Range(0, size), Random.Range(0, size), Random.Range(0, nPlanes));
    }

    protected void AddEdge(int a, int b) {
        if (!graph.ContainsKey(a)) graph.Add(a, new List<int>());
        if (!graph.ContainsKey(b)) graph.Add(b, new List<int>());
        graph[a].Add(b);
        graph[b].Add(a);
    }
}
