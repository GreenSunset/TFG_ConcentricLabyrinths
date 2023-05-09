using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Maze : MonoBehaviour
{
    public bool showNeighbours = false;
    public GameObject wallPrefab;
    [Range(1, 100)]
    public int size = 10;
    public int exitIndex {get; protected set;} = -2;
    [SerializeField] private MeshFilter meshFilter;
    public List<Vector3> points = new List<Vector3>();
    public Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();
    public Dictionary<int, HashSet<int>> maze = new Dictionary<int, HashSet<int>>();

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        GenerateMaze();
    }

    [ContextMenu("Clear")]
    public void ClearMaze()
    {
        points.Clear();
        graph.Clear();
        maze.Clear();
        meshFilter.sharedMesh = null;
    }

    [ContextMenu("Generate Maze")]
    public void GenerateMaze()
    {
        ClearMaze();
        GenerateGraph();
        GenerateMaze(size);
        GenerateMesh();
    }

    public virtual Vector3 GetNormal(int point) {
        return transform.up;
    }
    
    public virtual Quaternion GetRotation(int point) {
        return Quaternion.identity;
    }

    protected virtual void GenerateMesh() {
        if (points.Count == 0 || graph.Count == 0) return;

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
                float angle = Vector3.Angle(transform.up, normal);
                Vector3 vertex = Quaternion.Inverse(rotation) * templateVertices[j];
                vertex += point;
                vertices.Add(vertex);
                // normals.Add(normal);
                pointVertexMap[i].Add(vertices.Count - 1);
            }

            triangles.Add(templateVertices.Count * i);
            triangles.Add(templateVertices.Count * i + 1);
            triangles.Add(templateVertices.Count * i + 2);
            triangles.Add(templateVertices.Count * i + 2);
            triangles.Add(templateVertices.Count * i + 3);
            triangles.Add(templateVertices.Count * i + 0);
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

                        triangles.Add(templateVertices.Count * i + startL + 4);
                        triangles.Add(templateVertices.Count * neighbor + endL);
                        triangles.Add(templateVertices.Count * i + startL);

                        triangles.Add(templateVertices.Count * i + startL + 4);
                        triangles.Add(templateVertices.Count * neighbor + endL + 4);
                        triangles.Add(templateVertices.Count * neighbor + endL);
                        
                        triangles.Add(templateVertices.Count * i + startR + 4);
                        triangles.Add(templateVertices.Count * i + startR);
                        triangles.Add(templateVertices.Count * neighbor + endR);
                        
                        triangles.Add(templateVertices.Count * i + startR + 4);
                        triangles.Add(templateVertices.Count * neighbor + endR);
                        triangles.Add(templateVertices.Count * neighbor + endR + 4);

                    }
                    else {
                        triangles.Add(templateVertices.Count * i + startL + 4);
                        triangles.Add(templateVertices.Count * neighbor + endL + 4);
                        triangles.Add(templateVertices.Count * i + startR + 4);

                        triangles.Add(templateVertices.Count * i + startR + 4);
                        triangles.Add(templateVertices.Count * neighbor + endL + 4);
                        triangles.Add(templateVertices.Count * neighbor + endR + 4);

                        triangles.Add(templateVertices.Count * i + startL);
                        triangles.Add(templateVertices.Count * i + startL + 4);
                        triangles.Add(templateVertices.Count * i + startR + 4);

                        triangles.Add(templateVertices.Count * i + startL);
                        triangles.Add(templateVertices.Count * i + startR + 4);
                        triangles.Add(templateVertices.Count * i + startR);
                        
                        triangles.Add(templateVertices.Count * neighbor + endL);
                        triangles.Add(templateVertices.Count * neighbor + endR + 4);
                        triangles.Add(templateVertices.Count * neighbor + endL + 4);

                        triangles.Add(templateVertices.Count * neighbor + endL);
                        triangles.Add(templateVertices.Count * neighbor + endR);
                        triangles.Add(templateVertices.Count * neighbor + endR + 4);
                        
                    }
                }
            }
            // Connect to already visited points
            foreach (int j in graph[i]) {
                if (j > i && j % 6 == i % 6) {
                    // calculate neighbourhood direction
                    Vector3 direction = points[j] - point;

                    // connect corresponding vertices
                }
            }
        }
            
        meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh.vertices = vertices.ToArray();
        meshFilter.sharedMesh.triangles = triangles.ToArray();
        meshFilter.sharedMesh.RecalculateNormals();
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

    protected virtual void GenerateMaze(int size) {
        if (points.Count == 0 || this.graph.Count == 0) return;
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
        exitIndex = Random.Range(0, points.Count);
    }

    private void OnDrawGizmos()
    {
        if (maze == null) return;
        for (int i = 0; i < points.Count && graph.Count > 0; i++) {
            foreach (var neighbour in graph[i])
            {
                if (neighbour == -1 || neighbour < i) continue;
                if (maze.ContainsKey(i) && maze[i].Contains(neighbour)) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(points[i] + transform.position, points[neighbour] + transform.position);
                } else {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(points[i] + transform.position, points[neighbour] + transform.position);
                }
            }
        }
        Gizmos.color = Color.red;
        for (int i = 0; i < points.Count; i++) {
            if (i == exitIndex) {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(points[i] + transform.position, 0.1f);
                Gizmos.color = Color.red;
            } else {
                Gizmos.DrawSphere(points[i] + transform.position, 0.1f);
            }
        }
        // foreach (var point in points)
        // {
        //     Gizmos.DrawSphere(point + transform.position, 0.1f);
        // }
    }

    protected void AddEdge(int a, int b) {
        if (!graph.ContainsKey(a)) graph.Add(a, new List<int>());
        if (!graph.ContainsKey(b)) graph.Add(b, new List<int>());
        graph[a].Add(b);
        graph[b].Add(a);
    }
}
