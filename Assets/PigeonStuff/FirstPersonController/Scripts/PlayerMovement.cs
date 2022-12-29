using System.Collections;
using UnityEngine;

namespace Pigeon.Movement
{
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] CharacterController moveController;
        [SerializeField] PlayerLook playerLook;
        [SerializeField] Transform interpolatedTransform;

        public CharacterController MoveController => moveController;

        public PlayerLook PlayerLook => playerLook;

        Vector3 iPositionNow;
        Vector3 iPositionNext;

        Vector3 combinedVelocity;
        Vector3 moveVelocity;
        Vector3 fallVelocity;
        Vector3 externalVelocity;
        Vector3 physicsVelocity;

        public Vector3 PhysicsPosition => iPositionNext;

        public Vector3 Velocity => combinedVelocity;

        public Vector3 PrevRotation { get; private set; }
        public float RotationDelta { get; private set; }

        public float YSpeed => fallVelocity.y;
        public float Speed => currentMoveSpeed;

        Vector3 currentMoveDirection;
        float currentMoveSpeed;

        [Space(20)]
        [SerializeField] bool canSlide = true;
        [SerializeField] bool canRoll = true;
        [SerializeField] bool canWallrun = true;
        [SerializeField] bool canWallBounce = true;
        [SerializeField] bool canClamber = true;

        [Space(20)]
        [SerializeField] float gravityMultiplier;
        [SerializeField] float fallVelocityDrag;
        [SerializeField] float externalVelocityDragGrounded;
        [SerializeField] float externalVelocityDragAir;

        [Space(20)]
        [SerializeField] float moveSpeed;
        [SerializeField] float initialMoveSpeed;

        [Space(10)]
        [SerializeField] float speedAccelerationGrounded;
        [SerializeField] float speedAccelerationInAir;
        [SerializeField] float speedDeccelerationGrounded;
        [SerializeField] float speedDeccelerationInAir;
        [SerializeField] float directionAccelerationGrounded;
        [SerializeField] float directionAccelerationInAir;
        [SerializeField] float directionDeccelerationGrounded;
        [SerializeField] float directionDeccelerationInAir;
        [SerializeField] float accelerationWhenSlowing;
        [SerializeField] AnimationCurve runSpeedAccelerationCurve;
        [SerializeField] float runSpeedAccelerationDuration;

        [Space(10)]
        [SerializeField][Range(0f, 1f)] float strafeSpeedMultiplier;
        [SerializeField][Range(0f, 1f)] float strafeSpeedMultiplierWhileMoving;
        [SerializeField] float downhillSpeedMultiplier;
        [SerializeField][Range(0f, 1f)] float backwardSpeedMultiplier = 1f;

        [Space(10)]
        [SerializeField] float jumpSpeed;
        bool wantsToJump;
        [SerializeField][Range(0f, 1f)] float speedLostOnJump;
        [SerializeField][Range(0f, 1f)] float speedLostOnLanding;
        [field: SerializeField] public float LandingShockSpeedTheshold { get; private set; }

        [Space(20)]
        [SerializeField] float slideDuration;
        public float SlideStartTime { get; private set; } = float.MinValue;
        float slideStartSpeed;
        float slideProgress;
        float slideInitialYRotation;
        [SerializeField] float slideSpeed;
        [SerializeField] AnimationCurve slideSpeedCurve;
        [SerializeField] float slideSpeedAcceleration;
        [SerializeField] float slideDirectionAcceleration;
        [SerializeField] AnimationCurve slideAccelerationCurve;
        [SerializeField][Range(0f, 1f)] float slideStrafeMultiplier;
        [SerializeField] AnimationCurve slideCameraHeightCurve;
        [SerializeField] float slideCameraHeightMultiplier;
        [SerializeField] AnimationCurve slideCameraAngleCurve;
        [SerializeField] float slideCameraAngleMultiplier;

        [field: SerializeField] public float RollInputInterval { get; private set; }

        [Space(20)]
        [SerializeField] LayerMask groundLayerMask;
        [SerializeField] Vector3 groundCheckExtents;
        [SerializeField] Vector3 airborneGroundCheckExtents;
        [SerializeField] float downwardForceAlongSlope;
        float halfHeight;
        RaycastHit groundHit;

        [Space(20)]
        [SerializeField] float clamberCheckDistance;
        [SerializeField] LayerMask clamberLayerMask;
        [SerializeField] float clamberCheckWidth;
        [SerializeField] float minClamberHeight;
        [SerializeField] float maxClamberHeight;
        [SerializeField][Range(0f, 1f)] float maxClamberWallTilt = 0.75f;
        [SerializeField][Range(0f, 1f)] float maxClamberFloorTilt = 0.9f;
        [SerializeField] float clamberSpeed;
        [SerializeField] float minClamberDuration;
        [SerializeField] float maxClamberDuration;
        [SerializeField] AnimationCurve clamberCurveVertical;
        [SerializeField] AnimationCurve clamberCurveHorizontal;
        [SerializeField] float clamberExtraHorizontalDistance;
        [SerializeField] float clamberCameraAngleIntensity;
        [SerializeField] AnimationCurve clamberCamerAngleCurve;
        [SerializeField] float clamberBodyAngleIntensity;

        [Space(20)]
        [SerializeField] float wallrunSpeed;
        [SerializeField] float wallrunSpeedAcceleration;
        [SerializeField] float wallrunCheckDistance;
        [SerializeField] LayerMask wallrunLayerMask;
        [SerializeField, Range(0f, 1f)] float maxWallrunWallTilt;
        [SerializeField, Range(0f, 1f)] float wallrunMaxFacingWallDot;
        [SerializeField, Range(0f, 1f)] float targetWallrunLookAwayFromWallDot;
        [SerializeField] float wallrunLookAwayFromWallSpeed = 4f;
        [SerializeField] float wallrunDuration;
        [SerializeField] float wallrunWallDisableCooldown;
        float wallrunStartTime;
        float wallrunTargetCameraRotation;

        public float NormalizedWallrunTime => (Time.time - wallrunStartTime) / wallrunDuration;

        public int WallrunWallSide { get; private set; }

        [SerializeField] float wallrunYSpeedIntensity;
        [SerializeField] AnimationCurve wallrunYSpeedCurve;
        [SerializeField] float wallrunYSpeedDamp;

        [SerializeField] float wallrunJumpSpeedVertical;
        [SerializeField] float wallrunJumpSpeedBoost;
        [SerializeField, Range(0f, 1f)] float wallrunJumpForceAwayFromWall;

        [Space(20)]
        [SerializeField] Animator animator;

        public Animator Animator => animator;

        [field: SerializeField] public Transform HandBone { get; private set; }
        static readonly int HashRunning = Animator.StringToHash("Running");
        static readonly int HashRunSpeed = Animator.StringToHash("RunSpeed");
        static readonly int HashJump = Animator.StringToHash("Jump");
        static readonly int HashGrounded = Animator.StringToHash("Grounded");
        static readonly int HashSliding = Animator.StringToHash("Sliding");
        static readonly int HashClamber = Animator.StringToHash("Clamber");
        static readonly int HashWallrunning = Animator.StringToHash("Wallrunning");

        public bool IsRunning { get; private set; }
        float runStartTime;

        public bool Grounded { get; private set; }

        public bool Sliding { get; private set; }

        public bool Wallrunning { get; private set; }

        public event System.Action OnStartRunning;
        public event System.Action OnStopRunning;
        public event System.Action OnJump;

        /// <summary>
        /// Like OnEnteredGround, but only invoked if y velocity is negative
        /// </summary>
        public event System.Action OnLanded;
        public event System.Action OnLeftGround;
        public event System.Action OnEnteredGround;
        public event System.Action OnStartSlide;
        public event System.Action OnEndSlide;
        public event System.Action<float, float> OnSlide;
        public event System.Action OnWallBounce;
        public event System.Action OnStartWallrun;
        public event System.Action OnEndWallrun;

        int _movementLocks;
        public int MovementLocks
        {
            get => _movementLocks;
            set
            {
                if (_movementLocks == 0 && value > 0)
                {
                    if (IsRunning)
                    {
                        IsRunning = false;
                        OnStopRunning?.Invoke();
                    }
                }

                _movementLocks = value;
                if (_movementLocks < 0)
                {
                    _movementLocks = 0;
                }
            }
        }

        int _movementControlLocks;
        public int MovementControlLocks
        {
            get => _movementControlLocks;
            set
            {
                _movementControlLocks = value;
                if (_movementControlLocks < 0)
                {
                    _movementControlLocks = 0;
                }
            }
        }

        void Awake()
        {
            halfHeight = moveController.height * 0.5f;
        }

        void OnEnable()
        {
            OnStartRunning += PlayRunAnimation;
            OnStopRunning += StopRunAnimation;
            OnEnteredGround += EnableAnimatorGrounded;
            OnEnteredGround += ClearDisabledWallrunColliders;
            OnLeftGround += DisableAnimatorGrounded;
            //OnJump += PlayJumpAnimation;
            OnStartSlide += EnableAnimatorSlide;
            OnEndSlide += DisableAnimatorSlide;
            OnStartWallrun += EnableAnimatorWallrunning;
            OnEndWallrun += DisableAnimatorWallrunning;
        }

        void OnDisable()
        {
            OnStartRunning -= PlayRunAnimation;
            OnStopRunning -= StopRunAnimation;
            OnEnteredGround -= EnableAnimatorGrounded;
            OnEnteredGround -= ClearDisabledWallrunColliders;
            OnLeftGround -= DisableAnimatorGrounded;
            //OnJump -= PlayJumpAnimation;
            OnStartSlide -= EnableAnimatorSlide;
            OnEndSlide -= DisableAnimatorSlide;
            OnStartWallrun -= EnableAnimatorWallrunning;
            OnEndWallrun -= DisableAnimatorWallrunning;
        }

        public bool StartSlide()
        {
            // Only slide if our current velocity isn't behind us
            if (Sliding || Vector3.Dot(moveVelocity, transform.forward) < 0f)
            {
                return false;
            }

            Sliding = true;
            SlideStartTime = Time.time;
            slideStartSpeed = moveVelocity.magnitude;
            slideProgress = 0f;
            slideInitialYRotation = transform.localEulerAngles.y;
            OnStartSlide?.Invoke();

            return true;
        }

        void GetTargetVelocityAndAcceleration(Vector2 moveInput, bool wasRunning, bool wasGrounded, out Vector3 moveDirection, out float targetSpeed, out float speedAcceleration, out float directionAcceleration)
        {
            if (Sliding) // Sliding
            {
                // Get normalized slide time
                float t = (Time.time - SlideStartTime) / slideDuration;

                // Forward input should always be 1 when sliding so that we're moving forward
                // We should have little strafe control while sliding
                moveInput.y = 1f;
                moveInput.x *= slideStrafeMultiplier;

                targetSpeed = slideStartSpeed + slideSpeed * slideSpeedCurve.Evaluate(t);
                moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

                float accelerationMultiplier = slideAccelerationCurve.Evaluate(t);
                speedAcceleration = slideSpeedAcceleration * accelerationMultiplier;
                directionAcceleration = slideDirectionAcceleration * accelerationMultiplier;

                // Move camera down and angle it to the side
                //playerLook.AddPosition(new Vector3(0f, slideCameraHeightCurve.Evaluate(t) * slideCameraHeightMultiplier, 0f));
                //playerLook.AddRotation(new Vector3(0f, 0f, slideCameraAngleCurve.Evaluate(t) * slideCameraAngleMultiplier));

                // Constrain camera y rotation if grounded. Reset the target constrain angle if we leave the ground and touch it again.
                if (Grounded)
                {
                    if (!wasGrounded)
                    {
                        slideInitialYRotation = transform.localEulerAngles.y;
                    }

                    //playerLook.ConstrainRotationY(slideInitialYRotation);
                }

                //OnSlide?.Invoke(t, slideProgress);
                //slideProgress = t;

                if (t >= 1f) // End slide
                {
                    Sliding = false;
                    OnEndSlide?.Invoke();
                }

                return;
            }
            else if (Wallrunning) // Wallrunning
            {
                if (moveInput.y == 0f || Grounded)
                {
                    Wallrunning = false;
                    OnEndWallrun?.Invoke();
                    goto Run;
                }

                Vector3 forward = transform.forward;
                if (CheckForRunnableWalls(ref forward, false, out RaycastHit hit, out int side))
                {
                    WallrunWallSide = side;

                    if (wantsToJump && MovementControlLocks == 0)
                    {
                        fallVelocity.y += wallrunJumpSpeedVertical;
                        currentMoveSpeed += wallrunJumpSpeedBoost;
                        currentMoveDirection = Vector3.Slerp(currentMoveDirection, hit.normal, wallrunJumpForceAwayFromWall);
                        currentMoveSpeed *= speedLostOnJump;
                        OnJump?.Invoke();

                        Wallrunning = false;
                        OnEndWallrun?.Invoke();
                        goto Run;
                    }
                    else
                    {
                        DisableWallrunCollider(hit.collider);

                        float t = (Time.time - wallrunStartTime) / wallrunDuration;

                        moveDirection = forward * moveInput.y/* + transform.right * moveInput.x*/;
                        moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal);
                        targetSpeed = wallrunSpeed;
                        speedAcceleration = wallrunSpeedAcceleration;
                        directionAcceleration = directionAccelerationGrounded;

                        fallVelocity.y = Mathf.Lerp(fallVelocity.y, wallrunYSpeedIntensity * wallrunYSpeedCurve.Evaluate(t), wallrunYSpeedDamp * Time.deltaTime);

                        // Rotate camera away from wall
                        Vector3 cameraDirection = playerLook.transform.forward;
                        cameraDirection.y = 0f;
                        if (Vector3.Dot(cameraDirection, hit.normal) < targetWallrunLookAwayFromWallDot)
                        {
                            wallrunTargetCameraRotation = Quaternion.LookRotation(Vector3.LerpUnclamped(moveDirection, hit.normal, targetWallrunLookAwayFromWallDot), playerLook.transform.up).eulerAngles.y;
                            //playerLook.XLookRotation = Mathf.LerpAngle(playerLook.XLookRotation, targetRotation.x, wallrunLookAwayFromWallSpeed * Time.deltaTime);
                            //transform.localEulerAngles = new Vector3(0f, Mathf.LerpAngle(transform.localEulerAngles.y, wallrunTargetCameraRotation, wallrunLookAwayFromWallSpeed * Time.deltaTime), 0f);
                        }
                        else
                        {
                            wallrunTargetCameraRotation = float.NaN;
                        }

                        if (t >= 1f)
                        {
                            Wallrunning = false;
                            OnEndWallrun?.Invoke();
                            goto Run;
                        }
                    }
                }
                else
                {
                    Wallrunning = false;
                    OnEndWallrun?.Invoke();
                    goto Run;
                }

                return;
            }

