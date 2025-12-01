using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Hanzzz.MeshSlicerFree;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerSlash : MonoBehaviour
{
    private static readonly int BlendAnimationHash = Animator.StringToHash("Blend");
    private static readonly int AttackAnimationHash = Animator.StringToHash("Attack");

    public InputActionReference AttackAction;
    public Vector2 SlashForce = new (10f, 30f);
    public float SlashTorque = 10f;

    [Header("Animation")]
    public Animator SlashAnim;
    public AnimationCurve BlendAnimationCurve;
    public AnimationCurve AngleAnimationCurve;
    
    [Header("Slice Mesh")]
    public SliceableReference SlicePrefab;
    public Material SliceMaterial;
    
    [Header("Collision")]
    public SwordTrigger SwordTrigger;
    
    [Header("Audio")]
    public AudioClip[] SwordHitSounds;
    public AudioClip[] SwordMissSounds;
    
    private readonly MeshSlicer _meshSlicer = new();
    private readonly List<Material> _targetMaterials = new();
    private readonly List<int> _slicedIds = new();
    
    private float _enterSliceTime;
    
    private IdleState _idleAnimationState;
    
    private void Start()
    {
        SwordTrigger.OnSwordHit.AddListener(OnSwordHit);
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
    }

    private void OnAttackEnd(InputAction.CallbackContext ctx)
    {
        if(_enterSliceTime == 0)
            return;
        
        _enterSliceTime = 0;
        
        SlashAnim.SetTrigger(AttackAnimationHash);
    }

    // private IEnumerator RotateHandToTargetAnimation(Vector3 exitLookForward)
    // {
    //     var dir = (exitLookForward - _enterLookForward).normalized;
    //
    //     var projected = Vector3.ProjectOnPlane(dir, transform.forward);
    //     projected.z = 0;
    //     projected.Normalize();
    //     var angle = Vector3.SignedAngle(Vector3.up, -projected, Vector3.forward);
    //     
    //     if(angle < 0)
    //     {
    //         angle += 360;
    //     }
    //     
    //     var time = 0f;
    //     var animatedTransform = SlashAnim.transform;
    //     var currentAngle = animatedTransform.eulerAngles.z;
    //     do
    //     {
    //         time += Time.deltaTime;
    //         currentAngle = Mathf.LerpAngle(currentAngle, angle, AngleAnimationCurve.Evaluate(time)) % 360;
    //         if(currentAngle < 0)
    //         {
    //             currentAngle += 360;
    //         }
    //         
    //         animatedTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);
    //         yield return null;
    //     } 
    //     while (Mathf.Abs(currentAngle - angle) > 0.01f);
    //
    //     animatedTransform.localRotation = Quaternion.Euler(0, 0, angle);
    //     
    //     SlashAnim.SetTrigger(AttackAnimationHash);
    //     
    //     //Slice(exitBasePosition, exitTipPosition);
    // }

    private void OnSwordHit(Collider other)
    {
        var swordTrans = SwordTrigger.transform;
        var normal = -swordTrans.right;
        
        var skinnedMeshRenderer = other.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            skinnedMeshRenderer = other.GetComponent<SkinnedMeshRenderer>();
        }
        
        var rootTransform = other.transform.root;
        
        Assert.IsTrue(skinnedMeshRenderer != null, $"Missing SkinnedMeshRenderer on {other.name} or its children. Sliceable must have a SkinnedMeshRenderer. Parent was {rootTransform.name}");
        
        var bakedMesh = new Mesh();
        skinnedMeshRenderer.BakeMesh(bakedMesh);
        
        var v0 = swordTrans.position;
        var v1 = v0 + swordTrans.up;
        var v2 = v0 + swordTrans.forward;
        
        var (mesh1, mesh2) = _meshSlicer.Slice((v0, v1, v2), bakedMesh,
            skinnedMeshRenderer.transform, true);
        
        if(mesh1 == null || mesh2 == null)
            return;
        
        var direction = -swordTrans.forward;
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
            SoundFXManager.Instance.PlayRandomSound(SwordMissSounds, swordTrans);
        }
        
        rootTransform.gameObject.SetActive(false);
        rootTransform.GetComponent<AIBrain>()?.OnDeath();
        Destroy(rootTransform.gameObject);
        
        Destroy(bakedMesh);
    }

    private void PostSlicing(Mesh slicedMesh, Transform candidate, Vector3 force, Vector3 torque)
    {
        Profiler.BeginSample("PostSlicing");
        var res = Instantiate(SlicePrefab, candidate.position, candidate.rotation);
        res.transform.localScale = candidate.localScale;
        
        res.MeshFilter.mesh = slicedMesh;
        res.Renderer.SetMaterials(_targetMaterials);
        
        res.Rigidbody.AddForce(force, ForceMode.VelocityChange);
        res.Rigidbody.AddTorque(torque, ForceMode.VelocityChange);
        
        Destroy(res.gameObject, 60f);
        Profiler.EndSample();
    }
}