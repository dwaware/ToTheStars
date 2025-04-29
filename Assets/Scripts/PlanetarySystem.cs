using UnityEngine;
using System.Collections.Generic;

public class PlanetarySystem : MonoBehaviour
{
    [Header("Planet Settings")]
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject starPrefab;  // Reference to the star prefab
    [SerializeField] private float minOrbitRadius = 100f;
    [SerializeField] private float maxOrbitRadius = 950f;

    [Header("Orbit Line Settings")]
    [SerializeField] private Material orbitLineMaterial;
    [SerializeField] private float orbitLineWidth = 8f;
    [SerializeField] private int orbitLineSegments = 60;  // Back to 60 segments

    [Header("Asteroid Belt Settings")]
    [SerializeField] private GameObject asteroidPrefab;

    private readonly Color[] planetColors = new Color[] {
        Color.magenta,  // Gas giant (default purple)
        Color.green,    // Rocky
        Color.green     // Rocky
    };

    private readonly float[] planetScales = new float[] {
        50f,  // Gas giant
        25f,  // Rocky
        25f   // Rocky
    };

    private List<GameObject> planets = new List<GameObject>();
    private List<GameObject> orbitLines = new List<GameObject>();
    private List<GameObject> asteroidBelts = new List<GameObject>();
    private GameObject systemStar;

    private void Start()
    {
        if (!ValidateSetup())
        {
            Debug.LogError("PlanetarySystem: Missing required components. Please check inspector.");
            enabled = false;
            return;
        }

        // Create default orbit line material if none assigned
        if (orbitLineMaterial == null)
        {
            orbitLineMaterial = new Material(Shader.Find("Sprites/Default"));
            orbitLineMaterial.color = new Color(0.5f, 0.8f, 1f, 0.3f); // Light blue, semi-transparent
        }

        GenerateSystem();
    }

    private bool ValidateSetup()
    {
        if (planetPrefab == null)
        {
            Debug.LogError("PlanetarySystem: Planet prefab not assigned!");
            return false;
        }
        if (starPrefab == null)
        {
            Debug.LogError("PlanetarySystem: Star prefab not assigned!");
            return false;
        }
        return true;
    }

    private void GenerateSystem()
    {
        // Clear any existing objects
        ClearSystem();

        // Find Sol (our star)
        GameObject sol = GameObject.Find("[0] Sol");
        Color starColor = new Color(1f, 0.92f, 0.016f); // Default yellow color
        if (sol != null)
        {
            var solRenderer = sol.GetComponent<Renderer>();
            if (solRenderer != null && solRenderer.material != null)
            {
                starColor = solRenderer.material.color;
            }
        }

        // Create the system's star at origin
        systemStar = Instantiate(starPrefab, transform);
        systemStar.transform.localPosition = Vector3.zero;
        systemStar.transform.localScale = Vector3.one * 75f;
        systemStar.layer = LayerMask.NameToLayer("SystemMap");
        var starRenderer = systemStar.GetComponent<Renderer>();
        if (starRenderer != null && starRenderer.material != null)
        {
            starRenderer.material.color = starColor;
        }

        // Generate random number of planets (3-9)
        int planetCount = Random.Range(3, 10);
        
        // Calculate spacing based on available radius range and planet count
        float availableSpace = maxOrbitRadius - minOrbitRadius;
        float minSpacing = availableSpace / (planetCount + 1);

        // Determine maximum number of belts based on system size
        int maxBelts = planetCount < 5 ? 1 : 2;
        int currentBelts = 0;
        
        // Generate planets and possibly asteroid belts
        for (int i = 0; i < planetCount; i++)
        {
            float baseRadius = minOrbitRadius + ((i + 1) * minSpacing);
            float randomOffset = Random.Range(-minSpacing * 0.3f, minSpacing * 0.3f);
            float orbitRadius = Mathf.Clamp(baseRadius + randomOffset, minOrbitRadius, maxOrbitRadius);
            
            // Check if we can add a belt (30% chance if we haven't hit max belts)
            if (currentBelts < maxBelts && Random.value < 0.3f)
            {
                CreateAsteroidBelt(orbitRadius);
                currentBelts++;
            }
            else
            {
                CreatePlanet(orbitRadius, i % 3);
            }
            CreateOrbitLine(orbitRadius);
        }
    }

    private void CreatePlanet(float orbitRadius, int planetType)
    {
        // Generate random angle in radians
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Calculate position using polar coordinates
        Vector3 position = new Vector3(
            Mathf.Cos(angle) * orbitRadius,
            0,
            Mathf.Sin(angle) * orbitRadius
        );

        GameObject planet = Instantiate(planetPrefab, transform);
        planet.transform.localPosition = position;
        planet.transform.localScale = Vector3.one * planetScales[planetType];
        planet.layer = LayerMask.NameToLayer("SystemMap");
        var renderer = planet.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = planetColors[planetType];
        }
        planets.Add(planet);
    }

    private void CreateOrbitLine(float radius)
    {
        GameObject orbitLine = new GameObject("OrbitLine");
        orbitLine.transform.SetParent(transform);
        orbitLine.transform.localPosition = Vector3.zero;
        orbitLine.layer = LayerMask.NameToLayer("SystemMap");

        LineRenderer lineRenderer = orbitLine.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.material = orbitLineMaterial;
        lineRenderer.startWidth = orbitLineWidth;
        lineRenderer.endWidth = orbitLineWidth;
        lineRenderer.positionCount = orbitLineSegments + 1;

        // Create circle
        for (int i = 0; i <= orbitLineSegments; i++)
        {
            float angle = i * (360f / orbitLineSegments) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
        }

        orbitLines.Add(orbitLine);
    }

    private void CreateAsteroidBelt(float orbitRadius)
    {
        // Create a parent object for the belt
        GameObject beltParent = new GameObject($"Belt_{orbitRadius}");
        beltParent.transform.parent = transform;
        beltParent.transform.localPosition = Vector3.zero;
        beltParent.layer = LayerMask.NameToLayer("SystemMap");
        asteroidBelts.Add(beltParent);

        // Calculate number of asteroids based on radius
        int asteroidCount = Mathf.RoundToInt(10 + 4 * orbitRadius / 100f);

        // Create asteroids
        for (int i = 0; i < asteroidCount; i++)
        {
            float angle = (360f / asteroidCount) * i;
            // Add some random variation to the radius and angle
            float randomRadius = orbitRadius + Random.Range(-5f, 5f);
            float randomAngle = angle + Random.Range(-5f, 5f);
            
            // Convert angle to radians and calculate position
            float rad = randomAngle * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(rad) * randomRadius,
                0,
                Mathf.Sin(rad) * randomRadius
            );

            GameObject asteroid = Instantiate(asteroidPrefab, beltParent.transform);
            asteroid.transform.localPosition = position;
            asteroid.transform.localScale = Vector3.one * 10f; // Updated from 5f to 10f
            asteroid.layer = LayerMask.NameToLayer("SystemMap");

            // Set light gray color
            var renderer = asteroid.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = new Color(0.8f, 0.8f, 0.8f); // Light gray
            }
        }
    }

    private void ClearSystem()
    {
        // Destroy all existing objects
        if (systemStar != null) Destroy(systemStar);
        
        foreach (var planet in planets)
            if (planet != null) Destroy(planet);
        planets.Clear();
        
        foreach (var line in orbitLines)
            if (line != null) Destroy(line);
        orbitLines.Clear();

        foreach (var belt in asteroidBelts)
            if (belt != null) Destroy(belt);
        asteroidBelts.Clear();
    }
} 