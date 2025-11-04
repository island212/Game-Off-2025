using System;
using System.Collections;
using UnityEngine;

public class SliceableReference : MonoBehaviour
{
    public MeshFilter MeshFilter;
    public MeshRenderer Renderer;
    public MeshCollider Collider;
    public Rigidbody Rigidbody;
    
    private void Reset()
    {
        Renderer = GetComponent<MeshRenderer>();
        Collider = GetComponent<MeshCollider>();
        MeshFilter = GetComponent<MeshFilter>();
        Rigidbody = GetComponent<Rigidbody>();
    }
}