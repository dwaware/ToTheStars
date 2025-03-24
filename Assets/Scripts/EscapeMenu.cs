using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EscapeMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject escapeMenuPanel;
    
    private bool isMenuOpen = false;

    public static event System.Action OnQuitRequested;

    void Awake()
    {
        if (escapeMenuPanel != null)
        {
            escapeMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Escape menu panel not assigned! Please assign it in the Inspector.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        if (escapeMenuPanel != null)
        {
            isMenuOpen = !isMenuOpen;
            escapeMenuPanel.SetActive(isMenuOpen);
        }
        else
        {
            Debug.LogError("Cannot toggle menu - panel reference is missing!");
        }
    }

    public void RequestQuit()
    {
        Debug.Log("Quit requested");
        OnQuitRequested?.Invoke();
    }
} 