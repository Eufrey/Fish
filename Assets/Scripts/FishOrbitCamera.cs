using UnityEngine;
using UnityEngine.InputSystem;

namespace Fish
{
    [RequireComponent(typeof(Camera))]
    public sealed class FishOrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float targetHeight = 0.6f;
        [SerializeField] private float distance = 6f;
        [SerializeField] private float mouseSensitivity = 0.12f;
        [SerializeField] private float minimumPitch = 10f;
        [SerializeField] private float maximumPitch = 65f;
        [SerializeField] private float pitch = 25f;
        private float yaw;
        private int skipInputFrame = -1;
        public bool ExplorationInputEnabled { get; private set; } = true;

        public void SetExplorationInputEnabled(bool value)
        {
            ExplorationInputEnabled = value;
            // Closing on Escape or a UI click must not also affect exploration this frame.
            skipInputFrame = Time.frameCount;
        }

        public void Configure(Transform player) => target = player;

        private void OnEnable()
        {
            if (target == null)
            {
                Debug.LogError("Orbit camera needs a player target. Check Milestone 1A setup.", this);
                enabled = false;
                return;
            }
            yaw = transform.eulerAngles.y;
            if (ExplorationInputEnabled)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else ReleaseCursor();
        }

        private void OnDisable() => ReleaseCursor();

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) ReleaseCursor();
        }

        private static void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            bool acceptInput = ExplorationInputEnabled && Time.frameCount != skipInputFrame;
            if (acceptInput && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ReleaseCursor();
            else if (acceptInput && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (acceptInput && Cursor.lockState == CursorLockMode.Locked && Mouse.current != null)
            {
                // Mouse delta is already a per-frame displacement, not a speed.
                Vector2 delta = Mouse.current.delta.ReadValue();
                yaw += delta.x * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, minimumPitch, maximumPitch);
            }

            SnapToTarget();
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = target.position + Vector3.up * targetHeight;
            transform.SetPositionAndRotation(focus - rotation * Vector3.forward * distance, rotation);
        }
    }
}
