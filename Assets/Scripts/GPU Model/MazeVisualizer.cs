using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class MazeVisualizer : MonoBehaviour
{
    public Maze maze;
    public ComputeShader visShader;
    private RenderTexture renderTexture;
    private RenderTexture dstTexture;
    [Range(0, 1)]
    public float ratio = 1;
    private Camera cam;
    private ComputeBuffer pointBuffer;

    void Start()
    {
        cam = GetComponent<Camera>();
        SetUpMaze();
    }

    [ContextMenu("Setup Maze")]
    public void SetUpMaze() {
        if (maze == null || maze.points == null || maze.maze == null) {
            return;
        }
        Vector3[] points = maze.points.ToArray();
        int[] adjTable = new int [points.Length * points.Length];
        for (int i = 0; i < points.Length; i++) {
            Debug.Log("Point " + i + ": " + points[i]);
            for (int j = 0; j < points.Length; j++) {
                adjTable[i * points.Length + j] = maze.maze.ContainsKey(i) && maze.maze[i].Contains(j) ? 1 : 0;
            }
        }
        int pointSize = 3 * sizeof(float);
        // int adjSize = sizeof(int);
        if (pointBuffer != null) pointBuffer.Dispose();
        pointBuffer = new ComputeBuffer(points.Length, pointSize);
        // ComputeBuffer adjBuffer = new ComputeBuffer(adjTable.Length, adjSize);
        pointBuffer.SetData(points);
        // adjBuffer.SetData(adjTable);
        visShader.SetBuffer(0, "Points", pointBuffer);
        // visShader.SetBuffer(0, "Adjacency", adjBuffer);
        visShader.SetInt("PointCount", points.Length);
        visShader.SetFloat("MazeRadius", 5.2f);
        visShader.SetVector("MazePos", maze.transform.position);

        // adjBuffer.Dispose();        
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(src.width * 2, src.height * 2, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }
        if (dstTexture == null)
        {
            dstTexture = new RenderTexture(src.width, src.height, 24);
            dstTexture.enableRandomWrite = true;
            dstTexture.Create();
        }
        visShader.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        visShader.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);

        visShader.SetTexture(0, "Source", src);
        visShader.SetTexture(0, "RayMarch", renderTexture);
        visShader.SetTexture(0, "Destination", dstTexture);
        visShader.SetFloat("Ratio", ratio);
 
        int threadGroupsX = Mathf.CeilToInt(renderTexture.width / 32.0f);
        int threadGroupsY = Mathf.CeilToInt(renderTexture.height / 32.0f);
        visShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        threadGroupsX = Mathf.CeilToInt(dstTexture.width / 32.0f);
        threadGroupsY = Mathf.CeilToInt(dstTexture.height / 32.0f);
        visShader.Dispatch(1, threadGroupsX, threadGroupsY, 1);
        Graphics.Blit(dstTexture, dest);
    }
}
