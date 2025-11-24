using StarterAssets;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerController : MonoBehaviour
{
    public static PlayerController LocalPlayer;

    private void Awake()
    {
        Assert.IsNull(LocalPlayer);
        LocalPlayer = this;
    }

    private void OnEnable()
    {
        GameEvent.OnPause += OnPause;
        GameEvent.OnResume += OnResume;
    }

    private void OnDisable()
    {
        GameEvent.OnPause -= OnPause;
        GameEvent.OnResume -= OnResume;
    }

    private void OnDestroy()
    {
        LocalPlayer = null;
    }

    public void Hit(Vector3 velocity)
    {
        GetComponent<FirstPersonController>().enabled = false;
        GetComponent<CharacterController>().enabled = false;
        var rigid = gameObject.GetComponent<Rigidbody>();
        if(rigid == null)
            rigid = gameObject.AddComponent<Rigidbody>();
        
        rigid.interpolation = RigidbodyInterpolation.Interpolate;
        rigid.AddForce(velocity, ForceMode.VelocityChange);

        GameEvent.RaiseGameOver();
    }
    
    private void OnPause()
    {
        GetComponent<FirstPersonController>().enabled = false;
    }
    
    private void OnResume()
    {
        GetComponent<FirstPersonController>().enabled = true;
    }
}
