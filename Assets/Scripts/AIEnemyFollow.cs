using System;
using UnityEngine;
using UnityEngine.AI;

public class AIEnemyFollow : MonoBehaviour
{
    public AIBrain Brain;
    public NavMeshAgent NavAgent;
    
    void Update()
    {
        if(!NavAgent.enabled)
            return;
        
        NavAgent.SetDestination(Brain.Target.position);
    }

    private void Reset()
    {
        Brain = GetComponent<AIBrain>();
        NavAgent = GetComponent<NavMeshAgent>();
    }
}
