using System;
using UnityEngine;
using UnityEngine.AI;

public class AIBrain : MonoBehaviour
{
    public Transform Target;
    public NavMeshAgent Agent;
    public Transform HeadTarget;
    
    private void Start()
    {
        Target = PlayerController.LocalPlayer.transform;
        
        transform.parent = null;
        
        GameEvent.RaiseEnemySpawned();
    }
    
    private void Update()
    {
        if (HeadTarget != null && Target != null)
        {
            HeadTarget.position = Target.position;
        }
    }

    private void OnEnable()
    {
        GameEvent.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameEvent.OnGameOver -= OnGameOver;
    }

    void OnGameOver()
    {
        Agent.enabled = false;
    }

    public void OnDeath()
    {
        GameEvent.RaiseEnemyDied();
    }
}
