using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pigeon.Math;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pigeon.Movement
{
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] CharacterController moveController;
        [SerializeField] PlayerLook playerLook;
        [SerializeField, Tooltip("Child transform whose position will be interpolated between FixedUpdates")] Transform interpolatedTransform;
        public PlayerInput Input { get; private set; }

        [SerializeField] Animator animator;

        public Animator Animator => animator;

        public CharacterController MoveController => moveController;

        public PlayerLook PlayerLook => playerLook;

        Vector3 iPositionNow;
        Vector3 iPositionNext;

        Vector3 combinedVelocity;
        Vector3 moveVelocity;
        Vector3 fallVelocity;
        Vector3 externalVelocity;
        Vector3 physicsVelocity;

        /// <summary>
        /// Non-interpolated position
        /// </summary>
        public Vector3 PhysicsPosition => iPositionNext;

        /// <summary>
        /// Current velocity of the player
        /// </summary>
        public Vector3 Velocity => combinedVelocity;

        /// <summary>
        /// Rotation last frame
        /// </summary>
        public Vector3 PrevRotation { get; private set; }

        /// <summary>
        /// Rotation change this frame
        /// </summary>
        public float RotationDelta { get; private set; }

        /// <summary>
        /// Current y velocity
        /// </summary>
        public float YSpeed => fallVelocity.y;

        /// <summary>
        /// Current move speed
        /// </summary>
        public float Speed => currentMoveSpeed;

        Vector3 currentMoveDirection;
        float currentMoveSpeed;

        [SerializeField, Tooltip("Enables a walk speed when not holding the sprint key. Otherwise we always move at one speed.")] bool enableSeparateSprint = false;
        [SerializeField, Tooltip("Enables running horizontally along walls")] bool enableWallrun = true;
        [SerializeField, Tooltip("Enables sliding and crouching")] bool enableSlide = true;
        [SerializeField, Tooltip("Enables rolling when landing during a slide")] bool enableRoll = true;
        [SerializeField, Tooltip("Enables clambering up vertical surfices")] bool enableClamber = true;
        [SerializeField, Tooltip("Enables a temporary shock state when landing over a certain speed theshold")] bool enableLandingShock = false;

        public bool EnableSeparateSprint { get => enableSeparateSprint; set => enableSeparateSprint = value; }
        public bool EnableWallrun { get => enableWallrun; set => enableWallrun = value; }
        public bool EnableSlide { get => enableSlide; set => enableSlide = value; }
        public bool EnableRoll { get => enableRoll; set => enableRoll = value; }
        public bool EnableClamber { get => enableClamber; set => enableClamber = value; }
        public bool EnableLandingShock { get => enableLandingShock; set => enableLandingShock = value; }

        [SerializeField, Tooltip("How much gravity is applied (multiplied by 9.8)")] float gravityMultiplier = 1f;
        [SerializeField, Min(0f), Tooltip("Drag applied to fall velocity")] float fallVelocityDrag;
        [SerializeField, Min(0f), Tooltip("Drag applied to velocity that has been added using AddForce or AddImpulseForce when grounded")] float externalVelocityDragGrounded = 0.25f;
        [SerializeField, Min(0f), Tooltip("Drag applied to velocity that has been added using AddForce or AddImpulseForce when airborne")] float externalVelocityDragAir = 1f;

        [SerializeField, Min(0f), Tooltip("Target run speed")] float moveSpeed = 9f;
        [SerializeField, Min(0f), Tooltip("Target speed when not sprinting")] float walkSpeed = 5f;
        [SerializeField, Min(0f), Tooltip("Initial speed when we start moving and begin accelerating to the target run/walk speed")] float initialMoveSpeed = 4f;

        [SerializeField, Range(0f, 1f), Tooltip("Multiplier to left/right input when not moving forward or backward")] float strafeSpeedMultiplier = 0.77f;
        [SerializeField, Range(0f, 1f), Tooltip("Multiplier to left/right input when moving forward or backward")] float strafeSpeedMultiplierWhileMoving = 0.77f;
        [SerializeField, Range(0f, 1f), Tooltip("Multiplier to backward input")] float backwardSpeedMultiplier = 0.5f;

        [SerializeField, Tooltip("Jump force")] float jumpSpeed = 6f;
        bool wantsToJump;
        [SerializeField, Range(0f, 1f), Tooltip("Our current speed is multiplied by this when jumping")] float speedLostOnJump = 1f;
        [SerializeField, Range(0f, 1f), Tooltip("Our current speed is multiplied by this when landing")] float speedLostOnLanding = 1f;
        [SerializeField, Tooltip("Fall velocity must be lower than this to trigger OnLanded, which includes the land camera spring and speedLostOnLanding")] float velocityThresholdForLandEffects = -5f;
        [SerializeField, Tooltip("Enter a temporary shock state when landing with a y velocity greater than this value")] float landingShockSpeedThreshold = 100f;

        public float LandingShockSpeedTheshold => landingShockSpeedThreshold;

        /// <summary>
        /// Default run speed, or sprint speed if sprinting is enabled
        /// </summary>
        public float DefaultMoveSpeed => moveSpeed;

        [SerializeField, Min(0f), Tooltip("How fast we accelerate to our target speed")] float speedAccelerationGrounded = 5f;
        [SerializeField, Min(0f), Tooltip("How fast we accelerate to our target speed")] float speedAccelerationInAir = 5f;
        [SerializeField, Min(0f), Tooltip("How fast we decelerate to our target speed")] float speedDeccelerationGrounded = 13f;
        [SerializeField, Min(0f), Tooltip("How fast we decelerate to our target speed")] float speedDeccelerationInAir = 0.5f;
        [SerializeField, Min(0f), Tooltip("How fast we accelerate to our target direction")] float directionAccelerationGrounded = 5f;
        [SerializeField, Min(0f), Tooltip("How fast we accelerate to our target direction")] float directionAccelerationInAir = 3f;
        [SerializeField, Min(0f), Tooltip("How fast we decelerate to our target direction")] float directionDeccelerationGrounded = 123f;
        [SerializeField, Min(0f), Tooltip("How fast we decelerate to our target direction")] float directionDeccelerationInAir = 0f;
        [SerializeField, Min(0f), Tooltip("How fast we decelerate to a target speed that's below our current speed." +
            "\n\nThis can be used to let us keep high speeds for longer.")] float accelerationWhenSlowing = 1f;
        [SerializeField, Tooltip("Curve used to accelerate our move speed when we begin to run." +
            "\n\nDefaults to a stepped curve to simulate speeding up with each foot step.")] AnimationCurve runSpeedAccelerationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField, Min(0f), Tooltip("How long does it take to accelerate to our target move speed when we begin to run?")] float runSpeedAccelerationDuration = 0.4f;

        [SerializeField, Tooltip("Which layers can we walk on?")] LayerMask groundLayerMask = -1;
        [SerializeField, Tooltip("Size of the area around our feet to check for ground.")] Vector3 groundCheckExtents = new Vector3(0.25f, 0.18f, 0.25f);
        [SerializeField, Tooltip("Size of the area around our feet to check for ground." +
            "\n\nThis value is used to check for ground while we're airborne. " +
            "It should be kept small so that we don't immediately detect ground when jumping on a slope.")]
        Vector3 airborneGroundCheckExtents = new Vector3(0.01f, 0.18f, 0.01f);
        [SerializeField, Range(0f, 1f), Tooltip("The normal of the surface we're standing on must be >= this to count as ground")] float groundUpDirectionThreshold = 0.7f;
        [SerializeField, Min(0f), Tooltip("Downward force applied when moving along a sloped surface." +
            "\n\nThis helps keep us from flying off slopes when running downhill.")] float downwardForceAlongSlope = 8f;
        float halfHeight;
        RaycastHit groundHit;

        /// <summary>
        /// Current rigidbody we're standing on
        /// </summary>
        public Rigidbody ConnectedRigidbody { get; private set; }

        [SerializeField, Tooltip("On which layers can the player ride rigidbodies?")] LayerMask movingPlatformLayer = 0;
        readonly List<int> movingPlatformLayers = new List<int>();
        [SerializeField, Min(0f), Tooltip("Drag applied to velocity from a connected rigidbody after leaving it")] float connectedRigidbodyGroundDrag = 4f;
        [SerializeField, Min(0f), Tooltip("Drag applied to velocity from a connected rigidbody after leaving it")] float connectedRigidbodyAirDrag = 0.25f;
        [SerializeField, Min(0f), Tooltip("Drag applied to angular velocity from a connected rigidbody after leaving it")] float connectedRigidbodyAngularGroundDrag = 6f;
        [SerializeField, Min(0f), Tooltip("Drag applied to angular velocity from a connected rigidbody after leaving it")] float connectedRigidbodyAngularAirDrag = 0.5f;
        [SerializeField, Min(0f), Tooltip("Should connected rigidbodies interpolate their position?" +
            "\n\nIf enabled, their interpolation mode will be reset to its previous value when no longer standing on them." +
            "\n\nNon-interpolated rigidbodies will look very jittery while standiing on them.")]
        bool interpolateConnectedRigidbody = true;
        [SerializeField, Tooltip("Should we rotate on the y axis along with the rigidbody we're standing on?")] bool rotateWithConnectedRigidbody = true;
        RigidbodyInterpolation connectedRigidbodyInterpolation;
        Vector3 connectedRigidbodyVelocity;
        Vector3 connectedRigidbodyAngularVelocity;

        [SerializeField, Tooltip("How much speed is added when we start sliding or land after sliding")] float slideSpeed = 7f;
        Vector3 slideDirection;
        float initialSlideSpeed;
        [SerializeField, Min(0f)] float crouchSpeed = 5f;
        [SerializeField, Min(0f), Tooltip("How long do we keep our initial slide speed before beginning to slow")] float fullSlideDuration = 1f;
        [SerializeField, Min(0f), Tooltip("Minimum interval between adding slideSpeed to our current speed when we start sliding." +
            "\n\nThis can be used to stop the player from gaining infinite speed by spamming slide.")] float slideSpeedIncreaseCooldown = 0.5f;
        float lastSlideSpeedIncreaseTime;
        [SerializeField, Tooltip("Added gravity multiplier when sliding while airborne")] float gravityIncreaseWhileSlidingInAir = 1f;

        [SerializeField, Min(0f), Tooltip("How fast we decelerate to crouch speed")] float slideSpeedDeccelerationGrounded = 1f;
        [SerializeField, Min(0f), Tooltip("How fast we decelerate to crouch speed")] float slideSpeedDeccelerationInAir = 0.2f;
        [SerializeField, Min(0f), Tooltip("How fast we decelerate to our target direction while crouching")] float crouchDirectionAcceleration = 4f;
        [SerializeField, Range(0f, 1f), Tooltip("Multiplier to left/right input while sliding")] float slideStrafeMultiplier = 0.8f;
        [SerializeField, Range(0f, 1f), Tooltip("How much control do we have over our movement direction while sliding?")] float slideInputControl = 0.3f;
        [SerializeField, Min(0f), Tooltip("Collider height while sliding.\n\nShrink the collider to allow the player to slide through shorter gaps.")] float slideColliderHeight = 0.75f;

        /// <summary>
        /// Time at which the last slide was started
        /// </summary>
        public float SlideStartTime { get; private set; } = float.MinValue;

        /// <summary>
        /// Have we touched the ground yet during our current slide?
        /// </summary>
        public bool HasTouchedGroundDuringSlide { get; private set; }

        [SerializeField, Min(0f), Tooltip("How far away from a wall can we begin to clamber?")] float clamberCheckDistance = 1;
        [SerializeField] LayerMask clamberLayerMask = -1;
        [SerializeField, Min(0f), Tooltip("Width of the area to check when making sure the top of the clamberable wall is clear." +
            "\n\nThis value should be close to the player's width.")] float clamberCheckWidth = 0.6f;
        [SerializeField, Min(0f), Tooltip("Our distance from the top of a wall must be greater than this to clamber up it")] float minClamberHeight = 0.1f;
        [SerializeField, Min(0f), Tooltip("Our distance from the top of a wall must be less than this to clamber up it")] float maxClamberHeight = 2.5f;
        [SerializeField, Range(0f, 1f), Tooltip("A wall's tilt away from vertical must be less than this to be clamberable." +
            "\n\n0 is vertical and 1 is 90 degrees away from vertical.")] float maxClamberWallTilt = 0.1f;
        [SerializeField, Range(0f, 1f), Tooltip("The tilt of the surface we're clambering to must be less than this to be clamberable" +
            "\n\n0 is flat and 1 is 90 degrees away from flat.")] float maxClamberFloorTilt = 0.7f;
        [SerializeField, Min(0f), Tooltip("How fast we clamber in meters per second")] float clamberSpeed = 5f;
        [SerializeField, Min(0f), Tooltip("The minimum time it takes to clamber")] float minClamberDuration = 0.5f;
        [SerializeField, Min(0f), Tooltip("The maximum time it takes to clamber")] float maxClamberDuration = 3f;
        [SerializeField, Tooltip("The rate at which we clamber up a wall")] AnimationCurve clamberCurveVertical = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField, Tooltip("The rate at which we move toward to wall and then over the top of the wall")] AnimationCurve clamberCurveHorizontal = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField, Tooltip("How far we move over the top of the wall")] float clamberExtraHorizontalDistance = 1f;
        [SerializeField, Tooltip("How much to rotate the camera up when clambering")] float clamberCameraAngleIntensity = 25f;
        [SerializeField, Tooltip("How much the camera is rotated up using clamberCameraAngleIntensity over the duration of a clamber")] AnimationCurve clamberCamerAngleCurve;

        [SerializeField, Min(0f), Tooltip("Target speed when wallrunning")] float wallrunSpeed = 14f;
        [SerializeField, Min(0f), Tooltip("How fast we accelerate to our target speed when wallrunning")] float wallrunSpeedAcceleration = 2f;
        [SerializeField, Min(0f), Tooltip("Maximum wallrun duration")] float wallrunDuration = 1.5f;
        [SerializeField, Min(0f), Tooltip("Our y speed is pushed toward this value while wallrunning")] float wallrunYSpeedIntensity = 3f;
        [SerializeField, Tooltip("wallrunYSpeedIntensity is multiplied by this over the duration of a wallrun")] AnimationCurve wallrunYSpeedCurve = AnimationCurve.Linear(1f, 1f, 0f, 0f);
        [SerializeField, Min(0f), Tooltip("How fast our y speed is pushed toward wallrunYSpeedIntensity while wallrunning")] float wallrunYSpeedDamp = 9f;
        [SerializeField, Tooltip("Added move speed boost when we jump out of a wallrun")] float wallrunJumpSpeedBoost = 5.5f;
        [SerializeField, Tooltip("Added vertical speed when we jump out of a wallrun")] float wallrunJumpSpeedVertical = 4f;
        [SerializeField, Range(0f, 1f), Tooltip("How much are we pushed away from the wall when we jump off of it?" +
            "\n\nAt 0, all force is applied in the direction we're moving. At 1, all force is appplied away from the wall.")] float wallrunJumpForceAwayFromWall = 0.5f;

        [SerializeField, Min(0f), Tooltip("How far away from a wall can we start wallrunning?")] float wallrunCheckDistance = 1f;
        [SerializeField, Tooltip("On which layers can we wallrun?")] LayerMask wallrunLayerMask = -1;
        [SerializeField, Range(0f, 1f), Tooltip("A wall's tilt away from vertical must be less than this in order to be valid" +
            "\n\n0 is vertical and 1 is 90 degrees away from vertical.")] float maxWallrunWallTilt = 0.25f;
        [SerializeField, Range(0f, 1f), Tooltip("How far away from the wall to we need to look to start wallrunning?" +
            "\n\nAt 0, we must be looking at least 90 degrees away from the wall. At 1 we can be looking directly at the wall.")] float lookDirectionThresholdToStartWallrun = 0.75f;
        [SerializeField, Range(0f, 1f), Tooltip("How far away from the wall to we try to look while wallrunning." +
            "\n\nThe camera's rotation is pushed toward this angle while wallrunning." +
            "\n\n0 is 90 degrees away from the wall. 1 is 180 degreees away from the wall")] float targetLookAngleWhileWallrunning = 0.25f;
        [SerializeField, Min(0f), Tooltip("How fast do we rotate toward targetLookAngleWhileWallrunning?")] float wallrunLookAwayFromWallSpeed = 1f;

        [SerializeField, Min(0f), Tooltip("Minimum duration we have to be airborne before being able to wallrun")] float minTimeInAirBeforeWallrun = 0.1f;
        [SerializeField, Min(0f), Tooltip("Minimum time before we can wallrun on our last touched wall again." +
            "\n\nThis stops the player from jumping off a wall and immediately wallrunning on it again.")] float wallrunWallDisableCooldown = 2f;
        float wallrunStartTime;
        float wallrunTargetCameraRotation;

        /// <summary>
        /// 0-1 value for how long we've been wallrunning
        /// </summary>
        public float NormalizedWallrunTime => (Time.time - wallrunStartTime) / wallrunDuration;

        /// <summary>
        /// Which side we're wallrunning on. 1 if the wall is to our right, -1 if it's to our left.
        /// </summary>
        public int WallrunWallSide { get; private set; }

        static readonly int HashRunning = Animator.StringToHash("Running");
        static readonly int HashRunSpeed = Animator.StringToHash("RunSpeed");
        static readonly int HashJump = Animator.StringToHash("Jump");
        static readonly int HashGrounded = Animator.StringToHash("Grounded");
        static readonly int HashSliding = Animator.StringToHash("Sliding");
        static readonly int HashClamber = Animator.StringToHash("Clamber");
        static readonly int HashWallrunning = Animator.StringToHash("Wallrunning");

        /// <summary>
        /// Are we currently moing?
        /// </summary>
        public bool IsRunning { get; private set; }
        float runStartTime;

        /// <summary>
        /// Only valid when EnableSeparateSprint is enabled
        /// </summary>
        public bool IsSprinting { get; private set; }

        /// <summary>
        /// Are we currently touching the ground?
        /// </summary>
        public bool Grounded { get; private set; }

        /// <summary>
        /// Set to Time.time whenever we are grounded
        /// </summary>
        public float LastGroundedTime { get; private set; }

        /// <summary>
        /// Are we currently sliding?
        /// </summary>
        public bool Sliding { get; private set; }

        /// <summary>
        /// Are we currently wallrunning?
        /// </summary>
        public bool Wallrunning { get; private set; }

        /// <summary>
        /// Callback when we start moving
        /// </summary>
        public event System.Action OnStartRunning;
        /// <summary>
        /// Callback when we stop moving
        /// </summary>
        public event System.Action OnStopRunning;
        /// <summary>
        /// Callback when we start sprinting. Only valid if EnableSeparateSprint is enabled.
        /// </summary>
        public event System.Action OnStartSprinting;
        /// <summary>
        /// Callback when we stop sprinting. Only valid if EnableSeparateSprint is enabled.
        /// </summary>
        public event System.Action OnStopSprinting;
        /// <summary>
        /// Callback when we jump
        /// </summary>
        public event System.Action OnJump;

        /// <summary>
        /// Like OnEnteredGround, but only invoked if y velocity is negative
        /// </summary>
        public event System.Action OnLanded;
        /// <summary>
        /// Callback when we touch the ground after being airborne
        /// </summary>
        public event System.Action OnEnteredGround;
        /// <summary>
        /// Callback when we leave the ground
        /// </summary>
        public event System.Action OnLeftGround;
        /// <summary>
        /// Callback when we start sliding
        /// </summary>
        public event System.Action OnStartSlide;
        /// <summary>
        /// Callback when we stop sliding
        /// </summary>
        public event System.Action OnEndSlide;
        /// <summary>
        /// Callback when we start wallrunning
        /// </summary>
        public event System.Action OnStartWallrun;
        /// <summary>
        /// Callback when we stop wallrunning
        /// </summary>
        public event System.Action OnEndWallrun;
        /// <summary>
        /// Callback when we clamber
        /// </summary>
        public event System.Action OnStartClamber;
        /// <summary>
        /// Callback when we finish clambering
        /// </summary>
        public event System.Action OnEndClamber;

        int _movementLocks;
        /// <summary>
        /// Increment/decrement this to lock/unlock movement.
        /// <para>When movement is locked we can't move and no forces are applied.</para>
        /// </summary>
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
        /// <summary>
        /// Increment/decrement this to lock/unlock movement control.
        /// <para>When movement is locked forces are still applied but move input, sliding, clambering, and wallrunning are ignored.</para>
        /// </summary>
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
            Input = GetComponent<PlayerInput>();

            halfHeight = moveController.height * 0.5f;

            // Extract individual layers from moving platform layer mask
            if (movingPlatformLayer > 0)
            {
                for (int i = 0; i < 32; i++)
                {
                    if ((movingPlatformLayer & (1 << i)) != 0)
                    {
                        movingPlatformLayers.Add(i);
                    }
                }
            }

            groundLayerMask |= movingPlatformLayer;
        }

        void OnEnable()
        {
            if (EnableSeparateSprint)
            {
                OnStartSprinting += PlayRunAnimation;
                OnStopSprinting += StopRunAnimation;
            }
            else
            {
                OnStartRunning += PlayRunAnimation;
                OnStopRunning += StopRunAnimation;
            }
            OnEnteredGround += EnableAnimatorGrounded;
            OnEnteredGround += ClearDisabledWallrunColliders;
            OnLanded += EnableAnimatorLanded;
            OnLeftGround += DisableAnimatorGrounded;
            OnStartSlide += EnableAnimatorSlide;
            OnEndSlide += DisableAnimatorSlide;
            OnStartWallrun += EnableAnimatorWallrunning;
            OnEndWallrun += DisableAnimatorWallrunning;
        }

        void OnDisable()
        {
            if (EnableSeparateSprint)
            {
                OnStartSprinting -= PlayRunAnimation;
                OnStopSprinting -= StopRunAnimation;
            }
            else
            {
                OnStartRunning -= PlayRunAnimation;
                OnStopRunning -= StopRunAnimation;
            }
            OnEnteredGround -= EnableAnimatorGrounded;
            OnEnteredGround -= ClearDisabledWallrunColliders;
            OnLanded -= EnableAnimatorLanded;
            OnLeftGround -= DisableAnimatorGrounded;
            OnStartSlide -= EnableAnimatorSlide;
            OnEndSlide -= DisableAnimatorSlide;
            OnStartWallrun -= EnableAnimatorWallrunning;
            OnEndWallrun -= DisableAnimatorWallrunning;
        }

        /// <summary>
        /// Set the player's local position
        /// </summary>
        public void MovePosition(Vector3 position)
        {
            transform.localPosition = position;
            iPositionNow = position;
            iPositionNext = position;
            Physics.SyncTransforms();
        }

        /// <summary>
        /// Reset the player's velocity
        /// </summary>
        public void ResetVelocity()
        {
            combinedVelocity = Vector3.zero;
            moveVelocity = Vector3.zero;
            fallVelocity = Vector3.zero;
            externalVelocity = Vector3.zero;
            physicsVelocity = Vector3.zero;
            connectedRigidbodyVelocity = Vector3.zero;
            connectedRigidbodyAngularVelocity = Vector3.zero;
            currentMoveDirection = Vector3.zero;
            currentMoveSpeed = 0f;
        }

        public bool StartSlide()
        {
            if (Sliding)
            {
                return false;
            }

            Sliding = true;
            SlideStartTime = Time.time;
            slideDirection = currentMoveDirection != Vector3.zero ? currentMoveDirection.normalized : transform.forward;
            moveController.height = slideColliderHeight;
            moveController.center = new Vector3(0f, slideColliderHeight * -0.5f, 0f);

            // Only apply slide speed increase if we're grounded
            HasTouchedGroundDuringSlide = Grounded;
            if (HasTouchedGroundDuringSlide)
            {
                if (Time.time - lastSlideSpeedIncreaseTime > slideSpeedIncreaseCooldown)
                {
                    float speed = moveVelocity.magnitude;
                    if (speed <= crouchSpeed)
                    {
                        SlideStartTime -= fullSlideDuration + 0.5f;
                    }
                    else
                    {
                        initialSlideSpeed = speed + slideSpeed;
                        currentMoveSpeed = initialSlideSpeed;
                        lastSlideSpeedIncreaseTime = Time.time;
                    }
                }
            }
            else
            {
                initialSlideSpeed = moveVelocity.magnitude;
            }

            OnStartSlide?.Invoke();

            return true;
        }

        public bool EndSlide()
        {
            if (!Sliding)
            {
                return false;
            }

            Sliding = false;
            moveController.height = halfHeight * 2f;
            moveController.center = Vector3.zero;
            OnEndSlide?.Invoke();

            return true;
        }

        void GetTargetVelocityAndAcceleration(Vector2 moveInput, bool wasRunning, out Vector3 moveDirection, out float targetSpeed, out float speedAcceleration, out float directionAcceleration)
        {
            if (Sliding)
            {
                bool inFullSlide = Time.time - SlideStartTime < fullSlideDuration;

                if (HasTouchedGroundDuringSlide)
                {
                    targetSpeed = inFullSlide ? initialSlideSpeed : crouchSpeed;
                }
                else
                {
                    // Apply slide speed increase if we started sliding in the air and are now touching the ground
                    if (Grounded)
                    {
                        HasTouchedGroundDuringSlide = true;
                        
                        if (Time.time - lastSlideSpeedIncreaseTime > slideSpeedIncreaseCooldown)
                        {
                            initialSlideSpeed = moveVelocity.magnitude + slideSpeed;
                            currentMoveSpeed = initialSlideSpeed;
                            lastSlideSpeedIncreaseTime = Time.time;
                        }

                        targetSpeed = initialSlideSpeed;
                    }
                    else
                    {
                        targetSpeed = 0f;
                    }
                }

                if (currentMoveSpeed - crouchSpeed < 1f)
                {
                    // Move only using input direction if we're at crouching speed
                    moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

                    if (moveInput == Vector2.zero)
                    {
                        targetSpeed = 0f;

                        if (Grounded)
                        {
                            speedAcceleration = speedAccelerationGrounded;
                            directionAcceleration = crouchDirectionAcceleration;
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
                            directionAcceleration = crouchDirectionAcceleration;
                        }
                        else
                        {
                            speedAcceleration = speedDeccelerationInAir;
                            directionAcceleration = directionDeccelerationInAir;
                        }
                    }
                }
                else
                {
                    // Slide in the direction we started sliding in, with some amount of input control
                    moveInput.x *= slideStrafeMultiplier;
                    Vector3 inputMoveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
                    moveDirection = Vector3.LerpUnclamped(slideDirection, inputMoveDirection, slideInputControl);

                    if (Grounded)
                    {
                        speedAcceleration = slideSpeedDeccelerationGrounded;
                        directionAcceleration = directionAccelerationGrounded;
                    }
                    else
                    {
                        speedAcceleration = slideSpeedDeccelerationInAir;
                        directionAcceleration = directionAccelerationInAir;
                    }
                }

                return;
            }
            else if (Wallrunning)
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
                        if (Vector3.Dot(cameraDirection, hit.normal) < targetLookAngleWhileWallrunning)
                        {
                            wallrunTargetCameraRotation = Quaternion.LookRotation(Vector3.LerpUnclamped(moveDirection, hit.normal, targetLookAngleWhileWallrunning), playerLook.transform.up).eulerAngles.y;
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

            bool wasSprinting = IsSprinting;
            IsSprinting = IsRunning && Input.Controls.Player.Sprint.IsPressed();

            if (IsSprinting && !wasSprinting)
            {
                OnStartSprinting?.Invoke();
            }
            else if (!IsSprinting && wasSprinting)
            {
                OnStopSprinting?.Invoke();
            }

            if (IsRunning)
            {
                if (!wasRunning)
                {
                    runStartTime = Time.time;
                    currentMoveSpeed = Mathf.Max(currentMoveSpeed, initialMoveSpeed);
                }
                else if (EnableSeparateSprint && IsSprinting && !wasSprinting)
                {
                    runStartTime = Time.time;
                    currentMoveSpeed = Mathf.Max(currentMoveSpeed, initialMoveSpeed);
                }

                float runDuration = Time.time - runStartTime;
                float speed = !EnableSeparateSprint || IsSprinting ? moveSpeed : walkSpeed;
                if (runDuration <= runSpeedAccelerationDuration)
                {
                    targetSpeed = Mathf.LerpUnclamped(initialMoveSpeed, speed, runSpeedAccelerationCurve.Evaluate(runDuration / runSpeedAccelerationDuration));
                }
                else
                {
                    targetSpeed = speed;
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

        Vector3 controllerGroundNormal;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            controllerGroundNormal = hit.normal;
        }

        void Movement()
        {
            iPositionNow = transform.localPosition;

            bool wasRunning = IsRunning;

            // Box cast at our feet to check if we're grounded
            bool groundCheck = Physics.BoxCast(transform.localPosition, Grounded ? groundCheckExtents : airborneGroundCheckExtents, Vector3.down, out groundHit, Quaternion.identity, halfHeight, groundLayerMask, QueryTriggerInteraction.Ignore);
            bool wasGrounded = Grounded;

            // Slide down steep slopes
            bool controllerIsGrounded = moveController.isGrounded;
            if (groundCheck || controllerIsGrounded)
            {
                if (Vector3.Dot(controllerGroundNormal, Vector3.up) < groundUpDirectionThreshold && Vector3.Dot(groundHit.normal, Vector3.up) < groundUpDirectionThreshold)
                {
                    controllerIsGrounded = false;
                    groundCheck = false;

                    fallVelocity.y += Physics.gravity.y * (Sliding ? gravityMultiplier + gravityIncreaseWhileSlidingInAir : gravityMultiplier) * Time.deltaTime;
                    moveController.Move(Vector3.ProjectOnPlane(Vector3.up, controllerGroundNormal).NormalizedFast() * (fallVelocity.y * Time.deltaTime));

                    controllerGroundNormal = Vector3.up;
                }
            }

            // We don't want to set Grounded to true if our y velocity is positive - otherwise we can't jump when running uphill
            Grounded = (groundCheck || controllerIsGrounded) && fallVelocity.y <= 0f;
            
            if (Grounded && !wasGrounded)
            {
                OnEnteredGround?.Invoke();

                if (fallVelocity.y < velocityThresholdForLandEffects)
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

            // Get velocity from connected rigidbody
            if (Grounded && groundHit.rigidbody != null)
            {
                // Set rigidbody interpolation modes (Rigidbodies we're standing on should interpolate so they don't look jittery)
                if (ConnectedRigidbody != groundHit.rigidbody)
                {
                    if (ConnectedRigidbody != null)
                    {
                        ConnectedRigidbody.interpolation = connectedRigidbodyInterpolation;
                    }

                    ConnectedRigidbody = groundHit.rigidbody;
                    connectedRigidbodyInterpolation = ConnectedRigidbody.interpolation;
                    ConnectedRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                }

                connectedRigidbodyVelocity = ConnectedRigidbody.GetPointVelocity(groundHit.point);
                connectedRigidbodyAngularVelocity = ConnectedRigidbody.angularVelocity;
            }
            else if (ConnectedRigidbody != null)
            {
                // Apply drag to connected rigidbody velocity
                if (Grounded)
                {
                    connectedRigidbodyVelocity *= 1f - connectedRigidbodyGroundDrag * Time.fixedDeltaTime;
                    connectedRigidbodyAngularVelocity *= 1f - connectedRigidbodyAngularGroundDrag * Time.fixedDeltaTime;
                }
                else
                {
                    connectedRigidbodyVelocity *= 1f - connectedRigidbodyAirDrag * Time.fixedDeltaTime;
                    connectedRigidbodyAngularVelocity *= 1f - connectedRigidbodyAngularAirDrag * Time.fixedDeltaTime;
                }

                if (connectedRigidbodyVelocity.sqrMagnitude < 0.01f && Mathf.Abs(connectedRigidbodyAngularVelocity.y) < 0.03f)
                {
                    ConnectedRigidbody = null;
                }
            }
            else
            {
                connectedRigidbodyVelocity = Vector3.zero;
                connectedRigidbodyAngularVelocity = Vector3.zero;
            }

            // Get move direction
            Vector2 moveInput = MovementControlLocks == 0 ? Input.Controls.Player.Move.ReadValue<Vector2>() : Vector2.zero;
            moveInput.x *= moveInput.y == 0f ? strafeSpeedMultiplier : strafeSpeedMultiplierWhileMoving;
            if (moveInput.y < 0f)
            {
                moveInput.y *= backwardSpeedMultiplier;
            }

            CheckForWallrun(moveInput);

            GetTargetVelocityAndAcceleration(moveInput, wasRunning, out Vector3 moveDirection, out float targetSpeed, out float speedAcceleration, out float directionAcceleration);

            // Use accelerationWhenSlowing to decelerate if we're moving faster than our target speed
            if (currentMoveSpeed > targetSpeed && targetSpeed != 0f)
            {
                speedAcceleration = accelerationWhenSlowing;
            }

            // Accelerate move speed
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
                LastGroundedTime = Time.time;
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
                fallVelocity.y += Physics.gravity.y * (Sliding ? gravityMultiplier + gravityIncreaseWhileSlidingInAir : gravityMultiplier) * Time.deltaTime;
            }

            fallVelocity.y += physicsVelocity.y;
            physicsVelocity.y = 0f;

            // Combine velocities and move
            combinedVelocity = moveVelocity + fallVelocity + externalVelocity + physicsVelocity + connectedRigidbodyVelocity;
            Vector3 moveDelta = combinedVelocity;

            // Project our velocity onto the ground so we don't fly into the air when running downhill
            if (Grounded && Vector3.Dot(groundHit.normal, Vector3.up) != 1f)
            {
                //currentMoveDirection = Vector3.ProjectOnPlane(currentMoveDirection, Vector3.ProjectOnPlane(transform.forward, groundHit.normal)).normalized;
                moveDelta.y -= downwardForceAlongSlope;
            }

            // Moving platforms shouldn't be allowed to collide with us since we're not a rigidbody, but we need to collide with them only while we move
            int layer = gameObject.layer;
            for (int i = 0; i < movingPlatformLayers.Count; i++)
            {
                Physics.IgnoreLayerCollision(layer, movingPlatformLayers[i], false);
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
                if (connectedRigidbodyVelocity.y > 0f)
                {
                    connectedRigidbodyVelocity.y = 0f;
                }
            }

            for (int i = 0; i < movingPlatformLayers.Count; i++)
            {
                Physics.IgnoreLayerCollision(layer, movingPlatformLayers[i], true);
            }

            // Apply drag
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

            if (rotateWithConnectedRigidbody)
            {
                // Apply rotation from connected rigidbody
                transform.Rotate(0f, connectedRigidbodyAngularVelocity.y * Mathf.Rad2Deg * Time.deltaTime, 0f);
            }

            if (EnableSlide && !Sliding && !Wallrunning && MovementLocks == 0 && MovementControlLocks == 0 && Input.Controls.Player.Slide.WasPressedThisFrame())
            {
                StartSlide();
            }

            if (Sliding)
            {
                if (MovementControlLocks == 0 && !Input.Controls.Player.Slide.IsPressed())
                {
                    EndSlide();
                }
            }
            else if (Wallrunning)
            {
                if (!float.IsNaN(wallrunTargetCameraRotation))
                {
                    transform.localEulerAngles = new Vector3(0f, Mathf.LerpAngle(transform.localEulerAngles.y, wallrunTargetCameraRotation,
                        wallrunLookAwayFromWallSpeed * Time.deltaTime), 0f);
                }
            }

            if (Input.Controls.Player.Jump.WasPressedThisFrame())
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

            wantsToJump = false;
        }

        void CheckForClamberableSurface()
        {
            if (!EnableClamber || Grounded || !IsRunning || Sliding || MovementControlLocks > 0)
            {
                return;
            }

            // Only clamber if we're holding forward
            if (Input.Controls.Player.Move.ReadValue<Vector2>().y <= 0f)
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
                        if (floorPos.y - playerPosition.y >= minClamberHeight && floorHit.transform == hit.transform && Vector3.Dot(floorHit.normal, Vector3.up) > (1f - maxClamberFloorTilt))
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

                            StartCoroutine(ClamberCoroutine(targetPos, hit.normal));
                        }
                    }
                }
            }
        }

        IEnumerator ClamberCoroutine(Vector3 targetPos, Vector3 wallNormal)
        {
            MovementLocks++;
            OnStartClamber?.Invoke();

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

                playerLook.AddRotation(new Vector3(clamberCamerAngleCurve.Evaluate(normalizedTime) * clamberCameraAngleIntensity, 0f, 0f));

                Vector3 directionTowardLedge = targetPos - playerLook.transform.position;
                Vector3 targetRotation = Quaternion.LookRotation(directionTowardLedge).eulerAngles;
                playerLook.ConstrainRotationY(targetRotation.y);

                yield return null;
            }

            currentMoveDirection = Vector3.LerpUnclamped(-wallNormal, transform.forward, 0.5f);
            currentMoveDirection.y = 0f;
            fallVelocity.y = 0f;

            MovementLocks--;
            OnEndClamber?.Invoke();
        }

        Collider lastTouchedWallrunCollider;
        float lastTouchedWallrunColliderTime;

        void CheckForWallrun(Vector2 moveInput)
        {
            if (!EnableWallrun || Grounded || Wallrunning || Sliding || MovementControlLocks > 0)
            {
                return;
            }

            if (moveInput.y == 0f)
            {
                return;
            }

            if (Time.time - LastGroundedTime < minTimeInAirBeforeWallrun)
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
            if (CheckForRunnableWalls(ref forward, true, out RaycastHit hit, out _) && Vector3.Dot(forward, hit.normal) > -lookDirectionThresholdToStartWallrun)
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

            animator.SetInteger(HashGrounded, 0);
            PlayJumpAnimation();
        }

        void EnableAnimatorGrounded()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetInteger(HashGrounded, 1);
        }

        void EnableAnimatorLanded()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetInteger(HashGrounded, 2);
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

