using System;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController LocalPlayer;

    private void Awake()
    {
        Assert.IsNull(LocalPlayer);
        LocalPlayer = this;
    }

    private void OnDestroy()
    {
        LocalPlayer = null;
    }

    public void Hit(Vector3 velocity)
    {
        GetComponent<FirstPersonController>().enabled = false;
        GetComponent<CharacterController>().enabled = false;
        var rigid = gameObject.GetOrAddComponent<Rigidbody>();
        rigid.AddForce(velocity, ForceMode.VelocityChange);
        
        GameEvent.RaiseGameOver();
    }
}
