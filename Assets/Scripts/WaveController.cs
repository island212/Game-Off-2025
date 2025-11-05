using System;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    public int WaveCount = -1;
    public int WaveSize = 1;
    
    public GameObject[] WavePrefabs;
    
    private void OnEnable()
    {
        GameEvent.OnEnemySpawned += OnEnemySpawned;
        GameEvent.OnEnemyDied += OnEnemyDied;
    }
    
    private void OnDisable()
    {
        GameEvent.OnEnemySpawned -= OnEnemySpawned;
        GameEvent.OnEnemyDied -= OnEnemyDied;
    }

    private void OnEnemySpawned()
    {
        WaveSize++;
    }

    private void OnEnemyDied()
    {
        WaveSize--;
        if(WaveSize > 0)
            return;
        
        WaveCount++;

        if(WaveCount < WavePrefabs.Length)
        {
            Instantiate(WavePrefabs[WaveCount]);
        }
        else
        {
            GameEvent.RaisePlayerWin();
        }
    }
}
