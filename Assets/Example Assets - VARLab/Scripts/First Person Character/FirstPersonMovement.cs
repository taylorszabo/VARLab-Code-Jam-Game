/*This source code was originally provided by the Digital Simulations Lab (VARLab) at Conestoga College in Ontario, Canada.
 * It was provided as a foundation of learning for participants of our 2022 Introduction to Unity Boot Camp.
 * Participants are welcome to use, extend and share projects derived from this code under the Creative Commons Attribution-NonCommercial 4.0 International license as linked below:
        Summary: https://creativecommons.org/licenses/by-nc/4.0/
        Full: https://creativecommons.org/licenses/by-nc/4.0/legalcode
 * You may not sell works derived from this code, but we hope you learn from it and share that learning with others.
 * We hope it inspires you to make more games or consider a career in game development.
 * To learn more about the opportunities for computer science and software engineering at Conestoga College please visit https://www.conestogac.on.ca/applied-computer-science-and-information-technology */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TigerTail.FirstPersonCharacter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [DisallowMultipleComponent]
    public class FirstPersonMovement : MonoBehaviour
    {

        [Flags]
        public enum State
        {
            /// <summary>This player is moving via their own inputs.</summary>
            Moving = 1,
            /// <summary>The player is currently jumping.</summary>
            Jumping = 1 << 1,
            /// <summary>The player is currently falling.</summary>
            Falling = 1 << 2,
            /// <summary>The player has been immobilized and cannot move themselves.</summary>
            Immobilized = 1 << 3,
            /// <summary>The player has been knocked back by an external force.</summary>
            Knockback = 1 << 4,
            /// <summary>The player has been set to slide rather than have instant movement response during normal movement.</summary>
            Sliding = 1 << 5
        }


        /// <summary>Rigidbody attached to this player object.</summary>
        protected new Rigidbody rigidbody;

        /// <summary>Capsule Collider attached to this player object.</summary>
        protected new CapsuleCollider collider;

        /// <summary>
        ///     Holds the state flags for the controller's movement
        /// </summary>
        protected State state;

        /// <summary>Time the player last jumped at.</summary>
        protected float lastJumpTime;



        [Header("Inputs")]

        [SerializeField] protected string HorizontalInput = "Horizontal";
        [SerializeField] protected string VerticalInput = "Vertical";
        [SerializeField] protected string JumpInput = "Jump";
        [SerializeField] protected string SprintInput = "Sprint";


        [Header("Movement Values")]

        [Tooltip("Player movement speed.")]
        [Range(0.1f, 20f)]
        [SerializeField] private float moveSpeed = 10f;

        [Tooltip("Multiplier for movement speed when sprinting. Set to 1 to disable sprinting.")]
        [Range(1f, 3f)]
        [SerializeField] private float sprintModifier = 1.5f;

        [Tooltip("Force of the player's jump. Determines speed and height of jump.")]
        [Range(4f, 20f)]
        [SerializeField] private float jumpForce = 10f;

        [Tooltip("Determines how long to wait before applying gravity")]
        [Range(0.1f, 2f)]
        [SerializeField] private float jumpTime = 0.35f;

        [Tooltip("Distance below player required for them to be considered falling.\n" +
            "Increase this value if you find you can't jump while moving downhill slightly.")]
        [Range(0.01f, 0.5f)]
        [SerializeField] private float fallDistanceBuffer = 0.1f;


        [Tooltip("Used to adjust the force of gravity.")]
        [Range(0.1f, 10f)]
        [SerializeField] private float gravityModifier = 1f;

        /// <summary>
        ///     Constant such that a variable gravityModifier of 1 feels natural
        /// </summary>
        private const float STATIC_GRAVITY_MODIFIER = 2f;

        public Vector3 ExternalVelocity { get; set; }

        /// <summary>Whether the player is sliding across a surface or not.</summary>
        /// <remarks>Set this to help the player slide down a slope or across an icy surface.</remarks>
        public bool IsSliding
        {
            get { return state.HasFlag(State.Sliding); }
            set { ToggleState(State.Sliding, value); }
        }

        private void Awake()
        {
            collider = GetComponent<CapsuleCollider>();
            rigidbody = GetComponent<Rigidbody>();
        }


        /// <summary>
        ///     Physics code and all Unity physics runs on the FixedUpdate loop. 
        ///     The update rate of FixedUpdate is set in the Unity Player Settings under Time 
        ///     and is set to 200Hz for this project.
        /// </summary>
        private void FixedUpdate()
        {
            GroundCheck();

            var moveVelocity = CalculateMovementVector();
            var jumpVelocity = CalculateJumpVector();

            HandleMovementByState(moveVelocity , jumpVelocity);
        }

        /// <summary>Checks if the player is currently touching the ground and sets their state accordingly.</summary>
        private void GroundCheck()
        {
            // During the jumpTime, downward forces won't be applied
            if (state.HasFlag(State.Jumping) && (Time.time - jumpTime) < lastJumpTime) { return; }

            // Our sphere collider extends above and below this object's actual position in space by half of its height.
            // This means the floor is half the height of the capsule collider below us.
            // To check if we're touching the ground we need to see if a ray fired downwards that's half of our height touches the floor.
            // A small buffer is added because this ray can be slightly off the floor if we're on a slope even though our collider is actually making contact.
            var fallingRayDistance = collider.height / 2 + fallDistanceBuffer;

            // Draw DEBUG ray
            Debug.DrawRay(transform.position, Vector3.down *  fallingRayDistance, Color.red, Time.deltaTime);

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, fallingRayDistance))
            {
                ToggleState(State.Jumping | State.Falling, false);
                var adjustedPosition = transform.position;
                adjustedPosition.y = hit.point.y + collider.height / 2;
                transform.position = adjustedPosition;
            }
            else
            {
                ToggleState(State.Falling, true);
            }
        }


        /// <summary> 
        ///     Calculates the velocity vector for regular movement
        /// </summary>
        protected virtual Vector3 CalculateMovementVector()
        {
            var moveVelocity = Vector3.zero;

            // Horizontal axis handles left-right movement
            moveVelocity += transform.right * Input.GetAxisRaw(HorizontalInput);
            // Vertical axis handles forward-back movement
            moveVelocity += transform.forward * Input.GetAxisRaw(VerticalInput);

            if (HasAnyState(State.Immobilized))
                moveVelocity = Vector3.zero;

            // We're moving if we have a non-zero velocity.
            ToggleState(State.Moving, moveVelocity != Vector3.zero);

            // Going forward/back and left/right at the same time creates a right triangle with magnitude sqrt(2).
            // Normalizing this makes you move at the same speed regardless of input combination.
            moveVelocity = moveVelocity.normalized;

            // Modify velocity by the movement speed factor
            moveVelocity *= moveSpeed;

            return moveVelocity;
        }

        /// <summary>Returns the velocity vector for a jump.</summary>
        protected virtual Vector3 CalculateJumpVector()
        {
            if (HasAnyState(State.Jumping | State.Falling | State.Knockback | State.Immobilized))
                return Vector3.zero;

            if (Input.GetButtonDown(JumpInput))
            {
                ToggleState(State.Jumping, true);
                lastJumpTime = Time.time;
                return Vector3.up * jumpForce;
            }

            return Vector3.zero;
        }

        /// <summary>
        ///     Handles movement on a per-state basis.
        /// </summary>
        /// <remarks>
        ///     Jumping is applied as an additional force on the character,
        ///     while movement is applied instantaneously
        /// </remarks>
        protected virtual void HandleMovementByState(Vector3 moveVelocity, Vector3 jumpVelocity)
        {
            // Apply jump force which will be non-zero when we start the jump
            rigidbody.AddForce(jumpVelocity, ForceMode.VelocityChange);

            // Apply gravity when falling. This is in addition to the standard rigidbody gravity
            //if (HasAnyState(State.Falling))
            //{
            rigidbody.useGravity = false;
            rigidbody.AddForce(gravityModifier * STATIC_GRAVITY_MODIFIER * Physics.gravity, ForceMode.Acceleration);
            //}

            // Apply sprint modifier when the key is held, if the player is not falling
            if (!HasAnyState(State.Falling))
            {
                moveVelocity *= (Input.GetButton(SprintInput) ? sprintModifier : 1f);
            }

            // Preserves the current velocity (affected by drag) when sliding
            if (HasAnyState(State.Sliding))
            {
                if (Mathf.Abs(rigidbody.velocity.x) + 0.09f > Mathf.Abs(moveVelocity.x))
                {
                    moveVelocity.x = rigidbody.velocity.x;
                }

                if (Mathf.Abs(rigidbody.velocity.z) + 0.09f > Mathf.Abs(moveVelocity.z))
                {
                    moveVelocity.z = rigidbody.velocity.z;
                }
            }



            // Y-axis velocity is always preserved
            moveVelocity.y = rigidbody.velocity.y;

            rigidbody.velocity = moveVelocity;
        }



        /// <summary>
        ///     Sets a state flag based on whether or not it should be <paramref name="active"/>.
        /// </summary>
        /// <param name="state">Which state to modify.</param>
        /// <param name="active">Whether or not the state should be active.</param>
        protected void ToggleState(State state, bool active)
        {
            if (active)
            {
                this.state |= state;
                return;
            }

            this.state &= ~state;
        }

        /// <summary>Checks to see if any of the states passed in <paramref name="state"/> are active.</summary>
        protected bool HasAnyState(State state)
        {
            return (this.state & state) != 0;
        }

        /// <summary>Adds knockback to the player.</summary>
        public void AddKnockback(Vector3 knockbackVelocity)
        {
            state |= State.Knockback;
            rigidbody.AddForce(knockbackVelocity);
        }

        public void SetSliding(bool value)
        {
            Debug.Log("Changing sliding state");
            IsSliding = value;
        }
    }
}
