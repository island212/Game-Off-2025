using System;
using UnityEngine;

public class GameOverView : MonoBehaviour
{
    public GameObject GameOverPanel;
    
    private void OnEnable()
    {
        GameEvent.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameEvent.OnGameOver -= OnGameOver;
    }
    
    private void OnGameOver()
    {
        GameOverPanel.SetActive(true);
    }
}
