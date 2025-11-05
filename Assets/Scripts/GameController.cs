using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public InputActionReference RestartAction;

    private void OnEnable()
    {
        RestartAction.action.performed += OnRestart;
    }

    private void OnDisable()
    {
        RestartAction.action.performed -= OnRestart;
    }

    private void OnRestart(InputAction.CallbackContext obj)
    {
        SceneManager.LoadScene(0);
    }
}
