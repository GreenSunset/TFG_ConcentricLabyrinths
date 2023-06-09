using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SimpleMazeNavigator))]
public class SimpleMazePlayerController : MonoBehaviour
{
    public float speed = 5;
    public bool lockControls = false;
    private SimpleMazeNavigator navigator;
    // Start is called before the first frame update
    void Start()
    {
        navigator = GetComponent<SimpleMazeNavigator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (navigator == null || lockControls) return;
        int? movement = null;
        Vector3 input = InputMapper.MainAxis2D();
        float vertical = InputMapper.DPadV();
        if (vertical > 0) {
            navigator.Ascend();
        } else if (vertical < 0) {
            navigator.Descend();
        } else if (input.magnitude > .2) {
            input = SimpleObserverController.main.transform.TransformVector(input);
            Debug.DrawLine(transform.position, transform.position + input, Color.red);
            float potency = 0;
            movement = navigator.NeighbourInDirection(input, ref potency);
            potency = Mathf.Cos(Mathf.Deg2Rad * potency);
            if (movement != null) {
                navigator.MoveTowards((int) movement, Time.deltaTime * input.magnitude * potency * speed);
            }
        }
    }
}
