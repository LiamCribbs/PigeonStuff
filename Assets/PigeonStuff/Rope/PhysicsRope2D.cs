using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon
{
    public class PhysicsRope2D : RopeRenderer
    {
        public Transform attachedTransform;
        public Rigidbody2D attachedTransformRigidbody;
        public float attachedMass;
        public new Rigidbody2D rigidbody;

        protected override void FixedUpdate()
        {
            //gets velocity in units/frame, then gets the position for next frame
            Vector3 currentVelocity = rigidbody.velocity * Time.fixedDeltaTime;
            Vector3 extrapolatedPosition = start.position + currentVelocity;
            Vector3 hookedObjectPosition = attachedTransform.position;
            float distanceFromHook = Vector3.Distance(extrapolatedPosition, hookedObjectPosition);

            //Apply tension force
            if (distanceFromHook > rope.maxLength)
            {
                //Finds new velocity due to tension force grappling hook. Normalized vector that from node to test pos
                Vector3 positionToTest = (extrapolatedPosition - hookedObjectPosition).normalized;
                Vector3 newPosition = (positionToTest * rope.maxLength) + hookedObjectPosition;
                Vector3 newVelocity = newPosition - start.position;

                //Calculate tension force
                Vector3 deltaVelocity = newVelocity - currentVelocity;

                Vector3 tensionForce = deltaVelocity / Time.fixedDeltaTime;
                //float tensionForceMagnitude = tensionForce.magnitude;

                rigidbody.AddForceAtPosition(attachedMass * tensionForce, start.position);
                if (attachedTransformRigidbody != null)
                {
                    attachedTransformRigidbody.AddForceAtPosition(-attachedMass * tensionForce, end.position);
                }
                //rigidbody.AddForce(rigidbody.mass * tensionForce);
            }

            base.FixedUpdate();
        }
    }
}