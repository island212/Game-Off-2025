using System;
using UnityEngine;

public class WinnerViews : MonoBehaviour
{
    public GameObject WinPanel;
    
    private void OnEnable()
    {
        GameEvent.OnPlayerWin += OnPlayerWin;
    }

    private void OnDisable()
    {
        GameEvent.OnPlayerWin -= OnPlayerWin;
    }

    private void OnPlayerWin()
    {
        WinPanel.SetActive(true);
    }
}
