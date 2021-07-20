using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float mouseSensivity = 100f;
    float xRotation = 0f;

    public Transform playerBody;

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //float mousseX = Input.GetAxis("Mouse X") * mouseSensivity * Time.deltaTime;
        //float mousseY = Input.GetAxis("Mouse Y") * mouseSensivity * Time.deltaTime;
        float mousseX = Input.GetAxis("Horizontal") * mouseSensivity * Time.deltaTime;
        float mousseY = Input.GetAxis("Vertical") * mouseSensivity * Time.deltaTime;

        xRotation -= mousseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f,0f);
        playerBody.Rotate(Vector3.up * mousseX);
    }
}
