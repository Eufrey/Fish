using UnityEngine;
using UnityEngine.InputSystem;

namespace Fish
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FishExplorationPlayer : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Transform viewCamera;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float turnSpeed = 540f;
        [SerializeField] private float gravity = -20f;

        private CharacterController controller;
        private InputAction move;
        private float verticalSpeed;
        private int skipInputFrame = -1;
        public bool ExplorationInputEnabled { get; private set; } = true;

        public void SetExplorationInputEnabled(bool value)
        {
            ExplorationInputEnabled = value;
            skipInputFrame = Time.frameCount;
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            CharacterController body = controller != null ? controller : GetComponent<CharacterController>();
            bool wasEnabled = body.enabled;
            body.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            verticalSpeed = 0f;
            body.enabled = wasEnabled;
        }

        public void Configure(InputActionAsset actions, Transform cameraTransform)
        {
            inputActions = actions;
            viewCamera = cameraTransform;
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            InputAction moveSource = inputActions != null ? inputActions.FindAction("Player/Move") : null;
            if (moveSource == null || viewCamera == null)
            {
                Debug.LogError("Exploration needs the Player/Move action and a camera reference.", this);
                enabled = false;
                return;
            }

            // Own only this action; do not enable or disable the shared project asset.
            move = moveSource.Clone();
            move.Enable();
        }

        private void OnDisable()
        {
            move?.Dispose();
            move = null;
            verticalSpeed = 0f;
        }

        private void Update()
        {
            Vector2 input = ExplorationInputEnabled && Time.frameCount != skipInputFrame && Cursor.lockState == CursorLockMode.Locked
                ? Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f)
                : Vector2.zero;

            // Flatten the camera's forward vector so looking up cannot lift the player.
            Vector3 forward = Vector3.ProjectOnPlane(viewCamera.forward, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 direction = forward * input.y + right * input.x;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion facing = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, turnSpeed * Time.deltaTime);
            }

            if (controller.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f;

            verticalSpeed = Mathf.Max(verticalSpeed + gravity * Time.deltaTime, -30f);
            controller.Move((direction * moveSpeed + Vector3.up * verticalSpeed) * Time.deltaTime);
        }
    }
}
