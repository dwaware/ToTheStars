using UnityEngine;

public class StarMapController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float maxVerticalAngle = 80f; // Maximum up/down rotation

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private Vector3 rotationCenter = new Vector3(2000f, 2000f, 2000f);
    private float currentVerticalAngle = 0f;
    private Vector3 lastMousePosition;
    private bool isDragging = false;

    private void Start()
    {
        if (cameraTransform == null)
        {
            // Try to find the camera if not assigned
            var starMapCam = GameObject.Find("StarMapCamera")?.GetComponentInChildren<Camera>();
            if (starMapCam != null)
            {
                cameraTransform = starMapCam.transform;
            }
            else
            {
                Debug.LogError("StarMapController: Camera not found! Please assign it in the inspector.");
                enabled = false;
                return;
            }
        }
    }

    private void Update()
    {
        // Only process input if the star map is active
        if (!gameObject.activeSelf) return;

        HandleMapRotation();
    }

    private void HandleMapRotation()
    {
        // Start dragging
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        // End dragging
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        // Process dragging
        else if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            
            // Calculate rotation amounts
            float horizontalRotation = -delta.x * rotationSpeed;
            float verticalRotation = -delta.y * rotationSpeed;

            // Calculate new vertical angle
            float newVerticalAngle = currentVerticalAngle + verticalRotation;
            
            // Clamp vertical rotation
            if (Mathf.Abs(newVerticalAngle) <= maxVerticalAngle)
            {
                currentVerticalAngle = newVerticalAngle;
                // Rotate around X axis (up/down)
                cameraTransform.RotateAround(rotationCenter, cameraTransform.right, verticalRotation);
            }

            // Rotate around Y axis (left/right) - no constraints
            cameraTransform.RotateAround(rotationCenter, Vector3.up, horizontalRotation);

            lastMousePosition = Input.mousePosition;
        }
    }
} 