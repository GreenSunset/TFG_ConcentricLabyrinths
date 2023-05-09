using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraExploring : MonoBehaviour
{

    public float speed = 1f;
    public float turningSpeed = 10f;

    // Update is called once per frame
    void Update()
    {
        float u = InputMapper.AxisU();
        float v = -InputMapper.AxisV();

        transform.Translate(InputMapper.MainAxis() * speed * Time.deltaTime);
        if (u != 0) {
            transform.RotateAround(transform.position, Vector3.up, u * turningSpeed * Time.deltaTime);
        }
        if (v < 0 && Vector3.Angle(transform.forward, Vector3.up) > 10 || v > 0 && Vector3.Angle(transform.forward, Vector3.up) < 170) {
            transform.RotateAround(transform.position, transform.right, v * turningSpeed * Time.deltaTime);
        }
    }
}
