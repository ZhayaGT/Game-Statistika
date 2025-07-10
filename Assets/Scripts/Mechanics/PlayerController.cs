using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        
        // Add for rigged character
        [Header("Rigged Character Settings")]
        [SerializeField] private bool isRiggedCharacter = true;
        [SerializeField] private Transform characterVisual; // Reference to the visual transform that needs to be flipped
        
        // Add Input System variables
        public PlayerInput playerInput;
        public InputAction moveAction;
        public InputAction jumpAction;
        
        // Add variables for mobile touch controls
        [Header("Mobile Controls")]
        [SerializeField] private bool useMobileControls = false;
        [SerializeField] private float mobileMoveInput = 0f;
        
        public void OnMobileMove(float direction)
        {
            mobileMoveInput = direction;
        }
        
        // Add this method to reset movement
        public void StopMoving()
        {
            // Reset mobile input to zero
            mobileMoveInput = 0f;
            // Reset movement vector
            move = Vector2.zero;
            
            // Update animator if needed
            if (animator != null)
            {
                animator.SetFloat("velocityX", 0);
            }
        }
        
        public void OnMobileJump()
        {
            if (controlEnabled && jumpState == JumpState.Grounded)
                jumpState = JumpState.PrepareToJump;
        }
        
        public void OnMobileJumpRelease()
        {
            if (controlEnabled)
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            
            // Get sprite renderer if not using rigged character
            if (!isRiggedCharacter)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }
            
            // Find character visual if not assigned
            if (isRiggedCharacter && characterVisual == null)
            {
                // Try to find the visual transform (usually a child of the player)
                // This might be named "Visual", "Model", "Mesh", etc.
                Transform[] children = GetComponentsInChildren<Transform>();
                foreach (Transform child in children)
                {
                    if (child != transform && 
                        (child.name.Contains("Visual") || 
                         child.name.Contains("Model") || 
                         child.name.Contains("Mesh") ||
                         child.name.Contains("Rig")))
                    {
                        characterVisual = child;
                        break;
                    }
                }
                
                // If still not found, just use the first child
                if (characterVisual == null && transform.childCount > 0)
                {
                    characterVisual = transform.GetChild(0);
                }
            }
            
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            
            // Initialize Input System
            playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                moveAction = playerInput.actions["Move"];
                jumpAction = playerInput.actions["Jump"];
            }
            else
            {
                Debug.LogWarning("PlayerInput component not found on player. Add it to use the new Input System.");
            }
        }
        
        private void OnEnable()
        {
            if (playerInput != null)
            {
                jumpAction.performed += OnJumpPerformed;
                jumpAction.canceled += OnJumpCanceled;
            }
        }
        
        private void OnDisable()
        {
            if (playerInput != null)
            {
                jumpAction.performed -= OnJumpPerformed;
                jumpAction.canceled -= OnJumpCanceled;
            }
        }
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (controlEnabled && jumpState == JumpState.Grounded)
                jumpState = JumpState.PrepareToJump;
        }
        
        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            if (controlEnabled)
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                if (useMobileControls && (Application.isMobilePlatform || DebugAlwaysUseMobile()))
                {
                    move.x = mobileMoveInput;
                    //Debug.Log($"[MOBILE INPUT] move.x: {move.x}, platform: {Application.platform}");
                }
                else if (playerInput != null && playerInput.actions != null)
                {
                    try
                    {
                        Vector2 moveInput = moveAction.ReadValue<Vector2>();
                        move.x = moveInput.x;
                        //Debug.Log($"[INPUT SYSTEM] move.x: {move.x}");
                    }
                    catch (System.Exception e)
                    {
                        //Debug.LogError("Error reading input: " + e.Message);
                        move.x = Input.GetAxis("Horizontal");
                    }
                }
                else
                {
                    move.x = Input.GetAxis("Horizontal");
                    //Debug.Log($"[LEGACY INPUT] move.x: {move.x}");
                }
            }

            UpdateJumpState();
            base.Update();
        }

        private bool DebugAlwaysUseMobile()
        {
        #if UNITY_EDITOR
            return true; // Simulasikan platform mobile saat testing di editor
        #else
            return false;
        #endif
        }


        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            // Handle character direction based on movement
            if (move.x > 0.01f)
            {
                if (isRiggedCharacter && characterVisual != null)
                {
                    // For rigged character, rotate the visual transform
                    characterVisual.localRotation = Quaternion.Euler(0, 0, 0); // Forward direction
                }
                else
                {
                    // For sprite-based character, flip the sprite
                    spriteRenderer.flipX = false;
                }
            }
            else if (move.x < -0.01f)
            {
                if (isRiggedCharacter && characterVisual != null)
                {
                    // For rigged character, rotate the visual transform
                    characterVisual.localRotation = Quaternion.Euler(0, 180, 0); // Backward direction
                }
                else
                {
                    // For sprite-based character, flip the sprite
                    spriteRenderer.flipX = true;
                }
            }

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}