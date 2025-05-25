using UnityEngine;

public class SmartMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float activationDistance = 50f; // Distance from player to start moving
    public float deactivationDistance = 70f; // Distance to stop moving (slightly larger to prevent flickering)
    
    [Header("Audio Settings")]
    public AudioClip vehicleSound; // Assign police siren, ambulance, car engine, etc.
    [Range(0f, 1f)]
    public float maxVolume = 0.7f;
    public float maxAudioDistance = 100f; // Max distance where sound can be heard
    public float minAudioDistance = 5f; // Distance where sound is at max volume
    public bool playOnlyWhenMoving = true; // If true, sound only plays when vehicle is moving
    
    [Header("Optional Settings")]
    public bool showDebugInfo = false;
    public bool resetPositionWhenFarAway = true;
    public Vector3 startPosition;
    public float resetDistance = 100f; // How far vehicle can go before resetting
    
    private Transform playerTransform;
    private bool isMoving = false;
    private float distanceToPlayer;
    private AudioSource audioSource;
    
    void Start()
    {
        // Find the player
        FindPlayer();
        
        // Store starting position for reset functionality
        startPosition = transform.position;
        
        // Set up audio source
        SetupAudioSource();
    }
    
    void Update()
    {
        if (playerTransform == null)
        {
            // Try to find player again if not found
            FindPlayer();
            return;
        }
        
        // Calculate distance to player
        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Determine if vehicle should be moving
        bool shouldMove = ShouldVehicleMove();
        
        // Start or stop movement based on distance
        if (shouldMove && !isMoving)
        {
            StartMoving();
        }
        else if (!shouldMove && isMoving)
        {
            StopMoving();
        }
        
        // Move the vehicle if it should be moving
        if (isMoving)
        {
            MoveVehicle();
        }
        
        // Update audio based on distance and movement
        UpdateAudio();
        
        // Reset position if vehicle has gone too far
        if (resetPositionWhenFarAway)
        {
            CheckForReset();
        }
        
        // Debug information
        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: Distance to player: {distanceToPlayer:F1}, Moving: {isMoving}");
        }
    }
    
    bool ShouldVehicleMove()
    {
        if (isMoving)
        {
            // If already moving, use larger distance to prevent flickering
            return distanceToPlayer <= deactivationDistance;
        }
        else
        {
            // If not moving, use smaller distance to start
            return distanceToPlayer <= activationDistance;
        }
    }
    
    void StartMoving()
    {
        isMoving = true;
        
        // Start vehicle sound if it should play when moving
        if (playOnlyWhenMoving && audioSource != null && vehicleSound != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} started moving - Player nearby!");
        }
    }
    
    void StopMoving()
    {
        isMoving = false;
        
        // Stop vehicle sound if it only plays when moving
        if (playOnlyWhenMoving && audioSource != null)
        {
            audioSource.Stop();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} stopped moving - Player too far away");
        }
    }
    
    void MoveVehicle()
    {
        // Move forward in local Z direction (same as original script)
        transform.Translate(0, 0, speed * Time.deltaTime);
    }
    
    void CheckForReset()
    {
        float distanceFromStart = Vector3.Distance(transform.position, startPosition);
        
        // If vehicle has traveled too far from start, reset it
        if (distanceFromStart > resetDistance)
        {
            transform.position = startPosition;
            
            // Restart audio if vehicle has audio
            if (audioSource != null && vehicleSound != null)
            {
                if (!playOnlyWhenMoving || isMoving)
                {
                    audioSource.Stop();
                    audioSource.Play();
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} reset to starting position");
            }
        }
    }
    
    void SetupAudioSource()
    {
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source for 3D positional audio
        if (vehicleSound != null)
        {
            audioSource.clip = vehicleSound;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.volume = 0f; // Start at 0, will be updated based on distance
            audioSource.maxDistance = maxAudioDistance;
            audioSource.minDistance = minAudioDistance;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            
            // Start playing if it shouldn't only play when moving
            if (!playOnlyWhenMoving)
            {
                audioSource.Play();
            }
        }
    }
    
    void UpdateAudio()
    {
        if (audioSource == null || vehicleSound == null || playerTransform == null)
            return;
        
        // Calculate volume based on distance
        float normalizedDistance = Mathf.Clamp01((distanceToPlayer - minAudioDistance) / (maxAudioDistance - minAudioDistance));
        float targetVolume = (1f - normalizedDistance) * maxVolume;
        
        // Only play sound if conditions are met
        bool shouldPlayAudio = true;
        
        if (playOnlyWhenMoving && !isMoving)
        {
            shouldPlayAudio = false;
        }
        
        if (distanceToPlayer > maxAudioDistance)
        {
            shouldPlayAudio = false;
        }
        
        // Update audio accordingly
        if (shouldPlayAudio)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            audioSource.volume = targetVolume;
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        
        if (showDebugInfo && audioSource.isPlaying)
        {
            Debug.Log($"{gameObject.name} Audio: Distance: {distanceToPlayer:F1}, Volume: {targetVolume:F2}");
        }
    }
    
    void FindPlayer()
    {
        // Try to find player by tag first
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            return;
        }
        
        // If no player tag, try to find by name
        player = GameObject.Find("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            return;
        }
        
        // Try to find by PlayerController component
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerTransform = playerController.transform;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw activation distance in editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
        
        // Draw deactivation distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, deactivationDistance);
        
        // Draw reset distance
        if (resetPositionWhenFarAway)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPosition, resetDistance);
        }
    }
}