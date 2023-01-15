using UnityEngine;

namespace Pigeon.Movement
{
    public sealed class PlayerLook : MonoBehaviour
    {
        [SerializeField] PlayerMovement playerMovement;

        [field: SerializeField] public Camera Camera { get; private set; }
        [field: SerializeField] public Camera FirstPersonCamera { get; private set; }

        [field: Space(20), SerializeField, Tooltip("How fast the camera rotates")] public float LookSensitivity { get; set; } = 0.07f;
        
        /// <summary>
        /// Current base x rotation
        /// </summary>
        public float XLookRotation { get; set; }

        /// <summary>
        /// Rotation from input this frame
        /// </summary>
        public Vector2 RotationDeltaFromMouse { get; private set; }

        [SerializeField, Tooltip("Min and max rotation on the x axis")] Vector2 xBounds = new Vector2(-90f, 90f);

        /// <summary>
        /// Min and max rotation on the x axis
        /// </summary>
        public Vector2 XLookBounds => xBounds;

        Vector3 basePosition;
        Vector3 addPosition;
        Vector3 addRotation;

        [SerializeField, Tooltip("Minimum y offset of the camera from AddPosition")] float minYPosition = -0.68f;

        [SerializeField, Tooltip("Transform to move and rotate opposite to the camera's added position and rotation." +
            "\n\nThis is meant for UI. Recommended use is with a childed canvas set to screen space camera." +
            "\n\nHave a reticle or crosshair that moves opposite to headbob helps reduce motion sickness.")] RectTransform reticle;
        [SerializeField, Tooltip("How much the reticle moves opposite to added position")] float reticlePosMultiplier = 20f;
        [SerializeField, Tooltip("How much the reticle moves opposite to added rotation")] float reticleRotMultiplier = 0.5f;

        [Space(20)]
        [SerializeField, Tooltip("Multiplier for rotational screen shake")] float rotationShakeIntensity;
        [SerializeField, Tooltip("Decay speed rotational screen shake")] float rotationShakeDecay;
        [SerializeField, Tooltip("Maximum rotation from shake")] float maxRotationShake;
        [SerializeField, Tooltip("Multiplier for translational screen shake")] float translationShakeIntensity;
        [SerializeField, Tooltip("Decay speed translational screen shake")] float translationShakeDecay;
        [SerializeField, Tooltip("Maximum translation from shake")] float maxTranslationShake;
        float rotationShake;
        float translationShake;

        [Space(20)]
        [SerializeField, Tooltip("How far the player can move the camera when it's constrained with ConstrainRotation")] float maxConstrainDistance;
        [SerializeField, Tooltip("How slow the camera rotates as it nears maxConstrainDistance when constrained with ConstrainRotation")] AnimationCurve constrainCurve;
        bool constrainRotationX;
        bool constrainRotationY;
        Vector2 constrainRotationTarget;

        [Space(20)]
        [SerializeField, Tooltip("Arms model.\n\nArms are slightly rotated opposite to the camera's x rotation to make them appear more physical.")] Transform arms;
        [SerializeField, Tooltip("Multiplier for how much the arms are rotated opposite to the camera's x rotation")] float armAngleMultiplier;
        [SerializeField, Tooltip("Minimum and Maximum arm rotation")] Vector2 armAngleBounds;

        /// <summary>
        /// Multiplier for how much the arms are rotated opposite to the camera's x rotation
        /// </summary>
        public float ArmAngleMultiplier { get => armAngleMultiplier; set => armAngleMultiplier = value; }

        Vector2Int _rotationLocks;
        /// <summary>
        /// Locks the camera rotation on the x axis
        /// </summary>
        public int RotationLocksX
        {
            get => _rotationLocks.x;
            set
            {
                _rotationLocks.x = value;
                if (_rotationLocks.x < 0)
                {
                    _rotationLocks.x = 0;
                }
            }
        }

        /// <summary>
        /// Locks the camera rotation on the y axis
        /// </summary>
        public int RotationLocksY
        {
            get => _rotationLocks.y;
            set
            {
                _rotationLocks.y = value;
                if (_rotationLocks.y < 0)
                {
                    _rotationLocks.y = 0;
                }
            }
        }

        /// <summary>
        /// Add a position to the camera this frame
        /// </summary>
        public void AddPosition(Vector3 pos)
        {
            addPosition += pos;
        }

        /// <summary>
        /// Add a rotation to the camera this frame
        /// </summary>
        public void AddRotation(Vector3 pos)
        {
            addRotation += pos;
        }

        /// <summary>
        /// Constrain the camera's rotation toward a target
        /// </summary>
        public void ConstrainRotation(Vector2 target)
        {
            constrainRotationX = true;
            constrainRotationY = true;
            constrainRotationTarget = target;
        }

        /// <summary>
        /// Constrain the camera's x rotation toward a target
        /// </summary>
        public void ConstrainRotationX(float target)
        {
            constrainRotationX = true;
            constrainRotationTarget.x = target;
        }

