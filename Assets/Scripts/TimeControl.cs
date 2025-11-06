using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeControl : MonoBehaviour
{
    // public float TimeToAccelerate = 1f;
    // public float TimeToDecelerate = 1f;
    // public float MinTimeScale = 0.1f;
    //
    // private double _lastTimeMoved;
    // private double _lastTimeIdle;

    public float IdleTimeScale = 0.1f;

    private Vector2 _move;
    private bool _isPaused = false;
    
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
    
    void Update()
    {
        if (_isPaused)
            return;
            
        Time.timeScale = _move.sqrMagnitude > 0.01f ? 1f : IdleTimeScale;

        // if(_move.sqrMagnitude <= 0.01f)
        // {
        //     _lastTimeIdle = Time.unscaledTimeAsDouble;
        // }
        // else
        // {
        //     _lastTimeMoved = Time.unscaledTimeAsDouble;
        // }
        //
        // if(_lastTimeIdle < _lastTimeMoved)
        // {
        //     var time = _lastTimeMoved - _lastTimeIdle;
        //     var percent = math.clamp(time / TimeToAccelerate, 0, 1f);
        //
        //     Time.timeScale = math.sin((float)percent * math.PIHALF);
        // }
        // else
        // {
        //     var time = _lastTimeIdle - _lastTimeMoved;
        //     var percent = math.clamp(time / TimeToDecelerate, 0, 1f);
        //     
        //     Time.timeScale = math.max(math.cos((float)percent * math.PIHALF), MinTimeScale);
        // }
    }

    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
    }

    private void OnPause()
    {
        _isPaused = true;
    }
    
    private void OnResume()
    {
        _isPaused = false;
    }
}
