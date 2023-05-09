using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class CameraShaderRenderer : MonoBehaviour
{
    [System.Serializable]
    public struct CapsuleDat {
        public Vector3 pos1;
        public Vector3 pos2;
        public float radius;
    }
    [System.Serializable]
    public struct ShapeDat {
        public Vector3 position;
        public float size;
        public int type;
        public Color color;
    }

    [System.Serializable]
    public struct LightData {
        public Vector3 direction;
        public float intensity;
    }


    public ComputeShader computeShader;

    private RenderTexture renderTexture;
    [Range(0, 1)]
    public float ratio = 1;
    private Camera cam;

    public ShapeDat[] shapes = new ShapeDat[1];
    public CapsuleDat[] capsules = new CapsuleDat[1];

    public LightData[] lights = new LightData[1];
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update() {
        for (int i = 0; i < lights.Length; i++) {
            Debug.DrawRay(Vector3.zero, lights[i].direction.normalized * -10, Color.black);
        }

        // int size = 8;
        // for (float i = -size; i < size; i++) {
        //     for (float j = -size; j < size; j++) {
        //         Vector4 ori4 = (cam.cameraToWorldMatrix * new Vector4(0,0,0,1));
        //         Vector4 dir4 = cam.projectionMatrix.inverse * new Vector4(j / size, i / size, 0, 0);
        //         dir4.w = 0;
        //         dir4 = cam.cameraToWorldMatrix * dir4;
        //         Debug.DrawRay(new Vector3(ori4.x, ori4.y, ori4.z), new Vector3(dir4.x, dir4.y, dir4.z).normalized,new Color(i / size, j / size, 1));
        //     }
        // }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(src.width, src.height, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }
        computeShader.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        computeShader.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);

        int shapeSize = 8 * sizeof(float) + sizeof(int);
        ComputeBuffer shapeBuffer = new ComputeBuffer(shapes.Length, shapeSize);
        shapeBuffer.SetData(shapes);
        computeShader.SetBuffer(0, "Shapes", shapeBuffer);
        computeShader.SetInt("ShapeCount", shapes.Length);

        int capsuleSize = 7 * sizeof(float);
        ComputeBuffer capsuleBuffer = new ComputeBuffer(capsules.Length, capsuleSize);
        capsuleBuffer.SetData(capsules);
        computeShader.SetBuffer(0, "Capsules", capsuleBuffer);
        computeShader.SetInt("CapsuleCount", capsules.Length);

        int lightSize = 4 * sizeof(float);
        ComputeBuffer lightBuffer = new ComputeBuffer(lights.Length, lightSize);
        lightBuffer.SetData(lights);
        computeShader.SetBuffer(0, "Lights", lightBuffer);
        computeShader.SetInt("LightCount", lights.Length);

        computeShader.SetTexture(0, "Source", src);
        computeShader.SetTexture(0, "Destination", renderTexture);
        computeShader.SetFloat("Ratio", ratio);
 
        int threadGroupsX = Mathf.CeilToInt(renderTexture.width / 32.0f);
        int threadGroupsY = Mathf.CeilToInt(renderTexture.height / 32.0f);
        computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        Graphics.Blit(renderTexture, dest);
        shapeBuffer.Dispose();
        capsuleBuffer.Dispose();
        lightBuffer.Dispose();
    }
}
