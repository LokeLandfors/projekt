using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;

    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private Stamina stamina;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        stamina = GetComponent<Stamina>();
    }

    void Update()
    {
        // Movement input
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
        }

        input = input.normalized;

        // Move relative to player direction
        Vector3 move = transform.right * input.x +
                       transform.forward * input.y;

        // Sprints
        bool wantsToSprint = Keyboard.current != null &&
                             Keyboard.current.leftShiftKey.isPressed &&
                             input.magnitude > 0.1f;

        bool sprinting = wantsToSprint &&
                         (stamina == null || stamina.CurrentStamina > 0f);

        if (stamina != null)
        {
            stamina.SetSprinting(sprinting);
        }

        float speed = sprinting ? sprintSpeed : moveSpeed;
        controller.Move(move * speed * Time.deltaTime);
        // Ground check
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
