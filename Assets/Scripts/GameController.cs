using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public InputActionAsset InputActions;

    private InputActionMap _playerMap;
    private InputActionMap _endScreenMap;
    
    private InputAction _restartAction;

    private void Awake()
    {
        _playerMap = InputActions.FindActionMap("Player");
        _endScreenMap = InputActions.FindActionMap("EndScreen");
        _restartAction = _endScreenMap.FindAction("Restart");
    }

    private void Start()
    {
        _playerMap.Enable();
        _endScreenMap.Disable();
    }

    private void OnEnable()
    {
        _restartAction.performed += OnRestart;
        
        GameEvent.OnGameOver += OnGameOver;
        GameEvent.OnPlayerWin += OnPlayerWin;
    }

    private void OnDisable()
    {
        _restartAction.performed -= OnRestart;
        
        GameEvent.OnGameOver -= OnGameOver;
        GameEvent.OnPlayerWin -= OnPlayerWin;
    }
    
    private void OnPlayerWin()
    {
        _playerMap.Disable();
        _endScreenMap.Enable();
    }
    
    private void OnGameOver()
    {
        _playerMap.Disable();
        _endScreenMap.Enable();
    }

    private void OnRestart(InputAction.CallbackContext obj)
    {
        SceneManager.LoadScene(SceneConstants.PLAYGROUND);
    }
}
