using System;
using System.Collections.Generic;
using Hanzzz.MeshSlicerFree;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlash : MonoBehaviour
{
    public InputActionReference AttackAction;
    public Vector2 SlashForce = new Vector2(10f, 30f);
    public float SlashTorque = 10f;

    [Header("Slice Mesh")]
    public SliceableReference SlicePrefab;
    public Material SliceMaterial;
    
    [Header("Collision")]
    public Transform RayOrigin;
    public LayerMask RaycastMask;
    public float RaycastDistance = 3f;

    private readonly Collider[] _candidates = new Collider[32];
    private readonly MeshSlicer _meshSlicer = new();
    private readonly List<Material> _targetMaterials = new();

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

        var count = GetCollidersIntersectingPlane(_enterBasePosition, _enterTipPosition, exitBasePosition,
            exitTipPosition);

        var normal = Vector3.Cross(_enterTipPosition - _enterBasePosition, exitTipPosition - _enterBasePosition);
        normal.Normalize();

        Debug.DrawRay(_enterBasePosition, _enterTipPosition, Color.red, 10f);
        Debug.DrawRay(_enterBasePosition, exitTipPosition, Color.red, 10f);
        Debug.DrawRay(_enterBasePosition, normal, Color.blue, 10f);

        for (var i = 0; i < count; i++)
        {
            var meshFilter = _candidates[i].GetComponent<MeshFilter>();
            var rootTransform = _candidates[i].transform.root;
            
            Assert.IsTrue(meshFilter != null, $"Missing MeshFilter on {_candidates[i].name}. Sliceable must have a MeshFilter at the collider. Parent was {rootTransform.name}");
            
            var (mesh1, mesh2) = _meshSlicer.Slice((_enterBasePosition, _enterTipPosition, exitTipPosition), meshFilter.sharedMesh,
                _candidates[i].transform, true);
            
            if(mesh1 == null || mesh2 == null)
                continue;
            
            var direction = (exitTipPosition - _enterTipPosition).normalized;
            var force = direction * SlashForce.y;
            var torqueAxis = Vector3.Cross(direction, normal);
            
            _candidates[i].GetComponent<MeshRenderer>().GetSharedMaterials(_targetMaterials);
            _targetMaterials.Add(SliceMaterial);
            
            PostSlicing(mesh1, _candidates[i].transform, force + normal * SlashForce.x, torqueAxis * SlashTorque);
            PostSlicing(mesh2, _candidates[i].transform, force - normal * SlashForce.x, -torqueAxis * SlashTorque);

            rootTransform.gameObject.SetActive(false);
            rootTransform.GetComponent<AIBrain>()?.OnDeath();
            Destroy(rootTransform.gameObject);
        }
    }

    private void PostSlicing(Mesh slicedMesh, Transform candidate, Vector3 force, Vector3 torque)
    {
        var res = Instantiate(SlicePrefab, candidate.position, candidate.rotation);
        res.transform.localScale = candidate.localScale;
        
        res.MeshFilter.mesh = slicedMesh;
        res.Collider.sharedMesh = slicedMesh;
        res.Renderer.SetMaterials(_targetMaterials);
        
        res.Rigidbody.AddForce(force, ForceMode.VelocityChange);
        res.Rigidbody.AddTorque(torque, ForceMode.VelocityChange);
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