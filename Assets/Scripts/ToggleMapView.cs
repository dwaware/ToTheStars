using UnityEngine;

public class ToggleMapView : MonoBehaviour
{
    public GameObject systemMapCanvas; // Assign System Map Canvas
    public GameObject starMapCanvas;   // Assign Star Map Canvas
    public KeyCode toggleKey = KeyCode.M; // Press 'M' to toggle

    private bool showingSystemMap = true;

    void Start()
    {
        // Ensure only one map is active at the start
        systemMapCanvas.SetActive(true);
        starMapCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMaps();
        }
    }

    void ToggleMaps()
    {
        showingSystemMap = !showingSystemMap;

        systemMapCanvas.SetActive(showingSystemMap);
        starMapCanvas.SetActive(!showingSystemMap);
    }
}
