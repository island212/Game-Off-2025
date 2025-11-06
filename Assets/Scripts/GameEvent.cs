using System;

public static class GameEvent
{
    public static event Action OnWaveStarted;
    public static event Action OnPlayerWin;
    public static event Action OnGameOver;
    
    public static event Action OnEnemySpawned;
    public static event Action OnEnemyDied;
    public static event Action OnPause;
    public static event Action OnResume;
    
    public static void RaiseGameOver()
    {
        OnGameOver?.Invoke();
    }

    public static void RaiseEnemySpawned()
    {
        OnEnemySpawned?.Invoke();
    }
    
    public static void RaiseEnemyDied()
    {
        OnEnemyDied?.Invoke();
    }
    
    public static void RaisePlayerWin()
    {
        OnPlayerWin?.Invoke();
    }

    public static void RaiseWaveStarted()
    {
        OnWaveStarted?.Invoke();
    }

    public static void RaisePause()
    {
        OnPause?.Invoke();
    }
    
    public static void RaiseResume()
    {
        OnResume?.Invoke();
    }
}