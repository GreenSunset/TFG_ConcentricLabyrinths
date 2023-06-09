using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObserverController : MonoBehaviour
{
    static public SimpleObserverController main { get; private set; }
    public SimpleMazeNavigator target;
    public SimpleMultilevelMaze maze;
    private Camera cam;
    private Vector2 currentTilt = Vector2.zero;
    public float currentZoom = 0;
    [SerializeField] private float tiltSpeed = 10;

    void Start()
    {
        SetMain();
        
        cam = GetComponentInChildren<Camera>();
    }

    private void SetMain() {
        if (main == null) main = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null || maze == null) return;
        Vector3 targetPos = target.transform.position;
        Vector3 centerPos = maze.transform.position;
        Vector3 normal = Vector3.Lerp(maze.currentMaze.GetNormal(target.currentPointIndex), maze.currentMaze.GetNormal(target.nextPointIndex), target.current2nextRatio);

        // Look at target
        float rotation = InputMapper.AxisZ();
        if (Mathf.Abs(rotation) > .2) {
            transform.RotateAround(targetPos, normal, rotation * Time.deltaTime * 100);
        }

        // Follow target
        float distance = (maze.currentMaze.size + 1) / 2;
        transform.position = targetPos + normal.normalized * distance;
        Vector3 axis = Vector3.Cross(transform.forward, -normal);
        float angle = Vector3.Angle(transform.forward, -normal);
        transform.RotateAround(centerPos, axis, angle);

        // Camera tilt
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localRotation = Quaternion.identity;
        float tiltU = -InputMapper.AxisU();
        if (Mathf.Abs(tiltU) > .2) {
            currentTilt.x = Mathf.Lerp(currentTilt.x, tiltU, Time.deltaTime * tiltSpeed);
        } else {
            currentTilt.x = Mathf.Lerp(currentTilt.x, 0, Time.deltaTime * tiltSpeed);
        }
        float tiltV = InputMapper.AxisV();
        if (Mathf.Abs(tiltV) > .2) {
            currentTilt.y = Mathf.Lerp(currentTilt.y, tiltV, Time.deltaTime * tiltSpeed);
        } else {
            currentTilt.y = Mathf.Lerp(currentTilt.y, 0, Time.deltaTime * tiltSpeed);
        }
        cam.transform.RotateAround(targetPos, transform.up, currentTilt.x * 80);
        cam.transform.RotateAround(targetPos, transform.right, currentTilt.y * 80);

        // Camera zoom
        float zoom = InputMapper.AxisW();
        if (Mathf.Abs(zoom) > .2) {
            currentZoom = Mathf.Clamp(currentZoom + zoom * Time.deltaTime, -1, 1);
        }
        cam.fieldOfView = currentZoom * 30 + 60;
    }
}
