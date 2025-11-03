using System;
using System.Collections;
using UnityEngine;

public class Sliceable : MonoBehaviour
{
    public Collider Collider;
    public MeshFilter MeshFilter;
    public Rigidbody Rigidbody;
    
    public void OnSlice(Vector3 force, Vector3 torque, ForceMode mode)
    {
        Destroy(Collider);
        var meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.sharedMesh = MeshFilter.sharedMesh;
        Collider = meshCollider;
        
        Rigidbody.AddForce(force, mode);
        Rigidbody.AddTorque(torque, mode);
    }

    private void Reset()
    {
        Collider = GetComponent<Collider>();
        MeshFilter = GetComponent<MeshFilter>();
        Rigidbody = GetComponent<Rigidbody>();
    }
}