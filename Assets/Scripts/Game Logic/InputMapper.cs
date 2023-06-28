using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputMapper
{
    static Gamepad gamepad = Gamepad.current;

    public static float DPadH()
    {
        float axis = gamepad.dpad.right.ReadValue() - gamepad.dpad.left.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static float DPadV()
    {
        float axis = gamepad.dpad.up.ReadValue() - gamepad.dpad.down.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static Vector2 DPad()
    {
        return new Vector2(DPadH(), DPadV());
    }

    public static float AxisX()
    {
        float axis = Input.GetAxis("Keyboard X") + gamepad.leftStick.x.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static float AxisY()
    {
        float axis = Input.GetAxis("Keyboard Y") + gamepad.leftStick.y.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static float AxisZ()
    {
        float axis = Input.GetAxis("Keyboard Z") + gamepad.rightTrigger.ReadValue() - gamepad.leftTrigger.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static float AxisU()
    {
        float axis = Input.GetAxis("Keyboard U") + gamepad.rightStick.x.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }
    public static float AxisV()
    {
        float axis = Input.GetAxis("Keyboard V") + gamepad.rightStick.y.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static float AxisW()
    {
        float axis = Input.GetAxis("Keyboard W") + gamepad.rightShoulder.ReadValue() - gamepad.leftShoulder.ReadValue();
        return Mathf.Clamp(axis, -1, 1);
    }

    public static Vector3 MainAxis()
    {
        return new Vector3(AxisX(), AxisY(), AxisZ());
    }

    public static Vector3 MainAxis2D()
    {
        return new Vector3(AxisX(), AxisY(), 0);
    }
}
