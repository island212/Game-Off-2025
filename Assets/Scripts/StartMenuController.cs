using UnityEngine;

public class StartMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionPanel;

    public void OnStartClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneConstants.PLAYGROUND);
    }

    public void OnBackFromOptionsClicked()
    {
        optionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnOptionsClicked()
    {
        optionPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
