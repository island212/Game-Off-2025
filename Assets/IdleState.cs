using UnityEngine;

public class IdleState : StateMachineBehaviour
{
    public bool IsInState { get; private set; }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        IsInState = true;
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        IsInState = false;
    }
}