Run:

            IsRunning = moveInput != Vector2.zero && MovementLocks == 0;
            moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

            if (IsRunning)
            {
                if (!wasRunning)
                {
                    runStartTime = Time.time;
                    currentMoveSpeed = Mathf.Max(currentMoveSpeed, initialMoveSpeed);
                }

                float runDuration = Time.time - runStartTime;
                if (runDuration <= runSpeedAccelerationDuration)
                {
                    targetSpeed = Mathf.LerpUnclamped(initialMoveSpeed, moveSpeed, runSpeedAccelerationCurve.Evaluate(runDuration / runSpeedAccelerationDuration));
                }
                else
                {
                    targetSpeed = moveSpeed;
                }

                ///if (!Grounded)
                ///{
                ///    targetSpeed *= Mathf.Abs(moveInput.y);
                ///}
            }
            else
            {
                targetSpeed = 0f;
            }

            // Running

            // Accelerate toward new move velocity
            if (IsRunning)
            {
                if (Grounded)
                {
                    speedAcceleration = speedAccelerationGrounded;
                    directionAcceleration = directionAccelerationGrounded;
                }
                else
                {
                    speedAcceleration = speedAccelerationInAir;
                    directionAcceleration = directionAccelerationInAir;
                }
            }
            else
            {
                if (Grounded)
                {
                    speedAcceleration = speedDeccelerationGrounded;
                    directionAcceleration = directionDeccelerationGrounded;
                }
                else
                {
                    speedAcceleration = speedDeccelerationInAir;
                    directionAcceleration = directionDeccelerationInAir;
                }
            }
        }

        void Movement()
        {
            bool wasRunning = IsRunning;

            // Box cast at our feet to check if we're grounded
            bool groundCheck = Physics.BoxCast(transform.localPosition, Grounded ? groundCheckExtents : airborneGroundCheckExtents, Vector3.down, out groundHit, Quaternion.identity, halfHeight, groundLayerMask, QueryTriggerInteraction.Ignore);
            bool wasGrounded = Grounded;
            Grounded = groundCheck || moveController.isGrounded;

            if (Grounded && !wasGrounded)
            {
                OnEnteredGround?.Invoke();

                if (fallVelocity.y < 0f)
                {
                    OnLanded?.Invoke();

                    // Lower speed on landing
                    currentMoveSpeed *= speedLostOnLanding;
                }
            }
            else if (!Grounded && wasGrounded)
            {
                OnLeftGround?.Invoke();
            }

            // Get move direction
            Vector2 moveInput = MovementControlLocks == 0 ? PlayerInput.Instance.Controls.Player.Move.ReadValue<Vector2>() : Vector2.zero;
            moveInput.x *= moveInput.y == 0f ? strafeSpeedMultiplier : strafeSpeedMultiplierWhileMoving;
            if (moveInput.y < 0f)
            {
                moveInput.y *= backwardSpeedMultiplier;
            }

            CheckForWallrun(moveInput);

            GetTargetVelocityAndAcceleration(moveInput, wasRunning, wasGrounded, out Vector3 moveDirection, out float targetSpeed, out float speedAcceleration, out float directionAcceleration);

            // Move slower up slopes and faster down slopes
            // Get our direction relative to the slope. Positive is moving uphill, negative is moving downhill. 0.5 and -0.5 are paralell/perfect uphill and downhill.
            ///float relativeSlopeDirection = Vector3.SignedAngle(Vector3.Cross(Vector3.up, groundHit.normal), moveDirection, Vector3.up) / 180f;
            ///if (relativeSlopeDirection < 0f)
            ///{
            ///    float downhillSteepness = -relativeSlopeDirection;
            ///    targetSpeed += downhillSpeedMultiplier * downhillSteepness;
            ///}

            // Decelerate speed much slower if we're slowing down but not stopping
            ///float currentSpeed = moveVelocity.magnitude;
            ///float targetSpeed = moveDirection.magnitude;
            ///if (currentSpeed != 0f) moveVelocity /= currentSpeed;
            ///if (targetSpeed != 0f) moveDirection /= targetSpeed;
            ///if (currentSpeed > targetSpeed && targetSpeed != 0f)
            ///{
            ///    targetSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelerationWhenSlowing * Time.deltaTime);
            ///}
            ///else
            ///{
            ///    targetSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedAcceleration * Time.deltaTime);
            ///}

            // Interpolate normalized moveVelocity and multiply by its new magnitude
            ///moveVelocity = Vector3.Lerp(moveVelocity, moveDirection, directionAcceleration * Time.deltaTime);
            ///moveVelocity *= targetSpeed;

            if (currentMoveSpeed > targetSpeed && targetSpeed != 0f)
            {
                speedAcceleration = accelerationWhenSlowing;
            }

            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetSpeed, speedAcceleration * Time.deltaTime);

            if (currentMoveSpeed < 0.001f)
            {
                currentMoveDirection = Vector3.zero;
            }
            else if (moveDirection != Vector3.zero)
            {
                currentMoveDirection = Vector3.Lerp(currentMoveDirection, moveDirection, directionAcceleration * Time.deltaTime);
            }



            moveVelocity = currentMoveDirection * currentMoveSpeed;

            if (IsRunning && !wasRunning)
            {
                OnStartRunning?.Invoke();
            }
            else if (!IsRunning && wasRunning)
            {
                OnStopRunning?.Invoke();
            }

            if (Grounded)
            {
                fallVelocity.y = 0f;

                // Handle jump
                if (wantsToJump && MovementControlLocks == 0)
                {
                    fallVelocity.y += jumpSpeed;
                    Grounded = false;
                    OnLeftGround?.Invoke();
                    currentMoveSpeed *= speedLostOnJump;
                    OnJump?.Invoke();
                }
            }
            else
            {
                // Accelerate fallVelocity by gravity
                fallVelocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            }

            fallVelocity.y += physicsVelocity.y;
            physicsVelocity.y = 0f;

            iPositionNow = transform.localPosition;

            // Combine velocities and move
            combinedVelocity = moveVelocity + fallVelocity + externalVelocity + physicsVelocity;
            Vector3 moveDelta = combinedVelocity;

            // Project our velocity onto the ground so we don't fly into the air when running downhill
            if (Grounded && Vector3.Dot(groundHit.normal, Vector3.up) != 1f)
            {
                //currentMoveDirection = Vector3.ProjectOnPlane(currentMoveDirection, Vector3.ProjectOnPlane(transform.forward, groundHit.normal)).normalized;
                moveDelta.y -= downwardForceAlongSlope;
            }

            var collisionFlags = moveController.Move(moveDelta * Time.deltaTime);

            iPositionNext = transform.localPosition;

            // Check if we hit a collider above us
            if ((collisionFlags & CollisionFlags.Above) != 0)
            {
                // We want to negate any positive y velocity so we don't stick to the collider above us
                if (fallVelocity.y > 0f)
                {
                    fallVelocity.y = 0f;
                }
            }

            // Apply linear drag to fallVelocity
            fallVelocity *= 1f - fallVelocityDrag * Time.deltaTime;
            externalVelocity *= 1f - externalVelocityDragGrounded * Time.deltaTime;
            physicsVelocity *= 1f - (Grounded ? externalVelocityDragGrounded : externalVelocityDragAir) * Time.deltaTime;
        }

        public void AddForce(Vector3 force)
        {
            physicsVelocity += force * Time.fixedDeltaTime;
        }

        public void AddImpulseForce(Vector3 force)
        {
            physicsVelocity += force;
        }

        public void AddForce(Vector3 force, ForceMode mode)
        {
            physicsVelocity += mode == ForceMode.Impulse ? force : force * Time.fixedDeltaTime;
        }

        void Update()
        {
            // Interpolate position
            interpolatedTransform.position = Vector3.Lerp(iPositionNow, iPositionNext, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);

            // Begin slide
            if (canRoll && !Sliding && !Wallrunning && MovementLocks == 0 && MovementControlLocks == 0 && PlayerInput.Instance.Controls.Player.Slide.WasPressedThisFrame())
            {
                SlideStartTime = Time.time;

                if (canSlide && !Sliding && Grounded)
                {
                    StartSlide();
                }
            }

            if (Sliding) // Sliding
            {
                // Get normalized slide time
                float t = (Time.time - SlideStartTime) / slideDuration;

                // Move camera down and angle it to the side
                playerLook.AddPosition(new Vector3(0f, slideCameraHeightCurve.Evaluate(t) * slideCameraHeightMultiplier, 0f));
                playerLook.AddRotation(new Vector3(0f, 0f, slideCameraAngleCurve.Evaluate(t) * slideCameraAngleMultiplier));

                // Constrain camera y rotation if grounded. Reset the target constrain angle if we leave the ground and touch it again.
                if (Grounded)
                {
                    //if (!wasGrounded)
                    //{
                    //    slideInitialYRotation = transform.localEulerAngles.y;
                    //}

                    playerLook.ConstrainRotationY(slideInitialYRotation);
                }

                OnSlide?.Invoke(t, slideProgress);
                slideProgress = t;
            }
            else if (Wallrunning)
            {
                if (!float.IsNaN(wallrunTargetCameraRotation))
                {
                    transform.localEulerAngles = new Vector3(0f, Mathf.LerpAngle(transform.localEulerAngles.y, wallrunTargetCameraRotation,
                        wallrunLookAwayFromWallSpeed * Time.deltaTime), 0f);
                }
            }

            if (PlayerInput.Instance.Controls.Player.Jump.WasPressedThisFrame())
            {
                wantsToJump = true;
            }
        }

        void LateUpdate()
        {
            Vector3 rotation = transform.localEulerAngles;
            RotationDelta = Mathf.DeltaAngle(rotation.y, PrevRotation.y) * Time.deltaTime;
            PrevRotation = rotation;
        }

        void FixedUpdate()
        {
            if (MovementLocks > 0)
            {
                wantsToJump = false;
                iPositionNow = iPositionNext;
                iPositionNext = transform.localPosition;
                return;
            }

            Movement();
            CheckForClamberableSurface();
            WallBounce();

            wantsToJump = false;
        }

        [SerializeField] float wallBounceSpeedTheshold;
        [SerializeField] float wallBounceForwardDirectionThreshold;
        [SerializeField] float wallBounceSpeed;
        [SerializeField] float wallBounceVelocityMultiplier;
        [SerializeField] AnimationCurve wallBounceSlowCurve;
        [SerializeField, Range(0f, 1f)] float wallBounceNormalizedTimeToAllowMovement;

        void WallBounce()
        {
            if (!canWallBounce || !Grounded || MovementLocks > 0 || MovementControlLocks > 0)
            {
                return;
            }

            Vector3 forward = transform.forward;
            if (Physics.Raycast(playerLook.transform.position, forward, out RaycastHit hit, moveController.radius + 0.1f, clamberLayerMask))
            {
                if (moveVelocity.sqrMagnitude > wallBounceSpeedTheshold * wallBounceSpeedTheshold && Vector3.Dot(moveVelocity, forward) > wallBounceForwardDirectionThreshold)
                {
                    StartCoroutine(WallBounceCoroutine());
                    OnWallBounce?.Invoke();
                }
            }
        }

        IEnumerator WallBounceCoroutine()
        {
            MovementControlLocks++;

            Vector3 initialMoveVelocity = moveVelocity * -wallBounceVelocityMultiplier;
            moveVelocity = Vector3.zero;
            float time = 0f;

            while (time < 1f)
            {
                float prevTime = time;
                time += wallBounceSpeed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                externalVelocity = initialMoveVelocity * wallBounceSlowCurve.Evaluate(1f - time);

                if (prevTime < wallBounceNormalizedTimeToAllowMovement && time >= wallBounceNormalizedTimeToAllowMovement)
                {
                    MovementControlLocks--;
                }

                yield return null;
            }
        }

        void CheckForClamberableSurface()
        {
            if (!canClamber || Grounded || !IsRunning || Sliding || MovementControlLocks > 0)
            {
                return;
            }

            // Only clamber if we're holding forward
            if (PlayerInput.Instance.Controls.Player.Move.ReadValue<Vector2>().y <= 0f)
            {
                return;
            }

            Vector3 playerPosition = transform.localPosition;

            // Raycast for wall in front of us
            if (Physics.Raycast(playerPosition, transform.forward, out RaycastHit hit, clamberCheckDistance, clamberLayerMask))
            {
                // Make sure wall is facing us
                if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < maxClamberWallTilt)
                {
                    Vector3 wallHitPos = hit.point;
                    Vector3 boxCastStartPos = wallHitPos + new Vector3(0f, maxClamberHeight, 0f);
                    Vector3 boxCheckSize = new Vector3(clamberCheckWidth, 0.15f, 0.1f);
                    Quaternion playerRotation = transform.localRotation;

                    // Check for another collider at the start of our boxcast to make sure we don't clamber through a wall that's above the target wall
                    // CheckBox will detect the inside of a collider, whereas BoxCast will not
                    if (Physics.CheckBox(boxCastStartPos, boxCheckSize, playerRotation, clamberLayerMask))
                    {
                        return;
                    }

                    // Cast a box from above down toward the hit point to check for a floor on top of the wall
                    if (Physics.BoxCast(boxCastStartPos, boxCheckSize, Vector3.down, out RaycastHit floorHit, playerRotation, maxClamberHeight, clamberLayerMask))
                    {
                        // Make sure the floor is the same object as the wall AND is flat AND is far enough above us
                        Vector3 floorPos = floorHit.point;
                        if (floorPos.y - playerPosition.y >= minClamberHeight && floorHit.transform == hit.transform && Vector3.Dot(floorHit.normal, Vector3.up) > maxClamberFloorTilt)
                        {
                            // Add half our height so we actually clamber to the top of the wall
                            Vector3 targetPos = floorPos;
                            targetPos.y += moveController.height * 0.5f;

                            // Set horizontal target to the wall hit because the position of the floor hit could be anywhere along the box cast
                            targetPos.x = wallHitPos.x;
                            targetPos.z = wallHitPos.z;

                            // Add horizontal distance to the target so we clamber up and OVER rather than just up
                            Vector3 direction = targetPos - playerPosition;
                            direction.y = 0f;
                            direction.Normalize();
                            targetPos += direction * clamberExtraHorizontalDistance;

                            StartCoroutine(ClamberCoroutine(targetPos));
                        }
                    }
                }
            }
        }

        IEnumerator ClamberCoroutine(Vector3 targetPos)
        {
            MovementLocks++;

            PlayClamberAnimation();

            Vector3 initialPos = transform.localPosition;
            float duration = Mathf.Abs(targetPos.y - initialPos.y) / clamberSpeed;
            duration = Mathf.Clamp(duration, minClamberDuration, maxClamberDuration);
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float normalizedTime = time / duration;
                if (normalizedTime > 1f)
                {
                    normalizedTime = 1f;
                }

                float horizontalT = clamberCurveHorizontal.Evaluate(normalizedTime);
                transform.localPosition = new Vector3(Mathf.LerpUnclamped(initialPos.x, targetPos.x, horizontalT),
                    Mathf.LerpUnclamped(initialPos.y, targetPos.y, clamberCurveVertical.Evaluate(normalizedTime)),
                    Mathf.LerpUnclamped(initialPos.z, targetPos.z, horizontalT));

                //Vector3 directionTowardLedge = ledgePos - playerLook.transform.position;
                //Quaternion targetRotation = Quaternion.LookRotation(directionTowardLedge);
                //playerLook.LookTowardVertical(/*targetRotation.eulerAngles.x*/clamberCameraAngle, clamberLookAtLedgeIntensity);
                playerLook.AddRotation(new Vector3(clamberCamerAngleCurve.Evaluate(normalizedTime) * clamberCameraAngleIntensity, 0f, 0f));

                Vector3 directionTowardLedge = targetPos - playerLook.transform.position;
                Vector3 targetRotation = Quaternion.LookRotation(directionTowardLedge).eulerAngles;
                playerLook.ConstrainRotationY(targetRotation.y);

                yield return null;
            }

            MovementLocks--;
        }

        //struct DisabledCollider
        //{
        //    public Collider collider;
        //    public float disableTime;
        //}

        //readonly List<DisabledCollider> disabledWallrunColliders = new List<DisabledCollider>();

        Collider lastTouchedWallrunCollider;
        float lastTouchedWallrunColliderTime;

        void CheckForWallrun(Vector2 moveInput)
        {
            if (!canWallrun || Grounded || Wallrunning || Sliding || MovementControlLocks > 0)
            {
                return;
            }

            if (moveInput.y == 0f)
            {
                return;
            }

            //for (int i = disabledWallrunColliders.Count - 1; i >= 0; i--)
            //{
            //    if (Time.time - disabledWallrunColliders[i].disableTime > wallrunWallDisableCooldown)
            //    {
            //        disabledWallrunColliders.RemoveAt(i);
            //    }
            //}

            if (Time.time - lastTouchedWallrunColliderTime > wallrunWallDisableCooldown)
            {
                lastTouchedWallrunCollider = null;
            }

            Vector3 forward = transform.forward;
            if (CheckForRunnableWalls(ref forward, true, out RaycastHit hit, out _) && Vector3.Dot(forward, hit.normal) > -wallrunMaxFacingWallDot)
            {
                Wallrunning = true;
                wallrunStartTime = Time.time;
                OnStartWallrun?.Invoke();
            }
        }

        bool CheckForRunnableWalls(ref Vector3 forward, bool checkIfWallsAreDisabled, out RaycastHit hit, out int side)
        {
            Vector3 right = transform.right;
            side = 1;

            if (Physics.Raycast(transform.localPosition, right, out hit, wallrunCheckDistance, wallrunLayerMask) && (!checkIfWallsAreDisabled || !IsWallrunColliderDisabled(hit.collider)))
            {
                if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < maxWallrunWallTilt)
                {
                    return true;
                }
            }

            Vector3 left = -right;
            side = -1;

            if (Physics.Raycast(transform.localPosition, left, out hit, wallrunCheckDistance, wallrunLayerMask) && (!checkIfWallsAreDisabled || !IsWallrunColliderDisabled(hit.collider)))
            {
                if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < maxWallrunWallTilt)
                {
                    return true;
                }
            }

            side = 1;

            if (Physics.Raycast(transform.localPosition, Vector3.Lerp(right, forward, 0.25f), out hit, wallrunCheckDistance, wallrunLayerMask) && (!checkIfWallsAreDisabled || !IsWallrunColliderDisabled(hit.collider)))
            {
                if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < maxWallrunWallTilt)
                {
                    return true;
                }
            }

            side = -1;

            if (Physics.Raycast(transform.localPosition, Vector3.Lerp(left, forward, 0.25f), out hit, wallrunCheckDistance, wallrunLayerMask) && (!checkIfWallsAreDisabled || !IsWallrunColliderDisabled(hit.collider)))
            {
                if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < maxWallrunWallTilt)
                {
                    return true;
                }
            }

            return false;
        }

        void DisableWallrunCollider(Collider collider)
        {
            //for (int i = 0; i < disabledWallrunColliders.Count; i++)
            //{
            //    if (disabledWallrunColliders[i].collider == collider)
            //    {
            //        disabledWallrunColliders[i] = new DisabledCollider() { collider = collider, disableTime = Time.time };
            //        return;
            //    }
            //}

            //disabledWallrunColliders.Add(new DisabledCollider() { collider = collider, disableTime = Time.time } );
            lastTouchedWallrunCollider = collider;
            lastTouchedWallrunColliderTime = Time.time;
        }

        bool IsWallrunColliderDisabled(Collider collider)
        {
            //for (int i = 0; i < disabledWallrunColliders.Count; i++)
            //{
            //    if (disabledWallrunColliders[i].collider == collider)
            //    {
            //        return true;
            //    }
            //}

            //return false;
            return collider == lastTouchedWallrunCollider;
        }

        void ClearDisabledWallrunColliders()
        {
            //disabledWallrunColliders.Clear();
            lastTouchedWallrunCollider = null;
        }

        void PlayRunAnimation()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(HashRunning, true);
        }

        void StopRunAnimation()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(HashRunning, false);
        }

        public void SetRunAnimationSpeed(float speed)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetFloat(HashRunSpeed, speed);
        }

        void DisableAnimatorGrounded()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(HashGrounded, false);
            PlayJumpAnimation();
        }

        void EnableAnimatorGrounded()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(HashGrounded, true);
        }

        void PlayJumpAnimation()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(HashJump);
        }

        void PlayClamberAnimation()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(HashClamber);
        }

        void DisableAnimatorSlide()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(HashSliding, false);
        }

        void EnableAnimatorSlide()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(HashSliding, true);
        }

        void EnableAnimatorWallrunning()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(HashWallrunning, true);
        }

        void DisableAnimatorWallrunning()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(HashWallrunning, false);
        }

        public bool DisableAnimator { get; set; }

        /// <summary>
        /// Play an arm animation. Returns the remaining length of the animation.
        /// </summary>
        public void PlayAnimation(int nameID)
        {
            if (animator == null || DisableAnimator)
            {
                return;
            }

            //animator.StopPlayback();
            //var state = animator.GetCurrentAnimatorStateInfo(0);
            //return state.length - (state.length * state.normalizedTime);

            //animator.Play(nameID, 0, 0f);
            animator.CrossFade(nameID, 0.1f);
        }
    }
}