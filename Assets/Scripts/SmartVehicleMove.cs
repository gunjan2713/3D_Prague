using UnityEngine;

public class SmartVehicleMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float activationDistance = 50f; // Distance from player to start moving
    public float deactivationDistance = 70f; // Distance to stop moving (slightly larger to prevent flickering)
    
    [Header("Optional Settings")]
    public bool showDebugInfo = false;
    public bool resetPositionWhenFarAway = true;
    public Vector3 startPosition;
    public float resetDistance = 100f; // How far vehicle can go before resetting
    
    private Transform playerTransform;
    private bool isMoving = false;
    private float distanceToPlayer;
    
    void Start()
    {
        // Find the player
        FindPlayer();
        
        // Store starting position for reset functionality
        startPosition = transform.position;
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
        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} started moving - Player nearby!");
        }
    }
    
    void StopMoving()
    {
        isMoving = false;
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
            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} reset to starting position");
            }
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