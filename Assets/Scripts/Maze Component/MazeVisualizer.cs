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

    public Material material;

    [Range(0, 8)]
    public float currentLevel = 0;
    public int intCurrentLevel = 0;
    private float lastLevel = 0;
    private Dictionary<int, GameObject> levels = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        levels.Clear();
        maze = GetComponent<MultilevelMaze>();
        maze.Generate();
        currentLevel = (maze.maxSize - 1) / maze.nCases;
        intCurrentLevel = Mathf.FloorToInt(currentLevel);
        lastLevel = currentLevel;
        GenerateMesh(intCurrentLevel);
    }

    void Update() {
        if (intCurrentLevel != Mathf.FloorToInt(currentLevel)) {
            foreach (KeyValuePair<int, GameObject> level in levels) {
                level.Value.SetActive(false);
            }
            intCurrentLevel = Mathf.FloorToInt(currentLevel);
            GenerateMesh(intCurrentLevel);
            if (levels.ContainsKey(intCurrentLevel)) {
                levels[intCurrentLevel].SetActive(true);
                levels[intCurrentLevel].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 1);
            }
        }
        // if (lastLevel != currentLevel) {
        //     lastLevel = currentLevel;
        //     int otherLevel = intCurrentLevel - 1;
        //     float alpha = 1 - (currentLevel - intCurrentLevel);
        //     if (alpha < .9f) {
        //         alpha = 1 - alpha;
        //         otherLevel = intCurrentLevel + 1;
        //     } else {
        //         alpha = 1;
        //     }
        //     GenerateMesh(otherLevel);
        //     if (levels.ContainsKey(otherLevel)) {
        //         levels[otherLevel].SetActive(true);
        //         levels[otherLevel].GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, alpha);
        //     }
        // }
    }

    // [ContextMenu("Generate Mesh")]
    void GenerateMesh(int level = -1) {
        if (maze == null || levels.ContainsKey(level) || level < 0 || level > (maze.maxSize - 1) / maze.nCases) {
            return;
        }
        Debug.Log("Generating mesh for level " + level);
        int startIndex = maze.Level2Stride(level);
        int endIndex = maze.Level2Stride(level + 1);
        Dictionary<int, int> vertexStart = new Dictionary<int, int>();
        List<Vector3> vertices = new List<Vector3>();
        List<GameObject> lights = new List<GameObject>();
        List<int> triangles = new List<int>();
        int[] list = new int[]{0,0,0, 0};
        for (int i = startIndex; i < endIndex; i++) {
            vertexStart.Add(i, vertices.Count);
            List<int> neighbors = new List<int>(maze.graph[i]);
            Vector3 normal = maze.GetNormal(i);
            Vector3 north = maze.GetNorth(i);
            if (maze.maze[i].Contains(neighbors[neighbors.Count - 2])) {
                GameObject light = new GameObject("Light " + i, typeof(Light));
                Light l = light.GetComponent<Light>();
                l.type = LightType.Spot;
                l.spotAngle = 20;
                l.range = 1.4f;
                l.intensity = 5f;
                light.transform.position = maze.points[i] + normal * 1f;
                light.transform.forward = -normal;
                lights.Add(light);

            }
            neighbors.RemoveAll(x => x < startIndex || x >= endIndex);
            List<float> angles = neighbors.ConvertAll(x => Vector3.SignedAngle(north, Vector3.ProjectOnPlane(maze.points[x] - maze.points[i], normal), normal));
            List<Vector3> extra = new List<Vector3>();
            bool isExit = maze.maze[i].Contains(maze.graph[i][maze.graph[i].Count - 1]);
            for (int j = 0, prev = neighbors.Count - 1; j < neighbors.Count; prev = j, j++) {
                Vector3 pointNormal = Vector3.zero;
                List<Vector3> normals = new List<Vector3>(new HashSet<Vector3>() {
                    normal,
                    maze.GetNormal(neighbors[j]),
                    maze.GetNormal(neighbors[prev]),
                });
                foreach (Vector3 n in normals) pointNormal += n;
                pointNormal /= normals.Count;
                float angle = Mathf.LerpAngle(angles[j], angles[prev], 0.5f);
                vertices.Add(maze.points[i] + Quaternion.AngleAxis(angle, normal) * north * 0.25f - normal * .2f);
                vertices.Add(maze.points[i] + Quaternion.AngleAxis(angle, normal) * north * 0.45f + pointNormal * .2f);
                vertices.Add(maze.points[i] + Quaternion.AngleAxis(angle, normal) * north * 0.3f - normal * .15f);
                vertices.Add(maze.points[i] + Quaternion.AngleAxis(angle, normal) * north * 0.4f + pointNormal * .15f);
                // vertices.Add(maze.points[i] + Quaternion.AngleAxis(angle, normal) * north * 0.3f - normal * .2f);
                // vertices.Add(maze.points[i] + Quaternion.AngleAxis(angle, normal) * north * 0.4f + pointNormal * .2f);
                // vertices.Add(maze.points[i] + Quaternion.AngleAxis(angle, normal) * north * 0.3f - normal * .2f);
                // vertices.Add(maze.points[i] + Quaternion.AngleAxis(angle, normal) * north * 0.4f + pointNormal * .2f);
                if (isExit) {
                    extra.Add(maze.points[i] + Quaternion.AngleAxis(angle, normal) * north * 0.15f - normal * .3f);
                }
                if (neighbors[j] < i) {
                    List<int> neighbors2 = new List<int>(maze.graph[neighbors[j]]);
                    neighbors2.RemoveAll(x => x < startIndex || x >= endIndex);
                    int opposite = neighbors2.IndexOf(i);
                    if (opposite < 0) {
                        Debug.LogError("Opposite not found");
                        continue;
                    }
                    if (maze.maze[i].Contains(neighbors[j])) {
                        // Path
                        triangles.Add(vertexStart[i] + 4 * j);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count));
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite);
                        triangles.Add(vertexStart[i] + 4 * j);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count));

                        // Walls
                        triangles.Add(vertexStart[i] + 4 * j + 2);
                        triangles.Add(vertexStart[i] + 4 * j + 3);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 3);
                        triangles.Add(vertexStart[i] + 4 * j + 2);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 3);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 2);
                        
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 2);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 3);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 3);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 2);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 2);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 3);

                        // Bevel
                        triangles.Add(vertexStart[i] + 4 * j + 0);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 2);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 0);
                        triangles.Add(vertexStart[i] + 4 * j + 0);
                        triangles.Add(vertexStart[i] + 4 * j + 2);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 2);

                        triangles.Add(vertexStart[i] + 4 * j + 1);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 1);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 3);
                        triangles.Add(vertexStart[i] + 4 * j + 1);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 3);
                        triangles.Add(vertexStart[i] + 4 * j + 3);
                        
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 0);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 0);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 2);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 0);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 2);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 2);

                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 1);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 3);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 1);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 1);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 3);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 3);

                    }
                    else {
                        // Top
                        triangles.Add(vertexStart[i] + 4 * j + 1);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 1);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 1);
                        triangles.Add(vertexStart[i] + 4 * j + 1);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 1);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 1);

                        // Walls
                        triangles.Add(vertexStart[i] + 4 * j + 2);
                        triangles.Add(vertexStart[i] + 4 * j + 3);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 3);
                        triangles.Add(vertexStart[i] + 4 * j + 2);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 3);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 2);

                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 2);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 3);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 3);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 2);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 3);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 2);

                        
                        // Bevel
                        triangles.Add(vertexStart[i] + 4 * j + 0);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 2);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 0);
                        triangles.Add(vertexStart[i] + 4 * j + 0);
                        triangles.Add(vertexStart[i] + 4 * j + 2);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 2);

                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 0);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 2);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 0);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 0);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 2);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 2);

                        triangles.Add(vertexStart[i] + 4 * j + 1);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 1);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 3);
                        triangles.Add(vertexStart[i] + 4 * j + 1);
                        triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count) + 3);
                        triangles.Add(vertexStart[i] + 4 * j + 3);

                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 1);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 1);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 3);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 1);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * ((opposite + 1) % neighbors2.Count) + 3);
                        triangles.Add(vertexStart[neighbors[j]] + 4 * opposite + 3);
                    }
                    int next = neighbors[j];
                    List<int> cell = new List<int>() {
                        vertexStart[i] + 4 * j + 1,
                        vertexStart[next] + 4 * ((opposite + 1) % neighbors2.Count) + 1,
                    };
                    bool generateCell = true;
                    for (int lim = 7; cell.Count < lim;) {
                        int last = neighbors2[(opposite + 1) % neighbors2.Count];
                        if (last == i) break;
                        else if (last > i) {
                            generateCell = false;
                            break;
                        }
                        neighbors2 = new List<int>(maze.graph[last]);
                        neighbors2.RemoveAll(x => x < startIndex || x >= endIndex);
                        opposite = neighbors2.IndexOf(next);
                        next = last;
                        if (opposite < 0) {
                            Debug.LogError("Opposite not found: " + neighbors2.Count);
                            generateCell = false;
                            break;
                        }
                        cell.Add(vertexStart[next] + 4 * ((opposite + 1) % neighbors2.Count) + 1);
                    }
                    if (generateCell) {
                        for (int k = 0; k < cell.Count - 2; k++) {
                            triangles.Add(cell[0]);
                            triangles.Add(cell[k + 2]);
                            triangles.Add(cell[k + 1]);
                        }
                    }
                }
            }
            vertices.AddRange(extra);
            if (isExit) {
                for (int j = 0; j < neighbors.Count; j++) {
                    triangles.Add(vertexStart[i] + 4 * j);
                    triangles.Add(vertexStart[i] + 4 * ((j + 1) % neighbors.Count));
                    triangles.Add(vertexStart[i] + ((j + 1) % neighbors.Count) + 4 * neighbors.Count);
                    triangles.Add(vertexStart[i] + 4 * j);
                    triangles.Add(vertexStart[i] + ((j + 1) % neighbors.Count) + 4 * neighbors.Count);
                    triangles.Add(vertexStart[i] + j + 4 * neighbors.Count);
                }
            }
            else {
                for (int j = 0; j < neighbors.Count - 2; j++) {
                    triangles.Add(vertexStart[i] + 0);
                    triangles.Add(vertexStart[i] + 4 * (j + 1));
                    triangles.Add(vertexStart[i] + 4 * (j + 2));
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        levels[level] = new GameObject("Level " + level, typeof(MeshFilter), typeof(MeshRenderer));
        levels[level].transform.parent = transform;
        foreach (GameObject light in lights) {
            light.transform.parent = levels[level].transform;
        }
        levels[level].GetComponent<MeshFilter>().mesh = mesh;
        levels[level].GetComponent<MeshRenderer>().material = material;
    }

    void OnDrawGizmos() {
        if (!seeGraph && !seeMaze && !seePoints) {
            return;
        }
        if (maze == null) {
            return;
        }
        int startIndex = maze.Level2Stride(Mathf.FloorToInt(currentLevel));
        int endIndex = maze.Level2Stride(Mathf.FloorToInt(currentLevel) + 1);
        int nPoints = maze.points.Count;
        for (int i = 0; i < nPoints; i++) {
            int idxLevel = maze.GetLevel(i);
            float ratio = Mathf.Abs(idxLevel - currentLevel) / 1.5f;
            float size = Mathf.Lerp(0.1f, 0.05f, ratio);
            Color color = Color.black;
            color.a = Mathf.Lerp(1f, 0f, ratio);
            Gizmos.color = color;
            if (seePoints) Gizmos.DrawSphere(maze.transform.TransformPoint(maze.points[i]), size);
            if (seeGraph) {
                if (!maze.graph.ContainsKey(i)) continue;
                foreach (int neighbor in maze.graph[i]) {
                    if (neighbor >= 0) {
                        Gizmos.DrawLine(maze.transform.TransformPoint(maze.points[i]), maze.transform.TransformPoint((maze.points[i] + maze.points[neighbor]) / 2));
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
                        Gizmos.DrawLine(maze.transform.TransformPoint((maze.points[i])), maze.transform.TransformPoint((maze.points[i] + maze.points[neighbor]) / 2));
                    }
                }
            }
            
        }
        if (seeCompass && maze.GetType() == typeof(MultilevelSolidMaze)) {
            MultilevelSolidMaze solidMaze = (MultilevelSolidMaze)maze;
            Color[] colors = new Color[] {Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta};
            for (int i = 0; i < solidMaze.faces.Length; i++) {
                Vector3 up = solidMaze.transform.TransformPoint(solidMaze.faces[i] * Vector3.up * 2);
                Vector3 north = solidMaze.transform.TransformPoint(solidMaze.faces[i] * Vector3.forward);
                Vector3 east = solidMaze.transform.TransformPoint(solidMaze.faces[i] * Vector3.right);
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
