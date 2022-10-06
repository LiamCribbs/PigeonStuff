using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeRenderer2DNoTransform : MonoBehaviour
    {
        public Vector2 start;
        public Vector2 end;

        public VertletRope2D rope;

        protected virtual void Awake()
        {
            rope.Setup(rope.maxLength);
        }

        protected virtual void FixedUpdate()
        {
            rope.startPosition = start;
            rope.endPosition = end;
            rope.Simulate();
        }

        protected virtual void LateUpdate()
        {
            rope.DrawRope();
        }

        void OnEnable()
        {
            rope.lineRenderer.enabled = true;
        }

        void OnDisable()
        {
            rope.lineRenderer.enabled = false;
        }
    }
}