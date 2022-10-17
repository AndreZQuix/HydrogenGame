using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivityX;
    [SerializeField] private float sensitivityY;
    [SerializeField] GameObject cameraHolder;

    [SerializeField] private Transform orientation;

    private float xRotation;
    private float yRotation;

    private float tempSensX;
    private float tempSensY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        tempSensX = sensitivityX;
        tempSensY = sensitivityY;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraHolder.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void SetSensitivity(float sensX, float sensY)
    {
        sensitivityX = sensX;
        sensitivityY = sensY;
    }

    public void SetDefaultSensitivity()
    {
        sensitivityX = tempSensX;
        sensitivityY = tempSensY;
    }
}
