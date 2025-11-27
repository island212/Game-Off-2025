using System;
using System.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Math = Unity.Mathematics.Geometry.Math;

public interface IHitable
{
    void AddForce(Vector3 force, Vector3 position, ForceMode mode);
}

public class RigidBodyAgent : MonoBehaviour, IHitable
{
    public Animator animator;

    [SerializeField] private float _idleVelocity = 0.1f;
    
    [SerializeField] private Rigidbody _rigid;
    [SerializeField] private NavMeshAgent _navAgent;
    
    [Header("Footstep Audio")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private float footstepInterval = 1f; // Time between footsteps
    [SerializeField] private float minVelocityForFootsteps = 0.2f; // Minimum velocity to play footsteps
    
    private float _nextFootstepTime;

    private Coroutine _physicsCoroutine;
    
    private void Start()
    {
        // Add random offset so enemies don't all step in sync
        _nextFootstepTime = Time.time + UnityEngine.Random.Range(0f, footstepInterval);
    }
    
    public void AddForce(Vector3 force, Vector3 position, ForceMode mode)
    {
        if(_physicsCoroutine != null)
        {
            StopCoroutine(_physicsCoroutine);
            _physicsCoroutine = null;
        }

        _navAgent.enabled = false;
        _rigid.isKinematic = false;
        _rigid.useGravity = true;
        
        _rigid.AddForceAtPosition(force, position, mode);

        _physicsCoroutine = StartCoroutine(WaitForAgentGetStable());
    }

    private IEnumerator WaitForAgentGetStable()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => _rigid.linearVelocity.sqrMagnitude < _idleVelocity * _idleVelocity);

        _navAgent.enabled = true;
        _rigid.isKinematic = true;
        _rigid.useGravity = false;
        
        transform.localRotation = Quaternion.identity;
    }

    private void Reset()
    {
        _rigid = GetComponent<Rigidbody>();
        _navAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        var velocity = _navAgent.velocity.magnitude;
        animator.SetFloat("locomotion", velocity);
        
        // Play footstep sounds when moving
        if (velocity > minVelocityForFootsteps && Time.time >= _nextFootstepTime)
        {
            PlayFootstep();
            _nextFootstepTime = Time.time + footstepInterval;
        }
    }
    
    private void PlayFootstep()
    {
        if (footstepSound == null)
            return;
            
        SoundFXManager.Instance.PlaySound(footstepSound, transform);
    }
}
