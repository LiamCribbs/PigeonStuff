using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeRenderer2D : MonoBehaviour
    {
        public Transform start;
        public Transform end;

        public VertletRope2D rope;

        protected virtual void Awake()
        {
            rope.Setup(rope.maxLength);
        }

        protected virtual void FixedUpdate()
        {
            rope.startPosition = start.position;
            rope.endPosition = end.position;
            rope.Simulate();
        }

        protected virtual void LateUpdate()
        {
            rope.DrawRope();
        }
    }
}