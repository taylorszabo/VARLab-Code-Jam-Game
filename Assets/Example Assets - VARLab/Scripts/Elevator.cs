/*This source code was originally provided by the Digital Simulations Lab (VARLab) at Conestoga College in Ontario, Canada.
 * It was provided as a foundation of learning for participants of our 2022 Introduction to Unity Boot Camp.
 * Participants are welcome to use, extend and share projects derived from this code under the Creative Commons Attribution-NonCommercial 4.0 International license as linked below:
        Summary: https://creativecommons.org/licenses/by-nc/4.0/
        Full: https://creativecommons.org/licenses/by-nc/4.0/legalcode
 * You may not sell works derived from this code, but we hope you learn from it and share that learning with others.
 * We hope it inspires you to make more games or consider a career in game development.
 * To learn more about the opportunities for computer science and software engineering at Conestoga College please visit https://www.conestogac.on.ca/applied-computer-science-and-information-technology */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TigerTail
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [RequireComponent(typeof(Collider))]
    public class Elevator : MonoBehaviour
    {

        public enum Activation
        {
            /// <summary>The elevator will automatically begin moving.</summary>
            Automatic,
            /// <summary>The elevator will activate when the player steps on it.</summary>
            Player,
            /// <summary>The elevator will activate when any object collides with it.</summary>
            Everything,
            /// <summary>Elevator will not move without calling Move.</summary>
            Manual
        }

        public enum Deactivation
        {
            /// <summary>When there are no colliders on the elevator, it will stop moving.</summary>
            Stop,
            /// <summary>When there are no colliders on the elevator, it will continue moving to its next destination before stopping.</summary>
            Continue,
            /// <summary>When there are no colliders on the elevator, it will return to its last destination before stopping.</summary>
            Return
        }

        [Tooltip("The platform of the elevator, attached as a child object")]
        [SerializeField] private Transform platform;

        [Tooltip("Array of destinations for the elevator to loop between.")]
        [SerializeField] private Transform[] destinations;

        [Tooltip("Speed of the elevator in m/s.")]
        [Range(0f, 10f)]
        [SerializeField] protected float moveSpeed = 5f;

        [Tooltip("Time in seconds for the elevator to reach full speed")]
        [Range(0f, 4f)]
        [SerializeField] protected float startupTime = 2f;

        [Tooltip("Method used to activate the elevator.")]
        [SerializeField] protected Activation activateMode = Activation.Player;

        [Tooltip("What the elevator does when all of its activators fall off.")]
        [SerializeField] protected Deactivation deactivateMode = Deactivation.Return;

        [Tooltip("Event to fire when the Elevator reaches its destination.")]
        public UnityEvent<Transform> OnArrival;


        /// <summary>If the elevator is currently moving.</summary>
        protected bool moving;

        /// <summary>Current destination to move towards.</summary>
        private Transform destination;

        /// <summary>Current destination index to move towards.</summary>
        private int destinationIndex = 0;

        /// <summary>GameObjects of anything currently riding the elevator.</summary>
        private List<GameObject> activators;

        /// <summary>If the elevator is currently returning to its last destination.</summary>
        private bool reversing = false;

        /// <summary>Used to track the time spent getting the elevator up to full speed</summary>
        private float startupElapsed = 0f;

        protected void OnValidate()
        {
            if (!platform) { platform = transform.GetChild(0); }

            // Always ensure the platform is centered 
            if (platform) { platform.localPosition = Vector3.zero; }

            // Automatically moves the platform to the first destination position in the editor
            if (destinations != null && destinations.Length > 0)
            {
                destinationIndex = 0;
                destination = destinations[destinationIndex];

                if (destination)
                {
                    transform.position = destination.position;
                }
            }
        }

        private void Awake()
        {
            // Ensures the platform position is synced on startup
            OnValidate();

            activators = new List<GameObject>();

            moving = activateMode == Activation.Automatic; // Activate the elevator if it's in automatic mode.

            destinationIndex = 0;

            destination = destinations[destinationIndex];
        }

        protected virtual void Update()
        {
#if UNITY_EDITOR
            // This ensures that the platform will always be centered in the parent and it will track
            // to the first destination position
            if (!Application.isPlaying)
            {
                OnValidate();
            }
#endif
        }

        private void FixedUpdate()
        {
            if (moving)
            {
                startupElapsed += Time.fixedDeltaTime;

                // If startup time for the elevator movement is 0, do not attempt to do a smooth step.
                // Otherwise, the movement of the elevator starts up smoothly
                float delta = (startupTime == 0f) ? moveSpeed : Mathf.SmoothStep(0, moveSpeed, startupElapsed / startupTime);
                var newPosition = Vector3.MoveTowards(transform.position, destination.position, delta * Time.fixedDeltaTime);

                Vector3 playerVelocity = (newPosition - transform.position) / Time.fixedDeltaTime;
                Debug.DrawRay(transform.position, playerVelocity, Color.red, Time.fixedDeltaTime);

                //HandlePlayerVelocitySync(playerVelocity);
                transform.position = newPosition;

                var sqrDist = (transform.position - destination.position).sqrMagnitude;

                const float ERROR_MARGIN = 0.1f; // How close we need to be to the destination to count as having arrived.
                                                 // This is left squared since using square root to get distance is expensive and unnecessary.
                if (sqrDist < ERROR_MARGIN)
                {
                    HandleDestinationReached();
                }
            }
        }


        protected void HandleDestinationReached()
        {
            Debug.Log($"Platform arrived at destination '{destination.name}'");
            OnArrival?.Invoke(destination);

            startupElapsed = 0f;

            switch (activateMode)
            {
                case Activation.Manual:
                    moving = false; 
                    return;

                case Activation.Everything:
                case Activation.Player:
                    // Deactivate elevator if nothing is on it
                    moving = activators.Count > 0; 
                    break;

            }

            GetDestination();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check the object that collided and then decide if we care about the collision
            var validCollision = false;
            var isPlayer = Helpers.IsPlayer(other.gameObject);

            switch (activateMode)
            {
                case Activation.Everything:
                    validCollision = true;
                    break;

                case Activation.Player:
                    validCollision = isPlayer;
                    break;
            }
            
            // The player becomes a child object of the platform so they do not slide off
            if (isPlayer)
            {
               // playerMovement = other.gameObject.GetComponent<FirstPersonMovement>(); // ew coupling
                other.transform.SetParent(transform, true);
            }

            if (validCollision)
            {
                moving = true;
                startupElapsed = 0f;
                activators?.Add(other.gameObject);

                // If the elevator was returning to its last destination, we want to go forward again.
                if (reversing) { GetDestination(); }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var validCollision = false;
            var isPlayer = Helpers.IsPlayer(other.gameObject);

            switch (activateMode)
            {
                case Activation.Everything:
                    validCollision = true;
                    break;

                case Activation.Player:
                    validCollision = isPlayer;
                    break;
            }

            // Detach the player so their movement is no longer relative
            // to the platform movement
            if (isPlayer)
            {
                other.transform.SetParent(null, true);
            }

            if (validCollision)
            {
                startupElapsed = 0f;
                activators?.Remove(other.gameObject);
                DeactivateByMode();

            }
        }


        /// <summary>Handles the deactivation logic that should run when no activators are present on the elevator.</summary>
        private void DeactivateByMode()
        {
            startupElapsed = 0f;

            switch (deactivateMode)
            {
                case Deactivation.Return:
                    if (activators.Count <= 0) { GetDestination(true); }
                    break;

                case Deactivation.Stop:
                    moving = activators.Count > 0;
                    break;
            }
        }


        /// <summary>Forces the elevator to move to a specific transform.</summary>
        /// <remarks>This transform does not need to be part of the destinations array.</remarks>
        public void Move(Transform destination)
        {
            this.destination = destination;
            moving = true;
            startupElapsed = 0f;
        }

        /// <summary>Gets the next index to use with the destinations array, restarting from 0 if it reaches the end.</summary>
        private void GetLoopingDestination(bool reverse)
        {
            reversing = reverse;
            destinationIndex = Helpers.Modulo(destinationIndex + (reverse ? -1 : 1), destinations.Length);
            destination = destinations[destinationIndex];
        }

        private void GetDestination(bool reverse = false)
        {
            GetLoopingDestination(reverse);
        }
    }
}