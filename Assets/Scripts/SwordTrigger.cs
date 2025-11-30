using System;
using UnityEngine;
using UnityEngine.Events;

public class SwordTrigger : MonoBehaviour
{
    public UnityEvent<Collider> OnSwordHit;
    
    private void OnTriggerEnter(Collider other)
    {
        OnSwordHit?.Invoke(other);
    }
}
