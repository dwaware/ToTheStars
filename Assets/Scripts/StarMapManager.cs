using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class StarMapManager : MonoBehaviour
{
    public GameObject starPrefab;
    public GameObject escapeMenuPanel;

    private List<GameObject> stars = new List<GameObject>();
    private List<StarSystem> starSystems = new List<StarSystem>();
    private List<Vector3> starPositions = new List<Vector3>();

    private List<StarData> sortedStarData = new List<StarData>();
    private List<float> cumulativeFrequencies = new List<float>();

    private float sphereRadius = 995f;
    private float minSeparation = 220f;
    private float powerLawExponent = 2.0f;
    private bool isEscapeMenuOpen = false;

    // Origin point for the star map
    private Vector3 mapOrigin = new Vector3(2000f, 2000f, 2000f);

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

    void Start()
    {
        Debug.Log("StarMapManager started.");
        LoadStarData();
        GenerateStarSystems();
        SaveStarSystemsToJson();
        SaveBinnedDistances();
        SummarizeStarSystems();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleEscapeMenu();
        }
    }

    void ToggleEscapeMenu()
    {
        isEscapeMenuOpen = !isEscapeMenuOpen;
        escapeMenuPanel.SetActive(isEscapeMenuOpen);
        Debug.Log($"Escape Menu is now {(isEscapeMenuOpen ? "OPEN" : "CLOSED")}");
    }

    void LoadStarData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "spectral_classification_final_normalized.json");
        string json = File.ReadAllText(filePath);
        StarDataList starDataList = JsonUtility.FromJson<StarDataList>(json);

        Debug.Log($"Loaded {starDataList.stars.Count} stars from file.");

        float totalFrequency = starDataList.stars.Sum(s => s.Frequency);
        Debug.Log($"Total Frequency from JSON: {totalFrequency}%");

        if (Mathf.Abs(totalFrequency - 100.00f) > 0.01f)
        {
            Debug.LogWarning("WARNING: Total frequency does not sum to 100%! Check JSON data.");
        }

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

        Debug.Log("CDF table created for fast weighted selection.");
    }

    StarData GetRandomWeightedStar()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        int index = cumulativeFrequencies.BinarySearch(randomValue);

        if (index < 0)
            index = ~index;

        return sortedStarData[Mathf.Clamp(index, 0, sortedStarData.Count - 1)];
    }

    void GenerateStarSystems()
    {
        if (sortedStarData.Count == 0)
        {
            Debug.LogError("No star data available! Cannot generate star systems.");
            return;
        }

        starSystems.Clear();
        starPositions.Clear();

        // Create Sol at the map origin
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

                // Generate position relative to map origin
                position = mapOrigin + UnityEngine.Random.onUnitSphere * r;

                validPosition = starPositions.All(existing => Vector3.Distance(existing, position) >= minSeparation);

                attempts++;

            } while (!validPosition && attempts < 1000);

            if (!validPosition) continue;

            starPositions.Add(position);

            StarData starData = GetRandomWeightedStar();

            string starName = $"[{i}] {starData.StellarClass}{starData.Subclass}({starData.LuminosityClass})";
            StarSystem newStarSystem = new StarSystem(starName, position.x, position.y, position.z);
            starSystems.Add(newStarSystem);

            CreateStarGameObject(newStarSystem, ParseColorFromRGB(starData.RGB));
        }

        SaveStarSystemsToJson();
    }

    void SummarizeStarSystems()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(Application.streamingAssetsPath, $"StarSystemsSummary_{timestamp}.txt");

        string summary = $"Total Systems: {starSystems.Count}\n";
        File.WriteAllText(filePath, summary);
        Debug.Log($"Summary saved to: {filePath}");
    }

    void CreateStarGameObject(StarSystem starSystem, Color starColor)
    {
        GameObject starObj = Instantiate(starPrefab, new Vector3(starSystem.x, starSystem.y, starSystem.z), Quaternion.identity);
        starObj.transform.localScale = new Vector3(25, 25, 25);
        starObj.name = starSystem.name;
        starObj.GetComponent<Renderer>().material.color = starColor;
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
        string filePath = Path.Combine(Application.streamingAssetsPath, $"StarSystems_{timestamp}.json");

        StarSystemList starSystemList = new StarSystemList { starSystems = this.starSystems };

        string json = JsonUtility.ToJson(starSystemList, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Star systems successfully saved to: {filePath}");
    }

    void SaveBinnedDistances()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(Application.streamingAssetsPath, $"star_distance_bins_{timestamp}.txt");

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var pos in starPositions)
            {
                float distance = Vector3.Distance(mapOrigin, pos);
                writer.WriteLine($"Distance: {distance}");
            }
        }

        Debug.Log($"Binned distance data saved: {filePath}");
    }

    public void ClearAndRegenerate()
    {
        Debug.Log("Clearing existing stars and regenerating...");

        // Destroy all star GameObjects in the scene
        foreach (GameObject star in stars)
        {
            Destroy(star);
        }
        stars.Clear();  // Clear the list of GameObjects

        // Clear stored star data
        starSystems.Clear();
        starPositions.Clear();

        // Generate new star systems
        GenerateStarSystems();

        // Save the new data
        SaveStarSystemsToJson();
        SaveBinnedDistances();
        SummarizeStarSystems();
    }

    public void SummarizeCurrentSystems()
    {
        Debug.Log("Summarizing existing data...");

        // Save the data
        SaveStarSystemsToJson();
        SaveBinnedDistances();
        SummarizeStarSystems();
    }
} 