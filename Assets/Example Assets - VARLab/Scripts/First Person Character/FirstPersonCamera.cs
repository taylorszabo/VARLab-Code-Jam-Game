/*This source code was originally provided by the Digital Simulations Lab (VARLab) at Conestoga College in Ontario, Canada.
 * It was provided as a foundation of learning for participants of our 2022 Introduction to Unity Boot Camp.
 * Participants are welcome to use, extend and share projects derived from this code under the Creative Commons Attribution-NonCommercial 4.0 International license as linked below:
        Summary: https://creativecommons.org/licenses/by-nc/4.0/
        Full: https://creativecommons.org/licenses/by-nc/4.0/legalcode
 * You may not sell works derived from this code, but we hope you learn from it and share that learning with others.
 * We hope it inspires you to make more games or consider a career in game development.
 * To learn more about the opportunities for computer science and software engineering at Conestoga College please visit https://www.conestogac.on.ca/applied-computer-science-and-information-technology */

using UnityEngine;
using UnityEngine.EventSystems;

namespace TigerTail.FirstPersonCharacter
{
    [DisallowMultipleComponent]
    public class FirstPersonCamera : MonoBehaviour
    {
        [Tooltip("Locks the mouse cursor to the game window")]
        [SerializeField] private bool lockCursor = true;

        [Tooltip("Mouse sensitivity for camera rotation.")]
        [Range(50, 500)]
        [SerializeField] private float mouseSensitivity = 100f;

        [Tooltip("Transform of the player's body so that it can be rotated with the camera.")]
        [SerializeField] private Transform playerBody;

        /// <summary>Rotation around the x-axis for looking up and down.</summary>
        /// <remarks>Looking left and right is handled by rotating the actual player controller.
        /// This ensures that moving the player object in its forward direction will make it move in the direction we're looking.</remarks>
        private float verticalRotation = 0f;

        private void Awake()
        {
            if (lockCursor)
            {
                // Hides the mouse cursor and prevents it from exiting the screen bounds.
                Cursor.lockState = CursorLockMode.Locked; 
            }
        }

        protected virtual void Update()
        {
            CheckCursorState();

            HandleMouseMovement();
        }

        /// <summary>
        ///     Handles locking and freeing the mouse cursor to the game window
        /// </summary>
        protected virtual void CheckCursorState()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (Input.GetButtonDown("Cancel"))
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }


        /// <summary>
        ///     Updates the vertical rotation of the current transform, and the horizontal rotation
        ///     of the <see cref="playerBody"/> transform, so that the local forward direction is always
        ///     the direction the camera is facing
        /// </summary>
        protected virtual void HandleMouseMovement()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
