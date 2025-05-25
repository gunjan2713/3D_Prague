using UnityEngine;

public class FPSStyleCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // Your player
    public Vector3 offset = new Vector3(0, 1.8f, 0); // Height above player
    
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;
    
    [Header("Controls")]
    public KeyCode toggleMouseLook = KeyCode.Tab;
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool mouseLookActive = true;
    
    void Start()
    {
        if (target == null)
        {
            FindPlayer();
        }
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Initialize rotation
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.x;
        rotationY = angles.y;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleMouseLook))
        {
            ToggleMouseLook();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }
        
        if (mouseLookActive)
        {
            HandleMouseLook();
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Position camera at player location + offset
        transform.position = target.position + offset;
        
        // Apply rotation
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }
    
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        rotationY += mouseX;
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
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }
        
        if (player != null)
        {
            target = player.transform;
        }
    }
}