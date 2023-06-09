using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMultilevelMaze : MonoBehaviour
{
    public int topSize = 5;
    public int bottomSize { get {
        return 2 + topSize % 2;
        }}
    [Range(0,5)]
    public int currentLevel = -1;
    public SimpleMaze currentMaze { get {
        return mazes[currentLevel];
        }}
    public int levelDecrease = 2;
    [SerializeField]
    public MeshFilter meshFilter;
    private List<Mesh> mazeMeshes = new List<Mesh>();
    private List<SimpleMaze> mazes = new List<SimpleMaze>();

    void Awake() {
        Initialize();
    }

    [ContextMenu("Initialize")]
    private void Initialize() {
        Clear();

        Vector3Int exit = new Vector3Int(Random.Range(0, bottomSize), Random.Range(0, bottomSize), Random.Range(0,6));
        for (int size = bottomSize; size <= topSize; size += levelDecrease) {
            SimpleMaze maze = new SimpleCubicMaze(size);
            maze.GenerateMaze(exit);
            exit = maze.entryIndex + new Vector3Int(1,1,0);
            mazes.Add(maze);
            // MeshFilter meshFilter = Instantiate(new MeshFilter(), Vector3.zero, Quaternion.identity, transform);
            // meshFilter.mesh = maze.GenerateMesh();
            mazeMeshes.Add(maze.GenerateMesh());
            // meshFilter.gameObject.SetActive(false);
            currentLevel++;
        }
        meshFilter.sharedMesh = mazeMeshes[currentLevel];
        // mazeMeshes[0].gameObject.SetActive(true);
    }

    [ContextMenu("Previous Level")]
    public void AscendLevel() {
        Debug.Log("Previous level");
        if (currentLevel < mazes.Count - 1) {
            currentLevel++;
            meshFilter.sharedMesh = mazeMeshes[currentLevel];
        }
    }

    [ContextMenu("Next Level")]
    public void DescendLevel() {
        Debug.Log("Next level");
        if (currentLevel > 0) {
            currentLevel--;
            meshFilter.sharedMesh = mazeMeshes[currentLevel];
        }
    }

    public SimpleMaze PeekMaze(int level) {
        if (level < 0 || level >= mazes.Count) return null;
        return mazes[level];
    }

    [ContextMenu("Clear")]
    private void Clear() {
        foreach (SimpleMaze maze in mazes) {
            maze.ClearMaze();
        }
        mazes.Clear();
        currentLevel = -1;
    }

    private void OnDrawGizmos()
    {
        if (mazes == null || mazes.Count <= currentLevel || currentLevel < 0) return;
        SimpleMaze maze = mazes[currentLevel];
        for (int i = 0; i < maze.points.Count && maze.graph.Count > 0; i++) {
            foreach (var neighbour in maze.graph[i])
            {
                if (neighbour == -1 || neighbour < i) continue;
                if (maze.maze.ContainsKey(i) && maze.maze[i].Contains(neighbour)) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(maze.points[i] + transform.position, maze.points[neighbour] + transform.position);
                } else {
                    // Gizmos.color = Color.red;
                    // Gizmos.DrawLine(maze.points[i] + transform.position, maze.points[neighbour] + transform.position);
                }
            }
        }
        int exitIndex = maze.Coordinates2Index(maze.exitIndex);
        int entryIndex = maze.Coordinates2Index(maze.entryIndex);
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(maze.points[exitIndex] + transform.position, 0.3f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(maze.points[entryIndex] + transform.position, 0.3f);
    }
}
