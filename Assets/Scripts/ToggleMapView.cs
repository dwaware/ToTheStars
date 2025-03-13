using UnityEngine;

public class ToggleMapView : MonoBehaviour
{
    public GameObject systemMapCanvas; // Assign System Map Canvas
    public GameObject starMapCanvas;   // Assign Star Map Canvas
    public KeyCode toggleKey = KeyCode.M; // Press 'M' to toggle

    private bool showingSystemMap = false;

    void Start()
    {
        // Show star map by default
        systemMapCanvas.SetActive(false);
        starMapCanvas.SetActive(true);
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
