using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pigeon.Math;
using Random = UnityEngine.Random;

namespace Pigeon.Movement
{
    public sealed class PlayerLook : MonoBehaviour
    {
        [SerializeField] PlayerMovement playerMovement;
        public float lookSensitivity;

        public float XLookRotation { get; set; }

        public Vector2 RotationDeltaFromMouse { get; private set; }

        [SerializeField] Vector2 xBounds = new Vector2(-90f, 90f);

        public Vector2 XLookBounds => xBounds;

        Vector3 basePosition;
        Vector3 addPosition;
        Vector3 addRotation;

        [SerializeField] float minYPosition = -0.68f;

        [SerializeField] RectTransform reticle;
        [SerializeField] float reticlePosMultiplier = 20f;
        [SerializeField] float reticleRotMultiplier = 0.5f;

        [Space(20)]
        [SerializeField] float rotationShakeIntensity;
        [SerializeField] float rotationShakeDecay;
        [SerializeField] float maxRotationShake;
        [SerializeField] float translationShakeIntensity;
        [SerializeField] float translationShakeDecay;
        [SerializeField] float maxTranslationShake;
        float rotationShake;
        float translationShake;

        [Space(20)]
        [SerializeField] float maxConstrainDistance;
        [SerializeField] AnimationCurve constrainCurve;
        bool constrainRotationX;
        bool constrainRotationY;
        Vector2 constrainRotationTarget;

        [Space(20)]
        [SerializeField] Transform arms;
        [SerializeField] float armAngleMultiplier;
        [SerializeField] Vector2 armAngleBounds;

        public float ArmAngleMultiplier { get => armAngleMultiplier; set => armAngleMultiplier = value; }

        Vector2Int _rotationLocks;
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

        public void AddPosition(Vector3 pos)
        {
            addPosition += pos;
        }

        public void AddRotation(Vector3 pos)
        {
            addRotation += pos;
        }

        public void ConstrainRotation(Vector2 target)
        {
            constrainRotationX = true;
            constrainRotationY = true;
            constrainRotationTarget = target;
        }

        public void ConstrainRotationX(float target)
        {
            constrainRotationX = true;
            constrainRotationTarget.x = target;
        }

        public void ConstrainRotationY(float target)
        {
            constrainRotationY = true;
            constrainRotationTarget.y = target;
        }

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
            Vector2 mouseDelta = PlayerInput.Instance.Controls.Player.Look.ReadValue<Vector2>() * (lookSensitivity * Time.timeScale);
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

            Vector3 position = basePosition + addPosition;
            position.y = Mathf.Max(position.y, minYPosition);
            transform.localPosition = position;
            reticle.anchoredPosition = addPosition * -reticlePosMultiplier;
            reticle.localEulerAngles = addRotation * -reticleRotMultiplier;

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
            arms.localRotation = Quaternion.Euler(Mathf.Clamp((XLookRotation + addRotation.x) * armAngleMultiplier, armAngleBounds.x, armAngleBounds.y), 0f, 0f);

            addPosition = Vector3.zero;
            addRotation = Vector3.zero;
            constrainRotationX = false;
            constrainRotationY = false;
        }

        public void ShakeTranslate(float intensity)
        {
            translationShake += intensity * translationShakeIntensity;
            if (translationShake > maxTranslationShake)
            {
                translationShake = maxTranslationShake;
            }
        }

        public void ShakeRotation(float intensity)
        {
            rotationShake += intensity * rotationShakeIntensity;
            if (rotationShake > maxRotationShake)
            {
                rotationShake = maxRotationShake;
            }
        }

        //public void ShakeTranslateTemp(float intensity)
        //{
        //    addTranslationShake += intensity * translationShakeIntensity;
        //    translationShakePosition = Random.insideUnitCircle * (translationShake + addTranslationShake);
        //}

        //public void ShakeRotationTemp(float intensity)
        //{
        //    addRotationShake += intensity * rotationShakeIntensity;
        //    float maxRotationShake = rotationShake + addRotationShake;
        //    rotationShakePosition = Random.Range(-maxRotationShake, maxRotationShake);
        //}

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