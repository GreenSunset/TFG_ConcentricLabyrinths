using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MultilevelMaze))]
public class MazeVisualizer : MonoBehaviour
{
    public bool seeCompass = false;
    public bool seePoints = false;
    public bool seeGraph = false;
    public bool seeMaze = true;
    public MultilevelMaze maze;
    [Range(0, 8)]
    public float currentLevel = 0;
    // Start is called before the first frame update
    void Awake()
    {
        maze = GetComponent<MultilevelMaze>();
    }

    void OnDrawGizmos() {
        if (maze == null) {
            return;
        }
        Gizmos.color = Color.red;
        int startSize = maze.maxSize % maze.nCases == 0 ? maze.nCases : maze.maxSize % maze.nCases;
        float focusSize = maze.nCases * currentLevel + startSize;
        int currentSize = Mathf.FloorToInt(focusSize);
        int startIndex = maze.Level2Stride(currentSize);
        int endIndex = maze.Level2Stride(currentSize + 2);
        int nPoints = maze.points.Count;
        for (int i = 0; i < nPoints; i++) {
            MultilevelCubicMaze.Coordinate coord = maze.Idx2Coord(i, maze.maxSize);
            int idxSize = coord.levelSize;
            float ratio = Mathf.Abs(idxSize - focusSize) / 3f;
            float size = Mathf.Lerp(0.1f, 0.05f, ratio);
            Color color = Color.black;
            color.a = Mathf.Lerp(1f, 0.2f, ratio);
            Gizmos.color = color;
            if (seePoints) Gizmos.DrawSphere(maze.points[i], size);
            if (seeGraph) {
                if (!maze.graph.ContainsKey(i)) continue;
                foreach (int neighbor in maze.graph[i]) {
                    if (neighbor >= 0) {
                        Gizmos.DrawLine(maze.points[i], (maze.points[i] + maze.points[neighbor]) / 2);
                    }
                }
            }
            color = Color.Lerp(Color.red, Color.black, ratio);
            color.a = Mathf.Lerp(1f, 0.2f, ratio);
            Gizmos.color = color;
            if (seeMaze) {
                if (!maze.maze.ContainsKey(i)) continue;
                foreach (int neighbor in maze.maze[i]) {
                    if (neighbor >= 0) {
                        Gizmos.DrawLine(maze.points[i], (maze.points[i] + maze.points[neighbor]) / 2);
                    }
                }
            }
            
        }
        //// Compass
        if (seeCompass) {
            Color[] colors = new Color[] {Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta};
            for (int i = 0; i < maze.faces.Length; i++) {
                Vector3 up = maze.faces[i] * Vector3.up * 2;
                Vector3 north = maze.faces[i] * Vector3.forward;
                Vector3 east = maze.faces[i] * Vector3.right;
                Gizmos.color = colors[0];
                Gizmos.DrawLine(up, up - north);
                Gizmos.color = colors[1];
                Gizmos.DrawLine(up, up - east);
                Gizmos.color = colors[2];
                Gizmos.DrawLine(up, up + north);
                Gizmos.color = colors[3];
                Gizmos.DrawLine(up, up + east);
                Gizmos.color = colors[4];
                Gizmos.DrawLine(up, up + up / 4);
            }
        }
    }
}
