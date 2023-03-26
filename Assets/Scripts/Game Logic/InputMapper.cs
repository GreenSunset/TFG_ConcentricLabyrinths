using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputMapper
{
    static Gamepad gamepad = Gamepad.current;
    public static float AxisX()
    {
        float axis = Input.GetAxis("Keyboard X") + gamepad.rightStick.x.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static float AxisY()
    {
        float axis = Input.GetAxis("Keyboard Y") + gamepad.rightStick.y.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static float AxisZ()
    {
        float axis = Input.GetAxis("Keyboard Z") + gamepad.rightTrigger.ReadValue() - gamepad.leftTrigger.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static float AxisW()
    {
        float axis = Input.GetAxis("Keyboard W") + gamepad.leftStick.x.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }
    public static float AxisV()
    {
        float axis = Input.GetAxis("Keyboard V") + gamepad.leftStick.y.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static Vector3 MainAxis()
    {
        return new Vector3(AxisX(), AxisY(), 0);
    }
}
