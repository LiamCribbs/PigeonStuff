using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pigeon.Movement
{
    public sealed class Headbob : MonoBehaviour
    {
        [SerializeField] PlayerMovement playerMovement;
        [SerializeField] PlayerLook playerLook;

        [SerializeField, Tooltip("Headbob speed multiplier")] float speedMultiplier;
        [SerializeField, Tooltip("Running animation speed multiplier, applied on top of speedMultiplier")] float handSpeedMultiplier = 1f;
        [SerializeField, Tooltip("How fast headbob fades in when starting to move")] float fadeInSpeed;
        [SerializeField, Tooltip("How fast headbob fades out when no longer moving")] float fadeOutSpeed;
        [SerializeField, Tooltip("Curve for headbob intensity fading in and out")] Pigeon.Math.EaseFunction fadeCurve;
        Coroutine fadeCoroutine;

        float intensityMultiplier;
        float addIntensity;

        [SerializeField, Tooltip("Target camera position offset when running." +
            "\n\nLowering the head slightly can simulate a body leaning forward while sprinting.")] Vector3 runningHeadPosition;
        [SerializeField, Tooltip("Target camera rotation offset when running")] Vector3 runningHeadRotation;

        /// <summary>
        /// Is headbob currently being applied?
        /// </summary>
        public bool Active { get; private set; }

        [SerializeField, Min(0f), Tooltip("Headbob intensity multiplier")] float headbobIntensity = 1f;

        [SerializeField, Tooltip("Positional headbob along the x axis")] Range xPosRange;
        [SerializeField, Tooltip("Positional headbob along the y axis")] Range yPosRange;

        [SerializeField, Tooltip("Positional headbob along the x axis")] Range xRotRange;
        [SerializeField, Tooltip("Positional headbob along the y axis")] Range yRotRange;
        [SerializeField, Tooltip("Positional headbob along the z axis")] Range zRotRange;

        [SerializeField, Tooltip("X rotation animation when jumping")] SpringSettings jumpSpring;
        [SerializeField, Tooltip("X rotation animation when landing")] SpringSettings landSpring;
        [SerializeField, Tooltip("Minimum and maximum intensity multipliers from y speed when landing")] Vector2 landSpringIntensityBounds;
        [SerializeField, Tooltip("Minimum and maximum speed multipliers from y speed when landing")] Vector2 landSpringSpeedBounds;
        [SerializeField, Tooltip("Temporary headbob intensity added when landing")] SpringSettings landHeadbobIntensity;

        [SerializeField, Tooltip("Target x angle during a landing shock")] float landingShockXAngle;
        [SerializeField, Tooltip("How the camera rotates over the course of a landing shock")] AnimationCurve landingShockXAngleCurve;
        [SerializeField, Tooltip("Z rotation animation during a landing shock")] SpringSettings landingShockZRotationSpring;
        [SerializeField, Tooltip("Camera position animation during a landing shock")] SpringSettings3D landingShockPositionSpring;

        [SerializeField, Tooltip("How fast the camera moves and rotates toward its target when starting a slide")] float startSlideSpeed;
        [SerializeField, Tooltip("How fast the camera moves and rotates back to normal when ending a slide")] float endSlideSpeed;
        [SerializeField, Tooltip("Camera position offset when sliding")] Vector3 slidePosition;
        [SerializeField, Tooltip("How the camera moves to its offset when starting a slide")] AnimationCurve startSlidePositionCurve;
        [SerializeField, Tooltip("How the camera moves back to normal when ending a slide")] AnimationCurve endSlidePositionCurve;
        [SerializeField, Tooltip("Camera rotation offset when sliding")] Vector3 slideRotation;
        [SerializeField, Tooltip("How the camera z rotation changes as its y rotation approaches 90 degrees away from the slide direction")] AnimationCurve slideRotationFromDirectionCurve;
        [SerializeField, Tooltip("How the camera rotates to its offset when starting a slide")] AnimationCurve startSlideRotationCurve;
        [SerializeField, Tooltip("How the camera rotates back to normal when ending a slide")] AnimationCurve endSlideRotationCurve;
        Coroutine slideCoroutine;
        Vector3 slideAddPosition;
        Vector3 slideAddRotation;

        [SerializeField, Tooltip("How fast the camera rolls if rolling is enabled on PlayerMovemet")] float rollSpeed = 1f;
        [SerializeField, Tooltip("How the camera rotates over the course of a roll")] AnimationCurve rollCurve;
        [SerializeField, Tooltip("How the camera moves over the course of a roll")] AnimationCurve rollPositionCurve;
        [SerializeField, Tooltip("Target camera position during a roll")] Vector3 rollPosition;
        [SerializeField, Tooltip("Target camera z tilt during a roll")] float rollZRotation;

        [SerializeField, Tooltip("Camera angle offset when wallrunning")] Vector3 wallrunAngle;
        Vector3 currentWallrunAngle;
        [SerializeField, Tooltip("How fast the camera rotates to wallrunAngle when beginning a wallrun")] float wallrunAngleInitialSpeed;
        [SerializeField, Tooltip("How the camera rotates toward wallrunAngle when beginning a wallrun")] AnimationCurve wallrunAngleInitialCurve;
        [SerializeField, Tooltip("How the camera rotates back to normal over the course of a wallrun")] AnimationCurve wallrunAngleDecayCurve;
        [SerializeField, Tooltip("How fast the camera rotates back to normal when ending a wallrun")] float wallrunAngleEndSpeed;
        [SerializeField, Tooltip("How the camera rotates back to normal when ending an wallrun")] AnimationCurve wallrunAngleEndCurve;
        Coroutine wallrunTiltCoroutine;

        [SerializeField, Tooltip("Positional headbob along the x axis when wallrunning")] Range wallrunningXPosRange;
        [SerializeField, Tooltip("Positional headbob along the y axis when wallrunning")] Range wallrunningYPosRange;
        [SerializeField, Tooltip("Rotational headbob along the z axis when wallrunning")] Range wallrunningZRotRange;
        float wallrunningHeadbobInterpolation;
        [SerializeField, Tooltip("How fast headbob blends to the wallrunning headbob ranges when wallrunning")] float wallrunningHeadbobInterpolationSpeed;
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
            playerMovement.OnEndSlide += OnEndSlide;
            playerMovement.OnStartWallrun += OnStartWallrun;
            playerMovement.OnEndWallrun += OnEndWallrun;

            playerMovement.SetRunAnimationSpeed(speedMultiplier * handSpeedMultiplier);
        }

        void OnDisable()
        {
            playerMovement.OnJump -= OnJump;
            playerMovement.OnLanded -= OnLand;
            playerMovement.OnStartSlide -= OnStartSlide;
            playerMovement.OnEndSlide -= OnEndSlide;
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
                float intensity = ((intensityMultiplier + addIntensity * intensityMultiplier) * playerMovement.Speed / playerMovement.DefaultMoveSpeed) * headbobIntensity;
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
            if (playerMovement.Sliding && !playerMovement.HasTouchedGroundDuringSlide)
            {
                if (playerMovement.EnableRoll)
                {
                    StartCoroutine(RollCoroutine(rollSpeed, rollCurve, rollPositionCurve, rollPosition, rollZRotation));
                }
            }
            else if (playerMovement.EnableLandingShock && playerMovement.YSpeed < playerMovement.LandingShockSpeedTheshold)
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
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }

            slideCoroutine = StartCoroutine(StartSlideCoroutine());
        }

        void OnEndSlide()
        {
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }

            slideCoroutine = StartCoroutine(EndSlideCoroutine());
        }

        IEnumerator StartSlideCoroutine()
        {
            Vector3 initialAddPos = slideAddPosition;
            Vector3 initialAddRot = slideAddRotation;
            float time = 0f;

            while (time < 1f)
            {
                time += startSlideSpeed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                float angleFromDirection = Vector3.SignedAngle(transform.forward, playerMovement.Velocity, Vector3.up);
                float sign = Mathf.Sign(angleFromDirection);
                if (angleFromDirection < 0f)
                {
                    angleFromDirection = -angleFromDirection;
                }
                if (angleFromDirection > 90f)
                {
                    angleFromDirection = 90f - (angleFromDirection - 90f);
                }

                Vector3 rotation = new Vector3(slideRotation.x, slideRotation.y, slideRotation.z *
                    slideRotationFromDirectionCurve.Evaluate(angleFromDirection / 90f) * sign);

                slideAddPosition = Vector3.LerpUnclamped(initialAddPos, slidePosition, startSlidePositionCurve.Evaluate(time));
                slideAddRotation = Vector3.LerpUnclamped(initialAddRot, rotation, startSlideRotationCurve.Evaluate(time));
                playerLook.AddPosition(slideAddPosition);
                playerLook.AddRotation(slideAddRotation);

                yield return null;
            }

            while (true)
            {
                float angleFromDirection = Vector3.SignedAngle(transform.forward, playerMovement.Velocity, Vector3.up);
                float sign = Mathf.Sign(angleFromDirection);
                if (angleFromDirection < 0f)
                {
                    angleFromDirection = -angleFromDirection;
                }
                if (angleFromDirection > 90f)
                {
                    angleFromDirection = 90f - (angleFromDirection - 90f);
                }
                
                slideAddRotation.z = Mathf.Lerp(slideAddRotation.z, slideRotation.z * 
                    slideRotationFromDirectionCurve.Evaluate(angleFromDirection / 90f) * sign, 7f * Time.deltaTime);

                playerLook.AddPosition(slideAddPosition);
                playerLook.AddRotation(slideAddRotation);

                yield return null;
            }

            //slideCoroutine = null;
        }

        IEnumerator EndSlideCoroutine()
        {
            Vector3 initialAddPos = slideAddPosition;
            Vector3 initialAddRot = slideAddRotation;
            float time = 0f;

            while (time < 1f)
            {
                time += endSlideSpeed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                slideAddPosition = Vector3.LerpUnclamped(initialAddPos, Vector3.zero, endSlidePositionCurve.Evaluate(time));
                slideAddRotation = Vector3.LerpUnclamped(initialAddRot, Vector3.zero, endSlideRotationCurve.Evaluate(time));
                playerLook.AddPosition(slideAddPosition);
                playerLook.AddRotation(slideAddRotation);

                yield return null;
            }

            slideCoroutine = null;
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

        /// <summary>
        /// Plays landing shock spring and locks movement and rotation until it's done
        /// </summary>
        public void PlayLandingShock()
        {
            if (!playerMovement.EnableLandingShock)
            {
                return;
            }

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

#if UNITY_EDITOR
        [CustomEditor(typeof(Headbob))]
        class HeadbobEditor : Editor
        {
            Headbob script;

            static bool headbobFoldout;
            static bool jumpFoldout;
            static bool slideFoldout;
            static bool wallrunFoldout;

            void OnEnable()
            {
                script = (Headbob)target;
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.playerMovement)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.playerLook)));

                EditorGUILayout.Space(20);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.speedMultiplier)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.handSpeedMultiplier)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.fadeInSpeed)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.fadeOutSpeed)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.fadeCurve)));

                EditorGUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.runningHeadPosition)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.runningHeadRotation)));

                EditorGUILayout.Space(10);

                headbobFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(headbobFoldout, "Headbob");
                if (headbobFoldout)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.HelpBox("Headbob ranges interpolate between valueA and valueB over a duration, using an easing curve.", MessageType.Info);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.headbobIntensity)));

                    EditorGUILayout.Space(10);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.xPosRange)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.yPosRange)));

                    EditorGUILayout.Space(10);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.xRotRange)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.yRotRange)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.zRotRange)));

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(10);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space(10);

                jumpFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(jumpFoldout, "Jump");
                if (jumpFoldout)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.HelpBox("Springs animate the camera's rotation or position over time, and should always return to 0 at the end.", MessageType.Info);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.jumpSpring)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.landSpring)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.landSpringIntensityBounds)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.landSpringSpeedBounds)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.landHeadbobIntensity)));

                    if (script.playerMovement.EnableLandingShock)
                    {
                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField("Landing Shock", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.landingShockXAngle)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.landingShockXAngleCurve)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.landingShockZRotationSpring)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.landingShockPositionSpring)));
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(10);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (script.playerMovement.EnableSlide)
                {
                    EditorGUILayout.Space(10);

                    slideFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(slideFoldout, "Sliding");
                    if (slideFoldout)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.startSlideSpeed)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.endSlideSpeed)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.slidePosition)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.startSlidePositionCurve)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.endSlidePositionCurve)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.slideRotation)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.slideRotationFromDirectionCurve)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.startSlideRotationCurve)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.endSlideRotationCurve)));

                        if (script.playerMovement.EnableRoll)
                        {
                            EditorGUILayout.Space(10);
                            EditorGUILayout.LabelField("Roll", EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.rollSpeed)));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.rollCurve)));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.rollPositionCurve)));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.rollPosition)));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.rollZRotation)));
                        }

                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space(10);
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                if (script.playerMovement.EnableWallrun)
                {
                    EditorGUILayout.Space(10);

                    wallrunFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(wallrunFoldout, "Wallrun");
                    if (wallrunFoldout)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunAngle)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunAngleInitialSpeed)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunAngleInitialCurve)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunAngleDecayCurve)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunAngleEndSpeed)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunAngleEndCurve)));

                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField("Wallrun Headbob", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunningXPosRange)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunningYPosRange)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunningZRotRange)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunningHeadbobInterpolationSpeed)));

                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space(10);
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}