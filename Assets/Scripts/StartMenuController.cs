using UnityEngine;

public class StartMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject exitButton;

    private void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(false);
        }
#endif
    }
    
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
