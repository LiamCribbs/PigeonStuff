using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Movement.Testing
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovingRigidbodyPlatform : MonoBehaviour
    {
        [SerializeField] bool move;
        [SerializeField] Vector3 startPoint;
        [SerializeField] Vector3 endPoint;
        bool movingToStart;

        [Space(20)]
        [SerializeField] float speed;
        [SerializeField] Vector3 angularSpeed;

        new Rigidbody rigidbody;

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();

            startPoint += transform.localPosition;
            endPoint += transform.localPosition;
        }

        void FixedUpdate()
        {
            Vector3 position = transform.localPosition;
            Vector3 target = movingToStart ? startPoint : endPoint;

            Vector3 velocity = (target - position).normalized * speed;

            if (Vector3.Dot(rigidbody.velocity, velocity) < 0f)
            {
                movingToStart = !movingToStart;
                return;
            }

            rigidbody.velocity = velocity;

            rigidbody.angularVelocity = angularSpeed;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.8f, 0.85f, 0.1f);
            if (transform.parent != null)
            {
                Gizmos.matrix = transform.parent.localToWorldMatrix;
            }

            Gizmos.DrawSphere(transform.localPosition + startPoint, 1f);
            Gizmos.DrawSphere(transform.localPosition + endPoint, 1f);
        }
    }
}