using System;
using UnityEngine;
using UnityEngine.AI;

public class AIEnemyFollow : MonoBehaviour
{
    public Transform Target;
    public NavMeshAgent NavAgent;
    
    void Update()
    {
        if(!NavAgent.enabled)
            return;
        
        NavAgent.SetDestination(Target.position);
    }

    private void Reset()
    {
        NavAgent = GetComponent<NavMeshAgent>();
    }
}
