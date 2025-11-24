using UnityEngine;

public class StartMenuController : MonoBehaviour
{
    public void OnStartClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneConstants.PLAYGROUND);
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
