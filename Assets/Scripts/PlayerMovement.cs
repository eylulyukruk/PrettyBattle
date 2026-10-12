
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Debug.Log("PrettyBattle PlayerMovement started!");
    }

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed)
            input.y += 1;

        if (Keyboard.current.sKey.isPressed)
            input.y -= 1;

        if (Keyboard.current.aKey.isPressed)
            input.x -= 1;

        if (Keyboard.current.dKey.isPressed)
            input.x += 1;

        Vector3 moveDirection = new Vector3(input.x, 0, input.y);

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}
