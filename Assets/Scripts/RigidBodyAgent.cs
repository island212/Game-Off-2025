using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public interface IHitable
{
    void AddForce(Vector3 force, Vector3 position, ForceMode mode);
}

public class RigidBodyAgent : MonoBehaviour, IHitable
{
    [SerializeField] private float _idleVelocity = 0.1f;
    
    [SerializeField] private Rigidbody _rigid;
    [SerializeField] private NavMeshAgent _navAgent;

    private Coroutine _physicsCoroutine;
    
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

        _physicsCoroutine = StartCoroutine(PlayPhysics());
    }

    private IEnumerator PlayPhysics()
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
}
