using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeControl : MonoBehaviour
{
    public float IdleTimeScale = 0.1f;
    public Vector3 Offset; 
    public float Radius = 4f;
    public LayerMask EnemyLayer;
    
    private readonly Collider[] _colliders = new Collider[8];
    
    private void OnEnable()
    {
        GameEvent.OnPause += OnPause;
        GameEvent.OnResume += OnResume;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void OnDisable()
    {
        GameEvent.OnPause -= OnPause;
        GameEvent.OnResume -= OnResume;
    }
    
    void Update()
    {
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position + Offset, Radius, _colliders, EnemyLayer, QueryTriggerInteraction.Ignore);
        Time.timeScale = hitCount > 0 ? IdleTimeScale : 1f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + Offset, Radius);
    }

    private void OnPause()
    {
        enabled = false;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
        
        Time.timeScale = 0f;
    }
    
    private void OnResume()
    {
        enabled = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }
}
