using System;
using UnityEngine;

public class AIMelee : MonoBehaviour
{
    private static readonly int InRange = Animator.StringToHash("InRange");
    
    public Transform RayOrigin;
    public float AttackRange = 2f;
    
    [Header("Dependencies")]
    public AIBrain Brain;
    public Animator Animator;
    
    private bool _inMeleeRange;
    
    private void Update()
    {
        var distanceSqr = (Brain.Target.position - RayOrigin.position).sqrMagnitude;
        _inMeleeRange = distanceSqr < AttackRange * AttackRange;

        Animator.SetBool(InRange, _inMeleeRange);
    }

    public void OnPunchExtended()
    {
        if(!_inMeleeRange)
            return;
        
        var direction = (Brain.Target.position - RayOrigin.position).normalized;
        
        Brain.Target.GetComponent<PlayerController>().Hit(direction * 10f);
    }

    private void Reset()
    {
        Brain = GetComponent<AIBrain>();
        Animator = GetComponent<Animator>();
    }
}
