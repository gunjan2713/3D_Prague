using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Audio Settings")]
    public AudioClip[] footstepSounds;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public float footstepInterval = 0.5f;
    [Range(0f, 1f)]
    public float footstepVolume = 0.7f;

    [Header("Camera Reference")]
    public Camera playerCamera; // Assign your camera here, or it will find it automatically

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;
    private AudioSource audioSource;
    private float footstepTimer;

    void Start()
    {
        // Get the CharacterController component
        controller = GetComponent<CharacterController>();

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.volume = footstepVolume;

        // Find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }

        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    void Update()
    {
        // Store previous grounded state
        wasGrounded = isGrounded;

        // Check if player is on ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Play landing sound if just landed
        if (isGrounded && !wasGrounded && landSound != null)
        {
            PlaySound(landSound);
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }

        // Get input from keyboard
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Arrow keys
        float vertical = Input.GetAxis("Vertical");     // W/S or Arrow keys

        // Check if player is actively pressing movement keys
        bool isMovingInput = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

        // Calculate movement direction relative to camera (FPS-style movement)
        Vector3 direction = Vector3.zero;
        if (playerCamera != null)
        {
            // Get camera's forward and right directions (projected on horizontal plane)
            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 cameraRight = playerCamera.transform.right;
            
            // Remove Y component to keep movement on horizontal plane
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            
            // Normalize the vectors
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // Calculate movement direction based on input
            direction = cameraForward * vertical + cameraRight * horizontal;
        }
        else
        {
            // Fallback to world-space movement if no camera found
            direction = new Vector3(horizontal, 0f, vertical);
        }

        // Move the character
        if (direction.magnitude >= 0.1f)
        {
            controller.Move(direction * moveSpeed * Time.deltaTime);

            // Smoothly rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // Handle footstep sounds - only when pressing keys AND grounded
        if (isMovingInput && isGrounded)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayFootstepSound();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // Reset timer when not moving so footstep plays immediately when starting to move
            footstepTimer = 0f;
        }

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Play jump sound
            if (jumpSound != null)
            {
                PlaySound(jumpSound);
            }
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void PlayFootstepSound()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            // Pick a random footstep sound
            AudioClip randomClip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            PlaySound(randomClip);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.volume = footstepVolume;
            audioSource.Play();
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw ground check sphere in scene view
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}