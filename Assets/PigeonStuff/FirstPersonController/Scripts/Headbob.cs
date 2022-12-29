using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Movement
{
    public sealed class Headbob : MonoBehaviour
    {
        [SerializeField] PlayerMovement playerMovement;
        [SerializeField] PlayerLook playerLook;

        [Space(20)]
        [SerializeField] float speedMultiplier;
        [SerializeField] float handSpeedMultiplier = 1f;
        [SerializeField] float fadeInSpeed;
        [SerializeField] float fadeOutSpeed;
        [SerializeField] Pigeon.Math.EaseFunction fadeCurve;
        Coroutine fadeCoroutine;

        float intensityMultiplier;
        float addIntensity;

        [Space(10)]
        [SerializeField] Vector3 runningHeadPosition;
        [SerializeField] Vector3 runningHeadRotation;

        [Space(10)]
        [SerializeField] float turnTiltIntensity;
        [SerializeField] float turnTiltSpeed;
        [SerializeField] float maxTiltMoveSpeed;
        [SerializeField] AnimationCurve moveSpeedTiltCurve;
        [SerializeField] float maxTilt;
        float currentTilt;

        public bool Active { get; private set; }

        [Space(20)]
        [SerializeField] Range xPosRange;
        [SerializeField] Range yPosRange;

        [Space(20)]
        [SerializeField] Range xRotRange;
        [SerializeField] Range yRotRange;
        [SerializeField] Range zRotRange;

        [Space(20)]
        [SerializeField] SpringSettings jumpSpring;
        [SerializeField] SpringSettings landSpring;
        [SerializeField] Vector2 landSpringIntensityBounds;
        [SerializeField] Vector2 landSpringSpeedBounds;
        [SerializeField] SpringSettings landHeadbobIntensity;
        [SerializeField] float rollSpeed = 1f;

        [Space(10)]
        [SerializeField] AnimationCurve rollCurve;
        [SerializeField] AnimationCurve rollPositionCurve;
        [SerializeField] Vector3 rollPosition;
        [SerializeField] float rollZRotation;

        [Space(10)]
        [SerializeField] float landingShockXAngle;
        [SerializeField] AnimationCurve landingShockXAngleCurve;
        [SerializeField] SpringSettings landingShockZRotationSpring;
        [SerializeField] SpringSettings3D landingShockPositionSpring;

        [Space(20)]
        [SerializeField] SpringSettings startSlideSpring;
        [SerializeField] SpringSettings endSlideSpring;
        [SerializeField][Range(0f, 1f)] float endSlideTime;
        [SerializeField] float slideShakeIntensity;
        [SerializeField] AnimationCurve slideShakeCurve;

        [Space(20)]
        [SerializeField] SpringSettings wallBounceSpring;

        [Space(20)]
        [SerializeField] Vector3 wallrunAngle;
        Vector3 currentWallrunAngle;
        [SerializeField] float wallrunAngleInitialSpeed;
        [SerializeField] AnimationCurve wallrunAngleInitialCurve;
        [SerializeField] AnimationCurve wallrunAngleDecayCurve;
        [SerializeField] float wallrunAngleEndSpeed;
        [SerializeField] AnimationCurve wallrunAngleEndCurve;
        Coroutine wallrunTiltCoroutine;

        [SerializeField] Range wallrunningXPosRange;
        [SerializeField] Range wallrunningYPosRange;
        [SerializeField] Range wallrunningZRotRange;
        float wallrunningHeadbobInterpolation;
        [SerializeField] float wallrunningHeadbobInterpolationSpeed;
        Coroutine wallrunningHeadbobInterpolationCoroutine;

        public event System.Action OnStep;
        float lastStepT;

        /// <summary>
        /// Represents a value that interpolates from a to b over a duration using a curve
        /// </summary>
        [System.Serializable]
        struct Range
        {
            public float valueA;
            public float valueB;
            public float duration;
            public AnimationCurve curve;

            /// <summary>
            /// Evaluate at a time. Time is automatically put into range, so a global value like Time.time can be used.
            public float Evaluate(float time)
            {
                return duration == 0f ? 0f : Mathf.LerpUnclamped(valueA, valueB, curve.Evaluate(time % duration / duration));
            }

            /// <summary>
            /// Get the normalized time percentage between a and b
            /// </summary>
            public float GetDurationPercentage(float time)
            {
                return time % duration / duration;
            }
        }

        /// <summary>
        /// Settings for springing the camera on 1 axis
        /// </summary>
        [System.Serializable]
        public struct SpringSettings
        {
            public float intensity;
            public float speed;
            public AnimationCurve curve;
        }

        /// <summary>
        /// Settings for springing the camera on 3 axes
        /// </summary>
        [System.Serializable]
        public struct SpringSettings3D
        {
            public Vector3 intensity;
            public float speed;
            public AnimationCurve curve;
        }

        public void AddIntensity(float intensity)
        {
            addIntensity += intensity;
        }

        void OnEnable()
        {
            playerMovement.OnJump += OnJump;
            playerMovement.OnLanded += OnLand;
            playerMovement.OnStartSlide += OnStartSlide;
            playerMovement.OnSlide += OnSlide;
            playerMovement.OnWallBounce += OnWallBounce;
            playerMovement.OnStartWallrun += OnStartWallrun;
            playerMovement.OnEndWallrun += OnEndWallrun;

            playerMovement.SetRunAnimationSpeed(speedMultiplier * handSpeedMultiplier);
        }

        void OnDisable()
        {
            playerMovement.OnJump -= OnJump;
            playerMovement.OnLanded -= OnLand;
            playerMovement.OnStartSlide -= OnStartSlide;
            playerMovement.OnSlide -= OnSlide;
            playerMovement.OnWallBounce -= OnWallBounce;
            playerMovement.OnStartWallrun -= OnStartWallrun;
            playerMovement.OnEndWallrun -= OnEndWallrun;
        }

        void Update()
        {
            Vector3 pos;
            Vector3 rot;

            // We don't want headbob while not running, not grounded, or sliding
            if (playerMovement.IsRunning && (playerMovement.Grounded || playerMovement.Wallrunning) && !playerMovement.Sliding)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }

            // Calculate headbob values
            if (Active || fadeCoroutine != null)
            {
                float time = Time.time * speedMultiplier;

                if (playerMovement.Wallrunning)
                {
                    wallrunningXPosRange.valueB = Mathf.Abs(wallrunningXPosRange.valueB) * -playerMovement.WallrunWallSide;
                }

                pos = new Vector3
                {
                    x = wallrunningHeadbobInterpolation == 0f ? xPosRange.Evaluate(time) :
                            Mathf.LerpUnclamped(xPosRange.Evaluate(time), wallrunningXPosRange.Evaluate(time), wallrunningHeadbobInterpolation),
                    y = wallrunningHeadbobInterpolation == 0f ? yPosRange.Evaluate(time) :
                            Mathf.LerpUnclamped(yPosRange.Evaluate(time), wallrunningYPosRange.Evaluate(time), wallrunningHeadbobInterpolation),
                };

                rot = new Vector3
                {
                    x = xRotRange.Evaluate(time),
                    y = yRotRange.Evaluate(time),
                    z = wallrunningHeadbobInterpolation == 0f ? zRotRange.Evaluate(time) :
                        Mathf.LerpUnclamped(zRotRange.Evaluate(time), wallrunningZRotRange.Evaluate(time), wallrunningHeadbobInterpolation)
                };

                // Apply headbob for this frame
                float intensity = intensityMultiplier + addIntensity * intensityMultiplier;
                playerLook.AddPosition(intensity * (pos + runningHeadPosition));
                playerLook.AddRotation(intensity * (rot + runningHeadRotation));
                addIntensity = 0f;

                float stepT = zRotRange.GetDurationPercentage(time);
                if (playerMovement.Grounded && ((stepT >= 0.5f && lastStepT < 0.5f) || (stepT >= 0f && lastStepT > stepT)))
                {
                    OnStep?.Invoke();
                }
                lastStepT = stepT;
            }

            //float tiltMultiplier = Mathf.Min(1f, moveSpeedTiltCurve.Evaluate(playerMovement.Speed / maxTiltMoveSpeed));
            //float newTilt = Mathf.Lerp(currentTilt, playerMovement.RotationDelta * turnTiltIntensity * tiltMultiplier, turnTiltSpeed * Time.deltaTime);
            //float absTilt = Mathf.Abs(newTilt);
            //if (absTilt < 0.05f)
            //{
            //    if (absTilt < Mathf.Abs(currentTilt))
            //    {
            //        newTilt = 0f;
            //    }
            //}
            //else
            //{
            //    newTilt = Mathf.Clamp(newTilt, -maxTilt, maxTilt);
            //}

            //currentTilt = newTilt;
            //playerLook.AddRotation(new Vector3(0f, 0f, currentTilt));
        }

        void Activate()
        {
            if (Active)
            {
                return;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            Active = true;
            fadeCoroutine = StartCoroutine(FadeIn());
        }

        void Deactivate()
        {
            if (!Active)
            {
                return;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            Active = false;
            fadeCoroutine = StartCoroutine(FadeOut());
        }

        /// <summary>
        /// Fade in headbob over time
        /// </summary>
        IEnumerator FadeIn()
        {
            float initialMultiplier = intensityMultiplier;
            float time = 0f;

            while (time < 1f)
            {
                time += fadeInSpeed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                intensityMultiplier = Mathf.LerpUnclamped(initialMultiplier, 1f, fadeCurve.Evaluate(time));
                yield return null;
            }

            fadeCoroutine = null;
        }

        /// <summary>
        /// Fade out headbob over time
        /// </summary>
        IEnumerator FadeOut()
        {
            float initialMultiplier = intensityMultiplier;
            float time = 0f;

            while (time < 1f)
            {
                time += fadeOutSpeed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                intensityMultiplier = initialMultiplier * (1f - fadeCurve.Evaluate(time));
                yield return null;
            }

            fadeCoroutine = null;
        }

        void OnJump()
        {
            Spring(jumpSpring);
        }

        void OnLand()
        {
            if (Time.time - playerMovement.SlideStartTime <= playerMovement.RollInputInterval && playerMovement.StartSlide())
            {
                StartCoroutine(RollCoroutine(rollSpeed, rollCurve, rollPositionCurve, rollPosition, rollZRotation));
            }
            else if (playerMovement.YSpeed < playerMovement.LandingShockSpeedTheshold)
            {
                PlayLandingShock();
            }
            else
            {
                Spring(landSpring.curve, Mathf.Clamp(landSpring.intensity * -playerMovement.YSpeed, landSpringIntensityBounds.x, landSpringIntensityBounds.y),
                    Mathf.Clamp(landSpring.speed / -playerMovement.YSpeed, landSpringSpeedBounds.x, landSpringSpeedBounds.y));
                AddIntensityOverTime(landHeadbobIntensity);
            }
        }

        void OnStartSlide()
        {
            Spring(startSlideSpring);
        }

        void OnSlide(float t, float prevT)
        {
            // Play endSlideSpring at endSlideTime% of the way through the slide
            if (t >= endSlideTime && prevT < endSlideTime)
            {
                Spring(endSlideSpring);
            }

            //playerLook.ShakeTranslateTemp(slideShakeIntensity * slideShakeCurve.Evaluate(t) * Time.deltaTime);
            ///playerLook.AddPosition(Random.insideUnitCircle * (slideShakeCurve.Evaluate(t) * Time.deltaTime));
        }

        void OnWallBounce()
        {
            Spring(wallBounceSpring);
        }

        /// <summary>
        /// Spring the camera rotation over time
        /// </summary>
        public void Spring(SpringSettings spring)
        {
            StartCoroutine(SpringCoroutine(spring.curve, spring.intensity, spring.speed));
        }

        /// <summary>
        /// Spring the camera rotation over time
        /// </summary>
        public void Spring(SpringSettings3D spring)
        {
            StartCoroutine(SpringCoroutine(spring.curve, spring.intensity, spring.speed));
        }

        /// <summary>
        /// Spring the camera rotation over time
        /// </summary>
        public void Spring(AnimationCurve curve, float intensity, float speed)
        {
            StartCoroutine(SpringCoroutine(curve, intensity, speed));
        }

        /// <summary>
        /// Spring the camera position over time
        /// </summary>
        public void SpringPosition(SpringSettings3D spring)
        {
            StartCoroutine(SpringPositionCoroutine(spring.curve, spring.intensity, spring.speed));
        }

        /// <summary>
        /// Spring the camera position over time
        /// </summary>
        public void SpringPosition(AnimationCurve curve, Vector3 intensity, float speed)
        {
            StartCoroutine(SpringPositionCoroutine(curve, intensity, speed));
        }

        IEnumerator SpringCoroutine(AnimationCurve curve, float intensity, float speed)
        {
            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                float springRotation = curve.Evaluate(time) * intensity;
                playerLook.AddRotation(new Vector3(springRotation, 0f, 0f));

                yield return null;
            }
        }

        IEnumerator SpringCoroutine(AnimationCurve curve, Vector3 intensity, float speed)
        {
            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                playerLook.AddRotation(curve.Evaluate(time) * intensity);

                yield return null;
            }
        }

        IEnumerator SpringPositionCoroutine(AnimationCurve curve, Vector3 intensity, float speed)
        {
            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                playerLook.AddPosition(intensity * curve.Evaluate(time));

                yield return null;
            }
        }

        /// <summary>
        /// Add headbob intensity over time
        /// </summary>
        public void AddIntensityOverTime(SpringSettings spring)
        {
            StartCoroutine(IntensityBurstCoroutine(spring.curve, spring.intensity, spring.speed));
        }

        /// <summary>
        /// Spring the camera rotation over time
        /// </summary>
        public void AddIntensityOverTime(AnimationCurve curve, float intensity, float speed)
        {
            StartCoroutine(IntensityBurstCoroutine(curve, intensity, speed));
        }

        IEnumerator IntensityBurstCoroutine(AnimationCurve curve, float intensity, float speed)
        {
            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                float addIntensity = curve.Evaluate(time) * intensity;
                AddIntensity(addIntensity);

                yield return null;
            }
        }

        IEnumerator RollCoroutine(float speed, AnimationCurve rotationCurve, AnimationCurve positionCurve, Vector3 addPosition, float zRotation)
        {
            playerLook.RotationLocksX++;
            playerLook.RotationLocksY++;
            playerMovement.MovementControlLocks++;

            //Vector3 initialAngle = playerLook.transform.localEulerAngles;
            //float targetAngle = 360f + Mathf.DeltaAngle(initialAngle.x + 360f, 0f);

            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                playerLook.AddRotation(new Vector3(360f * rotationCurve.Evaluate(time), 0f, zRotation * Pigeon.Math.EaseFunctions.BellCurveQuadratic(time)));
                playerLook.AddPosition(addPosition * positionCurve.Evaluate(time));

                yield return null;
            }

            playerLook.RotationLocksX--;
            playerLook.RotationLocksY--;
            playerMovement.MovementControlLocks--;
        }

        public void PlayLandingShock()
        {
            StartCoroutine(LandingShockCoroutine(landingShockXAngle, landingShockXAngleCurve, landingShockZRotationSpring, landingShockPositionSpring));
        }

        IEnumerator LandingShockCoroutine(float targetXRotation, AnimationCurve xRotationCurve, SpringSettings rotSettingsZ, SpringSettings3D posSettings)
        {
            playerLook.RotationLocksX++;
            playerLook.RotationLocksY++;
            playerMovement.MovementLocks++;

            float initialXRotation = playerLook.XLookRotation;
            float time = 0f;

            while (time < 1f)
            {
                time += rotSettingsZ.speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                playerLook.XLookRotation = Mathf.LerpUnclamped(initialXRotation, targetXRotation, xRotationCurve.Evaluate(time));
                playerLook.AddRotation(new Vector3(0f, 0f, rotSettingsZ.intensity * rotSettingsZ.curve.Evaluate(time)));
                //playerLook.AddPosition(posSettings.intensity * posSettings.curve.Evaluate(time));

                yield return null;
            }

            playerLook.RotationLocksX--;
            playerLook.RotationLocksY--;
            playerMovement.MovementLocks--;
        }

        void OnStartWallrun()
        {
            if (wallrunTiltCoroutine != null)
            {
                StopCoroutine(wallrunTiltCoroutine);
            }

            wallrunTiltCoroutine = StartCoroutine(WallrunTiltCoroutine());
            SetWallrunHeadbobInterpolation(1f);
        }

        void OnEndWallrun()
        {
            if (wallrunTiltCoroutine != null)
            {
                StopCoroutine(wallrunTiltCoroutine);
            }

            wallrunTiltCoroutine = StartCoroutine(WallrunEndTiltCoroutine());
            SetWallrunHeadbobInterpolation(0f);
        }

        IEnumerator WallrunTiltCoroutine()
        {
            Vector3 initialWallrunAngle = currentWallrunAngle;
            float time = 0f;

            while (time < 1f)
            {
                time += wallrunAngleInitialSpeed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                currentWallrunAngle = Vector3.LerpUnclamped(initialWallrunAngle, wallrunAngle * playerMovement.WallrunWallSide, wallrunAngleInitialCurve.Evaluate(time));
                playerLook.AddRotation(currentWallrunAngle);

                yield return null;
            }

            float startWallrunT = playerMovement.NormalizedWallrunTime;

            while (playerMovement.Wallrunning)
            {
                //this.Print((playerMovement.NormalizedWallrunTime - startWallrunT) / (1f - startWallrunT), startWallrunT, playerMovement.NormalizedWallrunTime, wallrunAngleDecayCurve.Evaluate((playerMovement.NormalizedWallrunTime - startWallrunT) / (1f - startWallrunT)) * wallrunAngle);
                currentWallrunAngle = playerMovement.WallrunWallSide * wallrunAngleDecayCurve.Evaluate((playerMovement.NormalizedWallrunTime - startWallrunT) / (1f - startWallrunT)) * wallrunAngle;
                playerLook.AddRotation(currentWallrunAngle);

                yield return null;
            }

            wallrunTiltCoroutine = null;
        }

        IEnumerator WallrunEndTiltCoroutine()
        {
            Vector3 initialWallrunAngle = currentWallrunAngle;
            float time = 0f;

            while (time < 1f)
            {
                time += wallrunAngleEndSpeed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                currentWallrunAngle = wallrunAngleEndCurve.Evaluate(1f - time) * initialWallrunAngle;
                playerLook.AddRotation(currentWallrunAngle);

                yield return null;
            }

            wallrunTiltCoroutine = null;
        }

        void SetWallrunHeadbobInterpolation(float target)
        {
            if (wallrunningHeadbobInterpolationCoroutine != null) StopCoroutine(wallrunningHeadbobInterpolationCoroutine);
            wallrunningHeadbobInterpolationCoroutine = StartCoroutine(SetWallrunningHeadbobInterpolationCoroutine(target));
        }

        IEnumerator SetWallrunningHeadbobInterpolationCoroutine(float target)
        {
            float initial = wallrunningHeadbobInterpolation;
            float time = 0f;

            while (time < 1f)
            {
                time += wallrunningHeadbobInterpolationSpeed * Time.deltaTime;
                if (time > 1f) time = 1f;

                wallrunningHeadbobInterpolation = Mathf.LerpUnclamped(initial, target, Pigeon.Math.EaseFunctions.EaseOutQuadratic(time));
                yield return null;
            }

            wallrunningHeadbobInterpolationCoroutine = null;
        }
    }
}