#if UNITY_EDITOR
        [CustomEditor(typeof(PlayerMovement))]
        class PlayerMovementEditor : Editor
        {
            PlayerMovement script;

            static bool foldoutMovement;
            static bool foldoutSliding;
            static bool foldoutClamber;
            static bool foldoutWallrun;

            class CameraSetupEditorWindow : EditorWindow
            {
                static int firstPersonLayer;

                void OnGUI()
                {
                    if (!Selection.activeGameObject.TryGetComponent(out PlayerMovement script))
                    {
                        EditorGUILayout.HelpBox("Select a PlayerMovement object", MessageType.Error);
                        return;
                    }

                    if (!script.playerLook)
                    {
                        EditorGUILayout.HelpBox("Assign playerLook", MessageType.Error);
                        return;
                    }

                    if (!script.playerLook.Camera)
                    {
                        EditorGUILayout.HelpBox("Assign playerLook.Camera", MessageType.Error);
                        return;
                    }

                    if (!script.playerLook.FirstPersonCamera)
                    {
                        EditorGUILayout.HelpBox("Assign playerLook.FirstPersonCamera", MessageType.Error);
                        return;
                    }

                    EditorGUILayout.HelpBox("Select a first person layer. This layer will only be rendered by the first person camera at a fixed FOV." +
                        "This is used for rendering objects like arms and held items that we want to look the same if the player's FOV changes.", MessageType.Info);

                    EditorGUILayout.Space(10);
                    
                    firstPersonLayer = EditorGUILayout.LayerField("First Person Layer", firstPersonLayer);

                    EditorGUILayout.Space(10);

                    if (GUILayout.Button("Apply Layer Mask"))
                    {
                        script.playerLook.Camera.cullingMask &= ~(1 << firstPersonLayer);
                        script.playerLook.FirstPersonCamera.cullingMask = (1 << firstPersonLayer);
                        EditorUtility.SetDirty(script.playerLook.Camera);
                        EditorUtility.SetDirty(script.playerLook.FirstPersonCamera);

                        Close();
                    }
                }
            }

            void OnEnable()
            {
                script = (PlayerMovement)target;
            }

            /// BUTTON TO SETUP OCCLUSION MASKS ON CAMERAS

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.moveController)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.playerLook)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.interpolatedTransform)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.animator)));

                EditorGUILayout.Space(10);

                if (GUILayout.Button("Setup Cameras"))
                {
                    EditorWindow.GetWindow<CameraSetupEditorWindow>("Camera Setup");
                }

                EditorGUILayout.Space(10);

                foldoutMovement = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutMovement, "Movement");
                if (foldoutMovement)
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.gravityMultiplier)));

                    EditorGUILayout.Space(10);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.fallVelocityDrag)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.externalVelocityDragGrounded)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.externalVelocityDragAir)));

                    EditorGUILayout.Space(20);
                    EditorGUILayout.LabelField("Speeds", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.moveSpeed)));

                    var enableSeparateSprint = serializedObject.FindProperty(nameof(script.enableSeparateSprint));
                    EditorGUILayout.PropertyField(enableSeparateSprint);
                    if (enableSeparateSprint.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.walkSpeed)));
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.initialMoveSpeed)));

                    EditorGUILayout.Space(10);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.strafeSpeedMultiplier)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.strafeSpeedMultiplierWhileMoving)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.backwardSpeedMultiplier)));

                    EditorGUILayout.Space(10);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.jumpSpeed)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.speedLostOnJump)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.speedLostOnLanding)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.velocityThresholdForLandEffects)));

                    var enableLandingShock = serializedObject.FindProperty(nameof(script.enableLandingShock));
                    EditorGUILayout.PropertyField(enableLandingShock);
                    if (enableLandingShock.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.landingShockSpeedThreshold)));
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.Space(20);
                    EditorGUILayout.LabelField("Acceleration", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.speedAccelerationGrounded)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.speedAccelerationInAir)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.speedDeccelerationGrounded)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.speedDeccelerationInAir)));

                    EditorGUILayout.Space(10);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.directionAccelerationGrounded)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.directionAccelerationInAir)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.directionDeccelerationGrounded)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.directionDeccelerationInAir)));

                    EditorGUILayout.Space(10);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.accelerationWhenSlowing)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.runSpeedAccelerationCurve)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.runSpeedAccelerationDuration)));

                    EditorGUILayout.Space(10);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.groundLayerMask)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.groundCheckExtents)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.airborneGroundCheckExtents)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.groundUpDirectionThreshold)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.downwardForceAlongSlope)));

                    EditorGUILayout.Space(20);
                    EditorGUILayout.LabelField("Connected Rigidbodies", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.movingPlatformLayer)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.connectedRigidbodyGroundDrag)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.connectedRigidbodyAirDrag)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.connectedRigidbodyAngularGroundDrag)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.connectedRigidbodyAngularAirDrag)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.interpolateConnectedRigidbody)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.rotateWithConnectedRigidbody)));

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(10);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space(10);
                var enableSlide = serializedObject.FindProperty(nameof(script.enableSlide));
                EditorGUILayout.PropertyField(enableSlide);

                if (enableSlide.boolValue)
                {
                    foldoutSliding = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutSliding, "Sliding");
                    if (foldoutSliding)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.enableRoll)));
                        EditorGUILayout.Space(20);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.slideSpeed)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.crouchSpeed)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.fullSlideDuration)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.slideSpeedIncreaseCooldown)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.gravityIncreaseWhileSlidingInAir)));

                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.slideSpeedDeccelerationGrounded)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.slideSpeedDeccelerationInAir)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.crouchDirectionAcceleration)));

                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.slideStrafeMultiplier)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.slideInputControl)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.slideColliderHeight)));

                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space(10);
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                EditorGUILayout.Space(10);
                var enableClamber = serializedObject.FindProperty(nameof(script.enableClamber));
                EditorGUILayout.PropertyField(enableClamber);

                if (enableClamber.boolValue)
                {
                    foldoutClamber = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutClamber, "Clamber");
                    if (foldoutClamber)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.clamberCheckDistance)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.clamberLayerMask)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.clamberCheckWidth)));

                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.minClamberHeight)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.maxClamberHeight)));

                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.maxClamberWallTilt)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.maxClamberFloorTilt)));

                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.clamberSpeed)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.minClamberDuration)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.maxClamberDuration)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.clamberCurveVertical)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.clamberCurveHorizontal)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.clamberExtraHorizontalDistance)));

                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.clamberCameraAngleIntensity)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.clamberCamerAngleCurve)));

                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space(10);
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                EditorGUILayout.Space(10);
                var enableWallrun = serializedObject.FindProperty(nameof(script.enableWallrun));
                EditorGUILayout.PropertyField(enableWallrun);

                if (enableWallrun.boolValue)
                {
                    foldoutWallrun = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutWallrun, "Wallrun");
                    if (foldoutWallrun)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunSpeed)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunSpeedAcceleration)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunDuration)));

                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunYSpeedIntensity)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunYSpeedCurve)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunYSpeedDamp)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunJumpSpeedBoost)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunJumpSpeedVertical)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunJumpForceAwayFromWall)));

                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunCheckDistance)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunLayerMask)));

                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.maxWallrunWallTilt)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.lookDirectionThresholdToStartWallrun)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.targetLookAngleWhileWallrunning)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunLookAwayFromWallSpeed)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.minTimeInAirBeforeWallrun)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.wallrunWallDisableCooldown)));

                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}