using UnityEngine;
using UnityEngine.EventSystems;

public class QuitButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private EscapeMenu escapeMenu;

    void Start()
    {
        // Fallback if not set in inspector
        if (escapeMenu == null)
        {
            escapeMenu = FindFirstObjectByType<EscapeMenu>();
            Debug.LogWarning("EscapeMenu not set in inspector - found via FindFirstObjectByType");
        }
        
        if (escapeMenu == null)
        {
            Debug.LogError("Could not find EscapeMenu in scene!");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (escapeMenu != null)
        {
            escapeMenu.RequestQuit();
        }
    }
} 