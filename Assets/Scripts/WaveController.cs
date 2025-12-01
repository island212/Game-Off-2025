using System;
using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class EnemySpawn
{
    public GameObject Prefab;
    public int Count;
}

[System.Serializable]
public class Wave
{
    public EnemySpawn[] Enemies;
}

public class WaveController : MonoBehaviour
{
    public Transform[] SpawnPoints;
    public Wave[] Waves;
    
    [Header("UI Elements")]
    public TextMeshProUGUI CurrentWaveText;
    public TextMeshProUGUI EnemiesRemainingText;
    
    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = true;
    private int lastSpawnPointIndex = -1;
    
    private void Start()
    {
        UpdateUI();
    }
    
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
        enemiesAlive++;
        UpdateUI();
    }

    private void OnEnemyDied()
    {
        enemiesAlive--;
        UpdateUI();
        
        if(enemiesAlive <= 0 && waveInProgress)
        {
            waveInProgress = false;
            currentWaveIndex++;
            
            if(currentWaveIndex < Waves.Length)
            {
                StartNextWave();
            }
            else
            {
                GameEvent.RaisePlayerWin();
            }
        }
    }
    
    private void StartNextWave()
    {
        if(currentWaveIndex >= Waves.Length || SpawnPoints.Length == 0)
            return;
            
        waveInProgress = true;
        StartCoroutine(SpawnWaveWithDelay());
    }
    
    private IEnumerator SpawnWaveWithDelay()
    {
        Wave currentWave = Waves[currentWaveIndex];
        
        foreach(EnemySpawn enemySpawn in currentWave.Enemies)
        {
            for(int i = 0; i < enemySpawn.Count; i++)
            {
                int spawnPointIndex = GetRandomSpawnPointIndex();
                Transform spawnPoint = SpawnPoints[spawnPointIndex];
                lastSpawnPointIndex = spawnPointIndex;
                
                Instantiate(enemySpawn.Prefab, spawnPoint.position, spawnPoint.rotation);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
    
    private int GetRandomSpawnPointIndex()
    {
        if(SpawnPoints.Length == 1)
            return 0;
            
        int index;
        do
        {
            index = UnityEngine.Random.Range(0, SpawnPoints.Length);
        }
        while(index == lastSpawnPointIndex);
        
        return index;
    }
    
    private void UpdateUI()
    {
        if(CurrentWaveText != null)
        {
            CurrentWaveText.text = $"Wave: {currentWaveIndex + 1} / {Waves.Length}";
        }
        
        if(EnemiesRemainingText != null)
        {
            EnemiesRemainingText.text = $"Enemies: {enemiesAlive}";
        }
    }
}
