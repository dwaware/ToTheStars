using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuitHandler : MonoBehaviour
{
    void OnEnable()
    {
        EscapeMenu.OnQuitRequested += HandleQuit;
    }

    void OnDisable()
    {
        EscapeMenu.OnQuitRequested -= HandleQuit;
    }

    private void HandleQuit()
    {
        Debug.Log("Handling quit request");
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 