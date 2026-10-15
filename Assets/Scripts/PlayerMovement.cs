
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public Transform cameraTransform;

    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    private float verticalVelocity = 0f;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Keyboard.current == null || cameraTransform == null)
            return;

        Vector2 input = ReadMovementInput();

        Vector3 moveDirection = GetMovementDirection(input);

        RotatePlayer(moveDirection);

        HandleGravityAndJump();

        MovePlayer(moveDirection);
    }

    private Vector2 ReadMovementInput()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            input.y += 1;

        if (Keyboard.current.sKey.isPressed)
            input.y -= 1;

        if (Keyboard.current.aKey.isPressed)
            input.x -= 1;

        if (Keyboard.current.dKey.isPressed)
            input.x += 1;

        return Vector2.ClampMagnitude(input, 1f);
    }

    private Vector3 GetMovementDirection(Vector2 input)
    {
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection =
            cameraForward * input.y +
            cameraRight * input.x;

        return moveDirection;
    }

    private void RotatePlayer(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void HandleGravityAndJump()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (controller.isGrounded &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void MovePlayer(Vector3 moveDirection)
    {
        Vector3 velocity = moveDirection * moveSpeed;

        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }
}
