using UnityEngine;

namespace Fish
{
    [RequireComponent(typeof(Animator))]
    public sealed class FishPlayerAnimation : MonoBehaviour
    {
        private static readonly int Moving = Animator.StringToHash("Moving");
        private static readonly int Airborne = Animator.StringToHash("Airborne");

        private Animator animator;
        private CharacterController controller;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            controller = GetComponentInParent<CharacterController>();
        }

        private void LateUpdate()
        {
            if (controller == null) return;
            Vector3 velocity = controller.velocity;
            float speed = new Vector2(velocity.x, velocity.z).magnitude;
            animator.SetBool(Moving, controller.isGrounded && speed > 0.1f);
            animator.SetBool(Airborne, !controller.isGrounded);
        }
    }
}
