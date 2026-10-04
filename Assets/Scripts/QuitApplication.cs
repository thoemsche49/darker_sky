using UnityEngine;

/// <summary>
/// Quits the application, or stops Play Mode while running in the editor.
/// Called from the quit button in the burger menu.
/// </summary>
public class QuitApplication : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}