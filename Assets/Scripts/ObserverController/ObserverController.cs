using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverController : MonoBehaviour
{
    public Transform player;
    public Transform center;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || center == null) return;
        Vector3 playerPos = player.position;
        Vector3 centerPos = center.position;
        Vector3 direction = playerPos - centerPos;
        float distance = (direction.magnitude + 1) / 2 + direction.magnitude;
        transform.position = centerPos + direction.normalized * distance;
        Vector3 newLookAt = playerPos - transform.position;
        Vector3 axis = Vector3.Cross(transform.forward, newLookAt);
        float angle = Vector3.Angle(transform.forward, newLookAt);
        transform.RotateAround(centerPos, axis, angle);

        float rotation = InputMapper.AxisZ();
        if (Mathf.Abs(rotation) > .2) {
            transform.RotateAround(centerPos, Vector3.up, rotation * Time.deltaTime * 100);
        }
    }
}
