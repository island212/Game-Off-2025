using System;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputActionMap _playerMap;
    private InputActionMap _gameOverMap;
    
    private void Start()
    {   
        var input = GetComponent<PlayerInput>();
        _playerMap = input.actions.FindActionMap("Player");
        _gameOverMap = input.actions.FindActionMap("GameOver");
        
        _gameOverMap.Disable();
    }

    public void Hit(Vector3 velocity)
    {
        _playerMap.Disable();
        _gameOverMap.Enable();
        
        GetComponent<FirstPersonController>().enabled = false;
        GetComponent<CharacterController>().enabled = false;
        var rigid = gameObject.GetOrAddComponent<Rigidbody>();
        rigid.AddForce(velocity, ForceMode.VelocityChange);
        
        GameEvent.RaiseGameOver();
        Debug.Log($"Player Death");
    }
}
