using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;

    public InputActionAsset InputActions;

    private bool _paused = false;
    private InputActionMap _pauseMap;
    private InputAction _togglePauseAction;
    
    private CursorLockMode _previousCursorLockMode;
    private bool _previousCursorVisible;

    private void Awake()
    {
        _pauseMap = InputActions.FindActionMap("Pause");
        _togglePauseAction = _pauseMap.FindAction("TogglePause");
    }

    private void Start()
    {
        _pauseMap.Enable();
    }
    
    private void OnEnable()
    {
        _togglePauseAction.performed += OnTogglePause;
    }

    private void OnDisable()
    {
        _togglePauseAction.performed -= OnTogglePause;
    }
    
    public void OnTogglePause(InputAction.CallbackContext obj)
    {
        _paused = !_paused;
        
        if (_paused)
        {
            OnPause();
        }
        else
        {
            OnResume();
        }
    }
    
    private void OnPause()
    {
        GameEvent.RaisePause();

        Time.timeScale = 0f;
        
        _previousCursorLockMode = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
    
    private void OnResume()
    {
        GameEvent.RaiseResume();

        Time.timeScale = 1f;
        
        Cursor.lockState = _previousCursorLockMode;
        Cursor.visible = _previousCursorVisible;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
    
    public void OnContinueClicked()
    {
        OnResume();
        _paused = false;
    }
    
    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneConstants.PLAYGROUND);
    }
    
    public void OnBackToMenuClicked()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(SceneConstants.MENU);
    }
}
