using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDebug : MonoBehaviour {

	void Update ()
    {
        /* Axes */
        if(Input.GetAxis("Horizontal") != 0)
        {
            Debug.Log("X axis: " + Input.GetAxis("Horizontal"));
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            Debug.Log("Y axis: " + Input.GetAxis("Vertical"));
        }
        if (Input.GetAxis("Right_Stick_Horizontal") != 0)
        {
            Debug.Log("3rd axis: " + Input.GetAxis("Right_Stick_Horizontal"));
        }
        if (Input.GetAxis("Right_Stick_Vertical") != 0)
        {
            Debug.Log("4th axis: " + Input.GetAxis("Right_Stick_Vertical"));
        }
        if (Input.GetAxis("L2_Axis") != -1 && Input.GetAxis("L2_Axis") != 0)
        {
            Debug.Log("5th axis: " + Input.GetAxis("L2_Axis"));
        }
        if (Input.GetAxis("R2_Axis") != -1 && Input.GetAxis("R2_Axis") != 0)
        {
            Debug.Log("6th axis: " + Input.GetAxis("R2_Axis"));
        }
        if (Input.GetAxis("D_Pad_Horizontal") != 0)
        {
            Debug.Log("7th axis: " + Input.GetAxis("D_Pad_Horizontal"));
        }
        if (Input.GetAxis("D_Pad_Vertical") != 0)
        {
            Debug.Log("8th axis: " + Input.GetAxis("D_Pad_Vertical"));
        }

        /* Joystick Buttons */
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            Debug.Log("joystick button 0");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            Debug.Log("joystick button 1");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            Debug.Log("joystick button 2");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            Debug.Log("joystick button 3");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            Debug.Log("joystick button 4");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            Debug.Log("joystick button 5");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton6))
        {
            Debug.Log("joystick button 6");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            Debug.Log("joystick button 7");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton8))
        {
            Debug.Log("joystick button 8");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton9))
        {
            Debug.Log("joystick button 9");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton10))
        {
            Debug.Log("joystick button 10");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton11))
        {
            Debug.Log("joystick button 11");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton12))
        {
            Debug.Log("joystick button 12");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton13))
        {
            Debug.Log("joystick button 13");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton14))
        {
            Debug.Log("joystick button 14");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton15))
        {
            Debug.Log("joystick button 15");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton16))
        {
            Debug.Log("joystick button 16");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton17))
        {
            Debug.Log("joystick button 17");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton18))
        {
            Debug.Log("joystick button 18");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton19))
        {
            Debug.Log("joystick button 19");
        }
    }
}
