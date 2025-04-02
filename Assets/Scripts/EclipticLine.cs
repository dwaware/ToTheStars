using UnityEngine;

public class EclipticLine : MonoBehaviour
{
    private const float ECLIPTIC_PLANE_Y = 2000f;
    private const float ECLIPTIC_PLANE_RADIUS = 5000f; // Assuming this is the radius of your disc
    private LineRenderer lineRenderer;
    private Camera mainCamera;
    private float lineWidth = 3.0f;
    
    // Base colors with full opacity
    private Color abovePlaneColor = new Color(1f, 0f, 0f, 1f);    // Red
    private Color belowPlaneColor = new Color(0f, 1f, 0f, 1f);    // Green
    private Color transparentColor = new Color(0f, 0f, 0f, 0.1f);  // Nearly transparent
    private Color occludedColor = new Color(0f, 0f, 0f, 0.1f);    // Color for occluded parts

    void Start()
    {
        // Get or add LineRenderer component
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Configure the line renderer
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        // Make sure the line renders on top
        lineRenderer.material.renderQueue = 3000;
        lineRenderer.sortingOrder = 1;

        // Find the StarMap camera
        mainCamera = GameObject.Find("StarMap - Camera")?.GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("Could not find StarMap - Camera!");
        }
    }

    void Update()
    {
        if (lineRenderer == null || mainCamera == null) return;

        Vector3 starPos = transform.position;
        Vector3 planePos = new Vector3(starPos.x, ECLIPTIC_PLANE_Y, starPos.z);
        
        // Calculate tilt factor - now 1 at 90 degrees (edge-on) and 0 at 0 degrees (looking straight down/up)
        float tiltFactor = 1f - Mathf.Abs(Vector3.Dot(mainCamera.transform.forward, Vector3.up));
        
        // Debug logging
        Debug.Log($"Camera Debug for {gameObject.name}:");
        Debug.Log($"  Camera Forward: {mainCamera.transform.forward}");
        Debug.Log($"  Dot Product with Up: {Vector3.Dot(mainCamera.transform.forward, Vector3.up)}");
        Debug.Log($"  Tilt Factor: {tiltFactor}");
        Debug.Log($"  Star Position: {starPos}");
        Debug.Log($"  Plane Position: {planePos}");
        
        // Set line positions
        lineRenderer.SetPosition(0, starPos);
        lineRenderer.SetPosition(1, planePos);

        // Adjust colors based on star position and tilt
        Color startColor, endColor;
        if (starPos.y > ECLIPTIC_PLANE_Y)
        {
            startColor = new Color(abovePlaneColor.r, abovePlaneColor.g, abovePlaneColor.b, abovePlaneColor.a * tiltFactor);
            endColor = new Color(transparentColor.r, transparentColor.g, transparentColor.b, transparentColor.a * tiltFactor);
        }
        else
        {
            startColor = new Color(belowPlaneColor.r, belowPlaneColor.g, belowPlaneColor.b, belowPlaneColor.a * tiltFactor);
            endColor = new Color(transparentColor.r, transparentColor.g, transparentColor.b, transparentColor.a * tiltFactor);
        }
        
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;
        
        Debug.Log($"  Final Colors - Start: {startColor}, End: {endColor}");
    }
} 