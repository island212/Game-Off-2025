using System;

public static class GameEvent
{
    public static event Action OnGameOver;
    
    public static void RaiseGameOver()
    {
        OnGameOver?.Invoke();
    }
}