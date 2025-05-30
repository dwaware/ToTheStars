using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class StarMapManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private GameObject galacticPlanePrefab;

    private List<GameObject> stars = new List<GameObject>();
    private List<StarSystem> starSystems = new List<StarSystem>();
    private List<Vector3> starPositions = new List<Vector3>();

    private List<StarData> sortedStarData = new List<StarData>();
    private List<float> cumulativeFrequencies = new List<float>();

    private float sphereRadius = 995f;
    private float minSeparation = 220f;
    private float powerLawExponent = 2.0f;

    private Vector3 mapOrigin = new Vector3(2000f, 2000f, 2000f);
    private string logsPath;

    [System.Serializable]
    public class StarSystem
    {
        public string name;
        public float x, y, z;

        public StarSystem(string name, float x, float y, float z)
        {
            this.name = name;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    [System.Serializable]
    public class StarSystemList
    {
        public List<StarSystem> starSystems;
    }

    [System.Serializable]
    public class StarData
    {
        public string StellarClass;
        public int Subclass;
        public string LuminosityClass;
        public float Mass;
        public float Radius;
        public float Temp;
        public float Luminosity;
        public string RGB;
        public string Color;
        public string Planets;
        public float Frequency;
    }

    [System.Serializable]
    public class StarDataList
    {
        public List<StarData> stars;
    }

    private Mesh CreateHighResolutionCylinderMesh(int segments)
    {
        Mesh mesh = new Mesh();
        
        // Calculate vertices
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        // Add center vertices (top and bottom)
        vertices.Add(Vector3.up * 0.5f);   // Top center
        vertices.Add(Vector3.down * 0.5f); // Bottom center
        
        // Add perimeter vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);
            
            // Top perimeter
            vertices.Add(new Vector3(x, 0.5f, z));
            // Bottom perimeter
            vertices.Add(new Vector3(x, -0.5f, z));
        }
        
        // Create triangles
        for (int i = 0; i < segments; i++)
        {
            int current = 2 + i * 2;
            int next = 2 + ((i + 1) % segments) * 2;
            
            // Top cap
            triangles.Add(0);
            triangles.Add(current);
            triangles.Add(next);
            
            // Bottom cap
            triangles.Add(1);
            triangles.Add(next + 1);
            triangles.Add(current + 1);
            
            // Side quad (as two triangles)
            triangles.Add(current);
            triangles.Add(current + 1);
            triangles.Add(next);
            
            triangles.Add(next);
            triangles.Add(current + 1);
            triangles.Add(next + 1);
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }

    void Start()
    {
        if (!starPrefab) Debug.LogError("[StarMapManager] No Star Prefab assigned!");
        if (!galacticPlanePrefab) Debug.LogError("[StarMapManager] No Galactic Plane Prefab assigned!");
        
        mapOrigin = new Vector3(2000f, 2000f, 2000f);
        
        // Create the galactic plane with high resolution mesh
        GameObject galacticPlane = Instantiate(galacticPlanePrefab, mapOrigin, Quaternion.identity);
        galacticPlane.name = "Galactic Plane";
        
        // Replace the default cylinder mesh with our high resolution one
        MeshFilter meshFilter = galacticPlane.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            meshFilter.mesh = CreateHighResolutionCylinderMesh(72); // 72 segments for smooth circle
        }
        
        // Create path visualizer
        GameObject pathVisualizerObj = new GameObject("StarPathVisualizer");
        pathVisualizerObj.transform.position = mapOrigin; // Set it at the map origin
        pathVisualizerObj.layer = LayerMask.NameToLayer("StarMap"); // Put it in the StarMap layer
        pathVisualizerObj.AddComponent<StarPathVisualizer>();
        
        Debug.Log("🔭 [StarMapManager] Starting...");
        logsPath = Path.Combine(Application.dataPath, "..", "GameLogs");
        if (!Directory.Exists(logsPath)) Directory.CreateDirectory(logsPath);

        LoadStarData();
        GenerateStarSystems();
        SaveStarSystemsToJson();
        SaveBinnedDistances();
        SummarizeStarSystems();
    }

    void LoadStarData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "spectral_classification_final_normalized.json");
        string json = File.ReadAllText(filePath);
        StarDataList starDataList = JsonUtility.FromJson<StarDataList>(json);

        Debug.Log($"📄 Loaded {starDataList.stars.Count} stars from file.");

        float totalFrequency = starDataList.stars.Sum(s => s.Frequency);
        Debug.Log($"📊 Total Frequency from JSON: {totalFrequency}%");

        if (Mathf.Abs(totalFrequency - 100.00f) > 0.01f)
            Debug.LogWarning("⚠️ WARNING: Total frequency does not sum to 100%! Check JSON data.");

        PrecomputeCumulativeDistribution(starDataList.stars);
    }

    void PrecomputeCumulativeDistribution(List<StarData> starList)
    {
        sortedStarData.Clear();
        cumulativeFrequencies.Clear();

        sortedStarData = starList.OrderBy(s => s.Frequency).ToList();

        float cumulativeSum = 0f;
        foreach (var star in sortedStarData)
        {
            cumulativeSum += star.Frequency;
            cumulativeFrequencies.Add(cumulativeSum);
        }

        Debug.Log("✅ CDF table created for fast weighted selection.");
    }

    StarData GetRandomWeightedStar()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        int index = cumulativeFrequencies.BinarySearch(randomValue);

        if (index < 0) index = ~index;

        return sortedStarData[Mathf.Clamp(index, 0, sortedStarData.Count - 1)];
    }

    void GenerateStarSystems()
    {
        if (sortedStarData.Count == 0)
        {
            Debug.LogError("❌ No star data available! Cannot generate star systems.");
            return;
        }

        starSystems.Clear();
        starPositions.Clear();

        StarSystem sol = new StarSystem("[0] Sol", mapOrigin.x, mapOrigin.y, mapOrigin.z);
        starSystems.Add(sol);
        starPositions.Add(mapOrigin);
        CreateStarGameObject(sol, Color.yellow);

        int numStars = 50;
        for (int i = 1; i < numStars; i++)
        {
            Vector3 position;
            int attempts = 0;
            bool validPosition = false;

            do
            {
                float r = Mathf.Pow(sphereRadius, 1 - powerLawExponent) -
                          (Mathf.Pow(sphereRadius, 1 - powerLawExponent) - Mathf.Pow(minSeparation, 1 - powerLawExponent)) * UnityEngine.Random.value;
                r = Mathf.Pow(r, 1 / (1 - powerLawExponent));

                position = mapOrigin + UnityEngine.Random.onUnitSphere * r;
                validPosition = starPositions.All(existing => Vector3.Distance(existing, position) >= minSeparation);
                attempts++;
            }
            while (!validPosition && attempts < 1000);

            if (!validPosition) continue;

            starPositions.Add(position);

            StarData starData = GetRandomWeightedStar();
            string starName = $"[{i}] {starData.StellarClass}{starData.Subclass}({starData.LuminosityClass})";
            StarSystem newStarSystem = new StarSystem(starName, position.x, position.y, position.z);
            starSystems.Add(newStarSystem);

            CreateStarGameObject(newStarSystem, ParseColorFromRGB(starData.RGB));
        }
    }

    void CreateStarGameObject(StarSystem starSystem, Color starColor)
    {
        GameObject starObj = Instantiate(starPrefab, new Vector3(starSystem.x, starSystem.y, starSystem.z), Quaternion.identity);
        starObj.name = starSystem.name;

        starObj.transform.localScale = new Vector3(25, 25, 25);

        int starMapLayer = LayerMask.NameToLayer("StarMap");
        starObj.layer = starMapLayer;
        foreach (Transform t in starObj.GetComponentsInChildren<Transform>())
            t.gameObject.layer = starMapLayer;

        SphereCollider sc = starObj.GetComponent<SphereCollider>();
        if (sc != null)
        {
            sc.radius = 0.5f; // back to normalized
            sc.isTrigger = false;
            Debug.Log($"🛠️ Created star: {starSystem.name}, Pos: {starObj.transform.position}, Scale: {starObj.transform.localScale}, Collider Radius: {sc.radius}, Layer: {starObj.layer}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No SphereCollider found on star prefab: {starSystem.name}");
        }

        var renderer = starObj.GetComponent<Renderer>();
        var material = renderer.material;

        material.color = starColor;
        material.EnableKeyword("_EMISSION");
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        material.SetColor("_EmissionColor", starColor * 0.2f);

        // Add the EclipticLine component
        starObj.AddComponent<EclipticLine>();

        stars.Add(starObj);
    }

    Color ParseColorFromRGB(string rgbString)
    {
        var rgb = rgbString.Trim('(', ')').Split(',');
        return new Color(float.Parse(rgb[0]) / 255f, float.Parse(rgb[1]) / 255f, float.Parse(rgb[2]) / 255f);
    }

    void SaveStarSystemsToJson()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(logsPath, $"StarSystems_{timestamp}.json");

        StarSystemList starSystemList = new StarSystemList { starSystems = this.starSystems };
        string json = JsonUtility.ToJson(starSystemList, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"📁 Star systems saved to: {filePath}");
    }

    void SaveBinnedDistances()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(logsPath, $"star_distance_bins_{timestamp}.txt");

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var pos in starPositions)
            {
                float distance = Vector3.Distance(mapOrigin, pos);
                writer.WriteLine($"Distance: {distance}");
            }
        }

        Debug.Log($"📏 Binned distance data saved: {filePath}");
    }

    void SummarizeStarSystems()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(logsPath, $"StarSystemsSummary_{timestamp}.txt");

        string summary = $"Total Systems: {starSystems.Count}\n";
        File.WriteAllText(filePath, summary);
        Debug.Log($"🧾 Summary saved to: {filePath}");
    }

    public void ClearAndRegenerate()
    {
        Debug.Log("♻️ Clearing and regenerating stars...");
        foreach (GameObject star in stars) Destroy(star);
        stars.Clear();
        starSystems.Clear();
        starPositions.Clear();

        GenerateStarSystems();
        SaveStarSystemsToJson();
        SaveBinnedDistances();
        SummarizeStarSystems();
    }

    public void SummarizeCurrentSystems()
    {
        Debug.Log("📋 Summarizing current systems...");
        SaveStarSystemsToJson();
        SaveBinnedDistances();
        SummarizeStarSystems();
    }
}
