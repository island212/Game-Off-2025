using System.Collections;
using System.Collections.Generic;
using Hanzzz.MeshSlicerFree;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerSlash : MonoBehaviour
{
    private static readonly int BlendAnimationHash = Animator.StringToHash("Blend");
    private static readonly int AttackAnimationHash = Animator.StringToHash("Attack");

    public InputActionReference AttackAction;
    public Vector2 SlashForce = new Vector2(10f, 30f);
    public float SlashTorque = 10f;

    [Header("Animation")]
    public Animator SlashAnim;
    [FormerlySerializedAs("BlendCharge")] public AnimationCurve BlendAnimationCurve;
    public AnimationCurve AngleAnimationCurve;
    
    [Header("Slice Mesh")]
    public SliceableReference SlicePrefab;
    public Material SliceMaterial;
    
    [Header("Collision")]
    public Transform RayOrigin;
    public LayerMask RaycastMask;
    public float RaycastDistance = 3f;
    
    [Header("Audio")]
    public AudioClip[] SwordHitSounds;
    public AudioClip[] SwordMissSounds;

    private readonly Collider[] _candidates = new Collider[32];
    private readonly MeshSlicer _meshSlicer = new();
    private readonly List<Material> _targetMaterials = new();

    private float _enterSliceTime;
    private Vector3 _enterBasePosition, _enterTipPosition;
    
    private IdleState _idleAnimationState;
    private Coroutine _sliceCoroutine;
    
    private void Start()
    {
        _idleAnimationState = SlashAnim.GetBehaviour<IdleState>();
    }

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

    private void Update()
    {
        var maxTime = BlendAnimationCurve.keys[BlendAnimationCurve.length - 1].time;
        var timeSinceEnter = _enterSliceTime > 0 ? Time.timeSinceLevelLoad - _enterSliceTime : 0;

        var normalizeTime = math.clamp(timeSinceEnter / maxTime, 0, 1);
        
        SlashAnim.SetFloat(BlendAnimationHash, BlendAnimationCurve.Evaluate(normalizeTime));
    }

    private void OnAttackStart(InputAction.CallbackContext ctx)
    {
        if(!_idleAnimationState.IsInState)
            return;
        
        _enterSliceTime = Time.timeSinceLevelLoad;
        _enterBasePosition = RayOrigin.position;
        _enterTipPosition = RayOrigin.position + RayOrigin.forward * RaycastDistance;
    }

    private void OnAttackEnd(InputAction.CallbackContext ctx)
    {
        if(_enterSliceTime == 0)
            return;
        
        _enterSliceTime = 0;

        if(_sliceCoroutine != null)
            StopCoroutine(_sliceCoroutine);
        
        var exitBasePosition = RayOrigin.position;
        var exitTipPosition = RayOrigin.position + RayOrigin.forward * RaycastDistance;
        
        _sliceCoroutine = StartCoroutine(RotateHandToTargetAnimation(exitBasePosition, exitTipPosition));
    }

    private IEnumerator RotateHandToTargetAnimation(Vector3 exitBasePosition, Vector3 exitTipPosition)
    {
        var dir = (exitBasePosition - _enterBasePosition).normalized;

        var projected = Vector3.ProjectOnPlane(dir, transform.forward);
        projected.z = 0;
        projected.Normalize();
        var angle = Vector3.SignedAngle(Vector3.up, -projected, Vector3.forward);
        
        if(angle < 0)
        {
            angle += 360;
        }
        
        var time = 0f;
        var animatedTransform = SlashAnim.transform;
        var currentAngle = animatedTransform.eulerAngles.z;
        do
        {
            time += Time.deltaTime;
            currentAngle = Mathf.LerpAngle(currentAngle, angle, AngleAnimationCurve.Evaluate(time)) % 360;
            if(currentAngle < 0)
            {
                currentAngle += 360;
            }
            
            animatedTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        } 
        while (Mathf.Abs(currentAngle - angle) > 0.01f);

        animatedTransform.localRotation = Quaternion.Euler(0, 0, angle);
        
        SlashAnim.SetTrigger(AttackAnimationHash);
        
        Slice(exitBasePosition, exitTipPosition);
    }
    
    private void Slice(Vector3 exitBasePosition, Vector3 exitTipPosition)
    {
        var count = GetCollidersIntersectingPlane(_enterBasePosition, _enterTipPosition, exitBasePosition,
            exitTipPosition);

        var normal = Vector3.Cross(_enterTipPosition - _enterBasePosition, exitTipPosition - _enterBasePosition);
        normal.Normalize();

        Debug.DrawRay(_enterBasePosition, _enterTipPosition, Color.red, 10f);
        Debug.DrawRay(_enterBasePosition, exitTipPosition, Color.red, 10f);
        Debug.DrawRay(_enterBasePosition, normal, Color.blue, 10f);

        // Play miss sound if nothing was hit
        if (count == 0)
        {
            if (SwordMissSounds != null && SwordMissSounds.Length > 0)
            {
                SoundFXManager.Instance.PlayRandomSound(SwordMissSounds, RayOrigin);
            }
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var skinnedMeshRenderer = _candidates[i].GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer == null)
            {
                skinnedMeshRenderer = _candidates[i].GetComponent<SkinnedMeshRenderer>();
            }
            
            var rootTransform = _candidates[i].transform.root;

            Assert.IsTrue(skinnedMeshRenderer != null, $"Missing SkinnedMeshRenderer on {_candidates[i].name} or its children. Sliceable must have a SkinnedMeshRenderer. Parent was {rootTransform.name}");
            
            var bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            
            var (mesh1, mesh2) = _meshSlicer.Slice((_enterBasePosition, _enterTipPosition, exitTipPosition), bakedMesh,
                skinnedMeshRenderer.transform, true);
            
            if(mesh1 == null || mesh2 == null)
                continue;
            
            var direction = (exitTipPosition - _enterTipPosition).normalized;
            var force = direction * SlashForce.y;
            var torqueAxis = Vector3.Cross(direction, normal);
            
            skinnedMeshRenderer.GetSharedMaterials(_targetMaterials);
            _targetMaterials.Add(SliceMaterial);
            
            PostSlicing(mesh1, skinnedMeshRenderer.transform, force + normal * SlashForce.x, torqueAxis * SlashTorque);
            PostSlicing(mesh2, skinnedMeshRenderer.transform, force - normal * SlashForce.x, -torqueAxis * SlashTorque);

            // Play random sword hit sound
            if (SwordHitSounds != null && SwordHitSounds.Length > 0)
            {
                SoundFXManager.Instance.PlayRandomSound(SwordHitSounds, skinnedMeshRenderer.transform);
                SoundFXManager.Instance.PlayRandomSound(SwordMissSounds, RayOrigin);
            }

            rootTransform.gameObject.SetActive(false);
            rootTransform.GetComponent<AIBrain>()?.OnDeath();
            Destroy(rootTransform.gameObject);
            
            Destroy(bakedMesh);
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