using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private CharacterController controller;
    private Vector3 velocity;
    private float gravity = -9.81f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (ThirdPersonCamera.isUIOpen) return;
        if (controller.enabled)
        {
            Move();
        }
       
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.right * h + transform.forward * v;

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }    
        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMovement = moveDirection * moveSpeed + velocity;
        controller.Move(finalMovement * Time.deltaTime);
    }
}