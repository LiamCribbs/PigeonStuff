using UnityEngine;

namespace Pigeon.Sequencer
{
    public enum TransformSpace
    {
        World, Local, Anchored
    }

    public static class TransformSpaceExtensions
    {
        public static Vector3 GetPosition(this TransformSpace space, Transform transform)
        {
            return space == TransformSpace.World ? transform.position : space == TransformSpace.Local ? transform.localPosition : (Vector3)((RectTransform)transform).anchoredPosition;
        }

        public static void SetPosition(this TransformSpace space, Transform transform, Vector3 position)
        {
            switch (space)
            {
                case TransformSpace.World:
                    transform.position = position;
                    break;
                case TransformSpace.Local:
                    transform.localPosition = position;
                    break;
                default:
                    ((RectTransform)transform).anchoredPosition = position;
                    break;
            }
        }

        public static Quaternion GetRotation(this TransformSpace space, Transform transform)
        {
            return space == TransformSpace.World ? transform.rotation : transform.localRotation;
        }

        public static void SetRotation(this TransformSpace space, Transform transform, Quaternion rotation)
        {
            switch (space)
            {
                case TransformSpace.World:
                    transform.rotation = rotation;
                    break;
                default:
                    transform.localRotation = rotation;
                    break;
            }
        }
    }
}