using UnityEngine;

public class IndependentCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // Assign your player here
    public Vector3 offset = new Vector3(0, 4, 1); // Camera position relative to player (CHANGED: closer and more forward)

    [Header("Mouse Look Settings")]
    public bool enableMouseLook = true;
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -30f; // How far down you can look
    public float maxVerticalAngle = 60f;  // How far up you can look

    [Header("Camera Movement")]
    public float followSpeed = 10f;
    public bool smoothFollow = true;

    [Header("Controls")]
    public KeyCode toggleMouseLook = KeyCode.Tab;

    private float rotationX = 0f; // Up/down rotation
    private float rotationY = 0f; // Left/right rotation
    private bool mouseLookActive = true;

    void Start()
    {
        // Find player if not assigned
        if (target == null)
        {
            FindPlayer();
        }

        // Initialize rotation based on current camera rotation
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.x;
        rotationY = angles.y;

        // Lock cursor if mouse look is enabled
        if (enableMouseLook && mouseLookActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // Toggle mouse look on/off
        if (Input.GetKeyDown(toggleMouseLook))
        {
            ToggleMouseLook();
        }

        // Handle mouse input for camera rotation
        if (enableMouseLook && mouseLookActive)
        {
            HandleMouseLook();
        }

        // Unlock cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position based on rotation and offset
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        Vector3 desiredPosition = target.position + rotation * offset;

        // Move camera to desired position
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = desiredPosition;
        }

        // FIXED: Make camera look in the direction it's facing, not at the target
        // This creates a proper first-person perspective where player stays behind camera
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }

    void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Apply horizontal rotation (left/right)
        rotationY += mouseX;

        // Apply vertical rotation (up/down) and clamp it
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
    }

    void ToggleMouseLook()
    {
        mouseLookActive = !mouseLookActive;

        if (mouseLookActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Mouse look enabled");
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Mouse look disabled");
        }
    }

    void UnlockCursor()
    {
        mouseLookActive = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void FindPlayer()
    {
        // Try to find player by tag
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            return;
        }

        // Try to find by name
        player = GameObject.Find("Player");
        if (player != null)
        {
            target = player.transform;
            return;
        }

        // Try to find by PlayerController component
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            target = playerController.transform;
        }

        if (target != null)
        {
            Debug.Log($"Found player: {target.name}");
        }
        else
        {
            Debug.LogWarning("Could not find player! Please assign target manually.");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw the target position and offset in editor
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, 1f);

            Gizmos.color = Color.cyan;
            Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
            Vector3 cameraPos = target.position + rotation * offset;
            Gizmos.DrawWireSphere(cameraPos, 0.5f);
            Gizmos.DrawLine(target.position, cameraPos);
        }
    }
}