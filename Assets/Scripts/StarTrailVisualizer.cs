using UnityEngine;

public class StarTrailVisualizer : MonoBehaviour
{
    private TrailRenderer trailRenderer;
    private float trailTime = 5.0f; // How long the trail persists
    
    void Start()
    {
        // Add TrailRenderer component if it doesn't exist
        trailRenderer = gameObject.GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }
        
        // Configure trail renderer
        trailRenderer.time = trailTime; // Trail lifetime
        trailRenderer.startWidth = 0.1f;
        trailRenderer.endWidth = 0.05f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // Set trail colors
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f),
                new GradientColorKey(Color.white, 1.0f) 
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        trailRenderer.colorGradient = gradient;
        
        // Make sure trail is rendered on top
        trailRenderer.sortingOrder = 1;
    }
    
    public void SetTrailTime(float time)
    {
        trailTime = time;
        if (trailRenderer != null)
        {
            trailRenderer.time = time;
        }
    }
    
    public void ClearTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }
} 