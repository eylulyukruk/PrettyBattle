
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (animator == null || controller == null)
            return;

        Vector3 velocity = controller.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;
        animator.SetFloat("Speed", speed > 0.1f ? 1f : 0f);

        
    }
}