        /// <summary>
        /// Constrain the camera's y rotation toward a target
        /// </summary>
        public void ConstrainRotationY(float target)
        {
            constrainRotationY = true;
            constrainRotationTarget.y = target;
        }

        /// <summary>
        /// Move the camera's x rotation toward a target angle
        /// </summary>
        public void LookTowardVertical(float targetAngle, float speed)
        {
            XLookRotation = Mathf.LerpAngle(XLookRotation, targetAngle, speed * Time.deltaTime);
            if (XLookRotation > 180f)
            {
                XLookRotation = -(360f - XLookRotation);
            }
        }

        void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            basePosition = transform.localPosition;
        }

        void LateUpdate()
        {
            DecayShake();

            //Get rotations
            Vector2 mouseDelta = playerMovement.Input.Controls.Player.Look.ReadValue<Vector2>() * (LookSensitivity * Time.timeScale);
            if (RotationLocksX > 0)
            {
                mouseDelta.y = 0f;
            }
            if (RotationLocksY > 0)
            {
                mouseDelta.x = 0f;
            }

            if (constrainRotationX || constrainRotationY)
            {
                // Get abs angle delta between current y rotation and the target constraint angle
                Vector2 distance = playerMovement.transform.localEulerAngles;
                distance.x = Mathf.DeltaAngle(distance.x, constrainRotationTarget.x);
                distance.y = Mathf.DeltaAngle(distance.y, constrainRotationTarget.y);
                if (distance.x < 0f)
                {
                    distance.x = -distance.x;
                }
                if (distance.y < 0f)
                {
                    distance.y = -distance.y;
                }

                // Normalize to maxConstrainDistance and cap
                distance /= maxConstrainDistance;
                if (distance.x > 1f)
                {
                    distance.x = 1f;
                }
                if (distance.y > 1f)
                {
                    distance.y = 1f;
                }

                // The constraint power should get more intense the further we are from the target angle
                if (constrainRotationX)
                {
                    mouseDelta.y *= 1f - constrainCurve.Evaluate(distance.x);
                }
                if (constrainRotationY)
                {
                    mouseDelta.x *= 1f - constrainCurve.Evaluate(distance.y);
                }
            }

            RotationDeltaFromMouse = mouseDelta;

            // Set x rotation
            XLookRotation = Mathf.Clamp(XLookRotation - mouseDelta.y, xBounds.x, xBounds.y);

            // Rotate player y
            playerMovement.transform.Rotate(0f, mouseDelta.x, 0f);

            if (reticle != null)
            {
                Vector3 position = basePosition + addPosition;
                position.y = Mathf.Max(position.y, minYPosition);
                transform.localPosition = position;
                reticle.anchoredPosition = addPosition * -reticlePosMultiplier;
                reticle.localEulerAngles = addRotation * -reticleRotMultiplier;
            }

            //float totalRotation = XLookRotation + addRotation.x;
            //if (totalRotation > xBounds.y)
            //{
            //    addRotation.x = totalRotation - xBounds.y;
            //}
            //else if (totalRotation < xBounds.x)
            //{
            //    addRotation.x = totalRotation - xBounds.x;
            //}

            // Rotate x and rotate arms in the opposite direction
            addRotation.x += XLookRotation;
            if (RotationLocksX == 0)
            {
                addRotation.x = Mathf.Clamp(addRotation.x, xBounds.x, xBounds.y);
            }

            transform.localRotation = Quaternion.Euler(addRotation);

            if (arms != null)
            {
                arms.localRotation = Quaternion.Euler(Mathf.Clamp((XLookRotation + addRotation.x) * armAngleMultiplier, armAngleBounds.x, armAngleBounds.y), 0f, 0f);
            }

            // Reset per-frame variables
            addPosition = Vector3.zero;
            addRotation = Vector3.zero;
            constrainRotationX = false;
            constrainRotationY = false;
        }

        /// <summary>
        /// Add translational screen shake
        /// </summary>
        public void ShakeTranslate(float intensity)
        {
            translationShake += intensity * translationShakeIntensity;
            if (translationShake > maxTranslationShake)
            {
                translationShake = maxTranslationShake;
            }
        }

        /// <summary>
        /// Add rotational screen shake
        /// </summary>
        public void ShakeRotation(float intensity)
        {
            rotationShake += intensity * rotationShakeIntensity;
            if (rotationShake > maxRotationShake)
            {
                rotationShake = maxRotationShake;
            }
        }

        void DecayShake()
        {
            translationShake -= translationShakeDecay * Time.deltaTime;
            if (translationShake < 0f)
            {
                translationShake = 0f;
            }

            rotationShake -= rotationShakeDecay * Time.deltaTime;
            if (rotationShake < 0f)
            {
                rotationShake = 0f;
            }

            Vector2 translationShakePosition = Random.insideUnitCircle * translationShake;
            float maxRotationShake = rotationShake;
            float rotationShakePosition = Random.Range(-maxRotationShake, maxRotationShake);

            addPosition.x += translationShakePosition.x;
            addPosition.y += translationShakePosition.y;
            addRotation.z += rotationShakePosition;
        }
    }
}