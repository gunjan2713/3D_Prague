using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Assign your player here
    
    [Header("Camera Settings")]
    public float distance = 7f;
    public float height = 5f;
    public float rotationSpeed = 2f;
    public float followSpeed = 10f;
    
    [Header("Mouse Look")]
    public bool useMouseLook = true;
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;
    
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    
    void Start()
    {
        // If no target assigned, try to find player
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        
        // Lock cursor if using mouse look
        if (useMouseLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        // Initialize rotation based on current camera rotation
        Vector3 angles = transform.eulerAngles;
        currentRotationX = angles.x;
        currentRotationY = angles.y;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Handle mouse input
        if (useMouseLook)
        {
            currentRotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentRotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);
        }
        
        // Calculate desired position
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        Vector3 desiredPosition = target.position - (rotation * Vector3.forward * distance);
        desiredPosition.y = target.position.y + height;
        
        // Smooth camera movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        
        // Look at target
        transform.LookAt(target.position + Vector3.up * height * 0.5f);
        
        // Toggle cursor lock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }
}