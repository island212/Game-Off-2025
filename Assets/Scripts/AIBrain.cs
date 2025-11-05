using System;
using UnityEngine;
using UnityEngine.AI;

public class AIBrain : MonoBehaviour
{
    public Transform Target;
    public NavMeshAgent Agent;
    
    private void Start()
    {
        Target = PlayerController.LocalPlayer.transform;
        
        transform.parent = null;
        
        GameEvent.RaiseEnemySpawned();
    }

    private void OnEnable()
    {
        GameEvent.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameEvent.OnGameOver -= OnGameOver;
    }

    private void OnDestroy()
    {
        GameEvent.RaiseEnemyDied();
    }

    void OnGameOver()
    {
        Agent.enabled = false;
    }
}
