using System;
using System.Collections;
using UnityEngine;

public class SliceableReference : MonoBehaviour
{
    public MeshFilter MeshFilter;
    public MeshRenderer Renderer;
    public Rigidbody Rigidbody;
    
    private void Reset()
    {
        Renderer = GetComponent<MeshRenderer>();
        MeshFilter = GetComponent<MeshFilter>();
        Rigidbody = GetComponent<Rigidbody>();
    }
}