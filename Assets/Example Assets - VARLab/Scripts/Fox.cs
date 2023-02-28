/*This source code was originally provided by the Digital Simulations Lab (VARLab) at Conestoga College in Ontario, Canada.
 * It was provided as a foundation of learning for participants of our 2022 Introduction to Unity Boot Camp.
 * Participants are welcome to use, extend and share projects derived from this code under the Creative Commons Attribution-NonCommercial 4.0 International license as linked below:
        Summary: https://creativecommons.org/licenses/by-nc/4.0/
        Full: https://creativecommons.org/licenses/by-nc/4.0/legalcode
 * You may not sell works derived from this code, but we hope you learn from it and share that learning with others.
 * We hope it inspires you to make more games or consider a career in game development.
 * To learn more about the opportunities for computer science and software engineering at Conestoga College please visit https://www.conestogac.on.ca/applied-computer-science-and-information-technology */


using UnityEngine;
using UnityEngine.AI;

namespace TigerTail
{

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Fox : MonoBehaviour, IPickerUpper
    {
        // Animator keys and triggers
        public readonly string MovementSpeedKey = "MoveSpeed";

        public readonly string RollAction = "Somersault";
        public readonly string JumpAction = "Jump";

        protected Unity.Mathematics.Random randomizer = new();


        /// <summary>Animator component attached to the fox.</summary>
        private Animator animator;

        /// <summary>NavMeshAgent component attached to the fox.</summary>
        private NavMeshAgent agent;

        private bool isActionQueued = false;
        private float nextActionTime = 0f;
        private string queuedAction = string.Empty;


        private ThrowableSnowball[] fetchObjects = null;
        [SerializeField] private ThrowableSnowball fetchTarget = null;
        private ThrowableSnowball heldTarget = null;

        [SerializeField] private Transform mouthTransform;

        private Transform player;

        public float AnimationInterval = 10f;


        protected virtual void Awake()
        {
            randomizer = new();
            randomizer.InitState();

            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();

            player = GameObject.FindWithTag(Helpers.PLAYER_TAG).transform;
            fetchObjects = FindObjectsOfType<ThrowableSnowball>();

            Debug.Log($"Found {fetchObjects.Length} snowballs for Fox to fetch");
        }

        protected virtual void FixedUpdate()
        {
            HandleActions();
            HandleMovement();
        }

        protected virtual void HandleActions()
        {
            if (heldTarget) { return; }

            if (!isActionQueued)
            {
                isActionQueued = true;
                nextActionTime = Time.time + randomizer.NextFloat() * AnimationInterval;
                queuedAction = randomizer.NextBool() ? JumpAction : RollAction;
            }

            if (Time.time >= nextActionTime && animator)
            {
                isActionQueued = false;
                animator.SetTrigger(queuedAction);
            }
        }

        protected virtual void HandleMovement()
        {
            // Sets the MoveSpeed parameter in our Animator component to a value between 0 and 1.
            // This value is 0 when the fox is not moving and 1 when he's moving at max speed.
            animator.SetFloat(MovementSpeedKey, agent.velocity.magnitude / agent.speed);


            if (heldTarget)
            {
                agent.SetDestination(player.position);

                // already have a snowball, go to player
                var sqrDist = (transform.position - player.position).sqrMagnitude;

                // How close we need to be to the player before dropping the snowball.
                // Left as squared magnitude since using square root to get distance is expensive and unnecessary.
                const float ERROR_MARGIN = 25f;
                if (sqrDist < ERROR_MARGIN)
                {
                    DropSnowball();
                    // Stop the fox from continuing to move to player
                    agent.SetDestination(transform.position);
                }

                return;
            }

            if (fetchTarget)
            {
                agent.SetDestination(fetchTarget.transform.position);
                return;
            }

            for (int i = 0; i < fetchObjects.Length; i++)
            {
                if (fetchObjects[i].CurrentState == ThrowableSnowball.State.Thrown)
                {
                    fetchTarget = fetchObjects[i];
                    Debug.Log($"Fox is chasing after '{fetchTarget.name}'");
                    break;
                }
            }
        }


        /// <summary>
        ///     Sends the fox towards the target snowball
        /// </summary>
        /// <remarks>
        ///     If the player picks up the snowball before the Fox gets to it, Fox
        ///     will still try to get the snowball from the player and will push
        ///     the player around. This is a bug but it's funny behaviour
        /// </remarks>
        public bool PickupObject(IPickup pickup)
        {
            if (pickup is ThrowableSnowball snowball && snowball == fetchTarget)
            {
                Debug.Log("Fox found the snowball!");
                PickupSnowball(snowball);
                return true;
            }

            Debug.Log("This isn't the object Fox was looking for.");
            return false;
        }

        protected virtual void PickupSnowball(ThrowableSnowball snowball)
        {
            fetchTarget = null;
            heldTarget = snowball;
            heldTarget.SetParentPoint(mouthTransform);
        }

        protected virtual void DropSnowball()
        {
            if (!heldTarget) { return; }

            heldTarget.Drop(transform.forward * 50f);
            heldTarget = null;
        }
    }
}