using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EscapeMenu : MonoBehaviour
{
    public GameObject escapeMenuPanel;
    private bool isMenuOpen = false;

    void Start()
    {
        // Ensure the menu is closed at start
        if (escapeMenuPanel != null)
        {
            escapeMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Escape menu panel reference is null! Please assign it in the Inspector.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        if (escapeMenuPanel != null)
        {
            escapeMenuPanel.SetActive(isMenuOpen);
        }
        else
        {
            Debug.LogError("Cannot toggle menu - panel reference is null!");
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 