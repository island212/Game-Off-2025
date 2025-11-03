using System;
using System.Collections.Generic;
using Hanzzz.MeshSlicerFree;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlash : MonoBehaviour
{
    public InputActionReference AttackAction;
    public Vector2 SlashForce = new Vector2(10f, 30f);
    public float SlashTorque = 10f;
    
    public Material SliceMaterial;
    public Transform RayOrigin;
    public LayerMask RaycastMask;
    public float RaycastDistance = 3f;

    private readonly Collider[] _candidates = new Collider[32];
    private readonly MeshSlicer _meshSlicer = new ();
    
    private Vector3 _enterBasePosition, _enterTipPosition;
    
    private void OnEnable()
    {
        AttackAction.action.started += OnAttackStart;
        AttackAction.action.canceled += OnAttackEnd;
    }

    private void OnDisable()
    {
        AttackAction.action.started -= OnAttackStart;
        AttackAction.action.canceled -= OnAttackEnd;
    }

    private void OnAttackStart(InputAction.CallbackContext ctx)
    {
        _enterBasePosition = RayOrigin.position;
        _enterTipPosition = RayOrigin.position + RayOrigin.forward * RaycastDistance;
    }
    
    private void OnAttackEnd(InputAction.CallbackContext ctx)
    {
        var exitBasePosition = RayOrigin.position;
        var exitTipPosition = RayOrigin.position + RayOrigin.forward * RaycastDistance;

        var count = GetCollidersIntersectingPlane(_enterBasePosition, _enterTipPosition, exitBasePosition, exitTipPosition);
        
        var normal = Vector3.Cross(_enterTipPosition - _enterBasePosition, exitTipPosition - _enterBasePosition);
        
        Debug.DrawRay(_enterBasePosition, _enterTipPosition, Color.red, 10f);
        Debug.DrawRay(_enterBasePosition, exitTipPosition, Color.red, 10f);
        Debug.DrawRay(_enterBasePosition, normal, Color.blue, 10f);
        
        for (var i = 0; i < count; i++)
        {
            if(!_candidates[i].TryGetComponent<Sliceable>(out _))
                return;
            
            var (go1, go2) = _meshSlicer.Slice(_candidates[i].gameObject, (_enterBasePosition, _enterTipPosition, exitTipPosition), SliceMaterial);
            if(go1 == null || go2 == null)
                continue;
            
            _candidates[i].gameObject.SetActive(false);
            Destroy(_candidates[i].gameObject);

            var direction = (exitTipPosition - _enterTipPosition).normalized;
            
            var force = direction * SlashForce.y;
            var torqueAxis = Vector3.Cross(direction, normal);
            
            go1.GetComponent<Sliceable>().OnSlice(force + normal * SlashForce.x, torqueAxis * SlashTorque, ForceMode.VelocityChange);
            go2.GetComponent<Sliceable>().OnSlice(force - normal * SlashForce.x, -torqueAxis * SlashTorque, ForceMode.VelocityChange);
        }
    }

    private int GetCollidersIntersectingPlane(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        // Create a bounding box that encompasses the 4 points
        Bounds bounds = new Bounds(p1, Vector3.zero);
        bounds.Encapsulate(p2);
        bounds.Encapsulate(p3);
        bounds.Encapsulate(p4);

        // Add some thickness to the bounds
        bounds.Expand(0.1f);
        
        // Get all colliders in the bounding box
        return Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, _candidates, Quaternion.identity, RaycastMask);
    }
}