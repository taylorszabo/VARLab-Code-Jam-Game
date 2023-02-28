/*This source code was originally provided by the Digital Simulations Lab (VARLab) at Conestoga College in Ontario, Canada.
 * It was provided as a foundation of learning for participants of our 2022 Introduction to Unity Boot Camp.
 * Participants are welcome to use, extend and share projects derived from this code under the Creative Commons Attribution-NonCommercial 4.0 International license as linked below:
        Summary: https://creativecommons.org/licenses/by-nc/4.0/
        Full: https://creativecommons.org/licenses/by-nc/4.0/legalcode
 * You may not sell works derived from this code, but we hope you learn from it and share that learning with others.
 * We hope it inspires you to make more games or consider a career in game development.
 * To learn more about the opportunities for computer science and software engineering at Conestoga College please visit https://www.conestogac.on.ca/applied-computer-science-and-information-technology */

using UnityEngine;

namespace TigerTail.FirstPersonCharacter
{
    [DisallowMultipleComponent]
    public class FirstPersonPickupHandler : MonoBehaviour, IPickerUpper
    {
        [Tooltip("The transform pickups should be parented to for holding/throwing.")]
        [SerializeField] private Transform pickupLocation;

        /// <summary>Currently held pickup.</summary>
        private IPickup pickup;

        [Tooltip("Force to apply to thrown objects. (Mass-dependant)")]
        [Range(500,5000)]
        [SerializeField] private float throwForce = 2000f;

        private void Update()
        {
            HandleThrowing();
        }

        private void HandleThrowing()
        {
            if (Input.GetMouseButtonUp(0) && pickup != null)
            {
                if (pickup is IThrowable)
                {
                    (pickup as IThrowable).Throw(gameObject, pickupLocation.forward * throwForce);
                    pickup = null;
                }
            }
        }

        public bool PickupObject(IPickup pickup)
        {
            if (this.pickup != null) // Don't pick up an object if we already have one picked up.
                return false;

            pickup.SetParentPoint(pickupLocation);        

            this.pickup = pickup;
            return true;
        }
    }
}
