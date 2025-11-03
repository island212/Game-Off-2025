using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUnarmed : MonoBehaviour
{
    public float PunchForce = 10f;
    
    public Transform RayOrigin;
    public LayerMask RaycastMask;
    public float RaycastDistance = 2f;
    
    public void OnAttack(InputValue value)
    {
        if(!Physics.Raycast(RayOrigin.position, RayOrigin.forward, out var hitInfo, RaycastDistance, RaycastMask))
            return;
        
        if(!hitInfo.rigidbody.TryGetComponent<IHitable>(out var hitableComponent))
            return;
        
        hitableComponent.AddForce(RayOrigin.forward * PunchForce, hitInfo.point, ForceMode.VelocityChange);
    }
}
