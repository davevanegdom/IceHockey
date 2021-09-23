using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_WaveManager : MonoBehaviour
{
    public int WaveIndex = 0;
    public List<Wave> Waves;
    [SerializeField] private int _waveInterval;

    public static event Action<int> s_SetWaveIndex;
    public static event Action<int, int, int> s_SpawnWave;
    public static event Action<int> s_GameManagerAware;

    private void Start()
    {
        WaveIndex = 0;
    }

    private void WaveStart(int _currentWave)
    {
        WaveIndex = _currentWave;
        if(WaveIndex >= 0)
        {
            s_SpawnWave?.Invoke(WaveIndex, Waves[WaveIndex].NumberOfEnemies.AmountOfEnemies, Waves[WaveIndex].TargetTime.WaveTime);
            SetWaveIndex(WaveIndex + 1);
        }
        else
        {
            Debug.Log("Last Wave has been reached");
        }
    }

    private void WaveFinished(int _finishedWaveIndex)
    {
        WaveIndex++;
        StartCoroutine(WaveInterval());

    }

    public void SetWaveIndex(int _displayIndex)
    {
        s_SetWaveIndex?.Invoke(_displayIndex);
    }

    private IEnumerator WaveInterval()
    {
        float _time = _waveInterval;

        while (_time > 0)
        {
            _time -= Time.deltaTime;
            yield return null;
        }

        WaveStart(WaveIndex);
    }

    #region Wave Setup
    [System.Serializable]
    public class Wave
    {
        public EnemyCount NumberOfEnemies;
        public TargetTime TargetTime;
    }

    [System.Serializable]
    public class TargetTime
    {
        [Range(0, 300)] public int WaveTime;
    }

    [System.Serializable]
    public class EnemyCount
    {
        public int AmountOfEnemies;
    }
    #endregion

    private void OnEnable()
    {
        cs_WaveController.s_WaveEnded += WaveFinished;
        cs_GameManager.s_StartWaves += WaveStart;

    }

    private void OnDisable()
    {
        cs_WaveController.s_WaveEnded -= WaveFinished;
        cs_GameManager.s_StartWaves -= WaveStart;
    }
}
