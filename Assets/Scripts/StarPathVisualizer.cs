using UnityEngine;

public class StarPathVisualizer : MonoBehaviour
{
    private static StarPathVisualizer instance;
    private LineRenderer pathRenderer;
    private const int SEGMENTS = 60; // Number of points in the path (every 6 degrees)
    private const float ECLIPTIC_PLANE_Y = 2000f; // Match your ecliptic plane height
    
    [SerializeField]
    private bool showPaths = false; // Disabled by default, can be toggled in inspector
    
    void Awake()
    {
        // Singleton pattern to ensure only one visualizer exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // Add LineRenderer component if it doesn't exist
        pathRenderer = gameObject.GetComponent<LineRenderer>();
        if (pathRenderer == null)
        {
            pathRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configure line renderer
        pathRenderer.positionCount = SEGMENTS + 1; // +1 to close the loop
        pathRenderer.startWidth = 15f;
        pathRenderer.endWidth = 15f;
        pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pathRenderer.startColor = new Color(1f, 1f, 0f, 0.7f);
        pathRenderer.endColor = new Color(1f, 1f, 0f, 0.7f);
        pathRenderer.loop = true;
        
        // Make sure path renders on top
        pathRenderer.material.renderQueue = 3001;
        pathRenderer.sortingOrder = 2;
        
        // Initially hide the path
        SetPathVisibility(false);
    }
    
    public void UpdatePathForStar(Vector3 starPosition)
    {
        if (!showPaths) return; // Don't update if paths are disabled
        
        // Use the same center point as the star map
        Vector3 centerPos = new Vector3(2000f, ECLIPTIC_PLANE_Y, 2000f);
        
        // Calculate the star's offset from center in XZ plane
        float dx = starPosition.x - centerPos.x;
        float dz = starPosition.z - centerPos.z;
        float radius = Mathf.Sqrt(dx * dx + dz * dz);
        
        // The star's height stays constant during rotation
        float starHeight = starPosition.y;
        
        // Generate points around a circle in the XZ plane at the star's height
        pathRenderer.positionCount = SEGMENTS + 1;
        for (int i = 0; i <= SEGMENTS; i++)
        {
            float angle = (i * 360f / SEGMENTS) * Mathf.Deg2Rad;
            float x = centerPos.x + (radius * Mathf.Cos(angle));
            float z = centerPos.z + (radius * Mathf.Sin(angle));
            
            pathRenderer.SetPosition(i, new Vector3(x, starHeight, z));
        }
        
        // Make the path more visible
        pathRenderer.startWidth = 15f;
        pathRenderer.endWidth = 15f;
        Color pathColor = new Color(1f, 1f, 0f, 0.7f); // Bright yellow, more opaque
        pathRenderer.startColor = pathColor;
        pathRenderer.endColor = pathColor;
        
        SetPathVisibility(showPaths);
    }
    
    void Update()
    {
        if (!pathRenderer.enabled || !showPaths) return;
        
        // Update visibility based on camera angle (similar to EclipticLine visibility)
        Camera mainCamera = GameObject.Find("StarMap - Camera")?.GetComponent<Camera>();
        if (mainCamera != null)
        {
            float tiltFactor = 1f - Mathf.Abs(Vector3.Dot(mainCamera.transform.forward, Vector3.up));
            Color color = pathRenderer.startColor;
            color.a = 0.7f * tiltFactor;
            pathRenderer.startColor = color;
            pathRenderer.endColor = color;
        }
    }
    
    public void SetPathVisibility(bool visible)
    {
        if (pathRenderer != null)
        {
            pathRenderer.enabled = visible && showPaths;
        }
    }
    
    public static StarPathVisualizer Instance
    {
        get { return instance; }
    }
    
    // Public method to toggle paths on/off
    public void TogglePaths(bool enable)
    {
        showPaths = enable;
        SetPathVisibility(showPaths);
    }
} 