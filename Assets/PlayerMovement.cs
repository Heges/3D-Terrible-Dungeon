using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;

    public CharacterController controller;
    public float gravity = -19.1f;
    Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.right * x + transform.forward * y;

        velocity.y += gravity * Time.deltaTime;

        controller.Move(moveDirection * speed * Time.deltaTime);
        controller.Move(velocity * Time.deltaTime);
        //groundcheck
    }
}
