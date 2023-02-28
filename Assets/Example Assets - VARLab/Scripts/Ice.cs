/* This source code was originally provided by the Digital Simulations Lab (VARLab) at Conestoga College in Ontario, Canada.
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

namespace TigerTail
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class Ice : MonoBehaviour
    {
        [SerializeField]
        protected string MessageName = "SetSliding";

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (collision == null) { return; }

            collision.gameObject.SendMessage(MessageName, true, SendMessageOptions.DontRequireReceiver);

            //if (collision.gameObject.TryGetComponent(out FirstPersonMovement movement))
            //{
            //    movement.IsSliding = true;
            //}
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (collision == null) { return; }
            
            collision.gameObject.SendMessage(MessageName, false, SendMessageOptions.DontRequireReceiver);

            //if (collision.gameObject.TryGetComponent(out FirstPersonMovement movement))
            //{
            //    movement.IsSliding = false;
            //}
        }
    }
}
