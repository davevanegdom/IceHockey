using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class cs_WaveController : MonoBehaviour
{
    private cs_WaveManager _waveManager;
    public GameObject EnemyPrefab;
    public LayerMask SpawnLayer;
    private int _waveIndex;
    private int _enemiesToSpawn;
    private int _waveTime;
    private float _timeInterval;
    private int _enemiesKilled;
    private int _enemiesSpawned;
    private bool _isCheckpointWave;
    [SerializeField] private GameObject _waveIndicator;
    [SerializeField] private GameObject _waveCheckpointIndicator;
    [SerializeField] private AudioClip _startSoundEffect;

    public static event Action<int> s_WaveEnded;
    public static event Action<float> s_SetWaveProgress;
    public static event Action<int> s_CheckpointWave;
    public static event Action<int, float, int, int> s_ResetUI;
    public static event Action<AudioClip> s_StartSoundEffect;

    private void Start()
    {
        _waveManager = GetComponent<cs_WaveManager>();
        _waveIndicator.SetActive(false);
        _waveCheckpointIndicator.SetActive(false);
    }

    public void SpawnEnemies(int _newWaveIndex, int _enemiesCount, int _newWaveTime, bool _isCheckpoint)
    {
        _waveIndex = _newWaveIndex;
        _enemiesToSpawn = _enemiesCount;
        _waveTime = _newWaveTime;
        _timeInterval = _waveTime / _enemiesToSpawn;
        _enemiesKilled = 0;
        _enemiesSpawned = 0;
        _isCheckpointWave = _isCheckpoint;

        StartCoroutine(WaveAnnouncement());

        s_StartSoundEffect(_startSoundEffect);
        s_ResetUI?.Invoke(_waveIndex + 1, 0, 0, 0);
        SpawnEnemy();
        StartCoroutine(WaveTimer(_waveTime));
        Debug.Log("Wave " + _waveIndex + ": spawn " + _enemiesToSpawn + " enemies over the course of the next " + _waveTime + " seconds");
    }

    private void SpawnEnemy()
    {
        _enemiesSpawned++;

        if(_enemiesSpawned <= _enemiesToSpawn)
        {
            //Spawn player at the given point
            Instantiate(EnemyPrefab, enemySpawnPosition(), Quaternion.identity);
            _timeInterval = _waveTime / _enemiesToSpawn;
            StartCoroutine(SpawnTimer(_timeInterval));
        }
    }

    private void WaveProgress()
    {
        _enemiesKilled++;
        int _enemiesToKill = _waveManager.Waves[_waveManager.WaveIndex].NumberOfEnemies.AmountOfEnemies;
        float _progress = (float)_enemiesKilled / (float)_enemiesToSpawn;
        s_SetWaveProgress?.Invoke(_progress);


        if (_enemiesKilled == _enemiesToSpawn)
        {
            if (_isCheckpointWave)
            {
                s_CheckpointWave?.Invoke(_waveIndex);
            }
            s_WaveEnded?.Invoke(_waveIndex);
            Debug.Log("Wave Ended");
            StopAllCoroutines();
        }
    }

    //Get a point on an edge collider
    Vector2 enemySpawnPosition()
    {
        RaycastHit2D _hit = Physics2D.Raycast(Vector2.zero, new Vector2((UnityEngine.Random.Range(-1f, 1f)), (UnityEngine.Random.Range(-1f, 1f))), 100f, SpawnLayer);

        if (_hit.collider == null)
        {
            Debug.Log("No collider was found");
        }
        return _hit.point;
    }

    public IEnumerator SpawnTimer(float _currentTimeInterval)
    {
        float _time = _currentTimeInterval;

        while (_time > 0)
        {
            _time -= Time.deltaTime;
            yield return null;
        }
        //spawn Enemy
        SpawnEnemy();
    }

    public IEnumerator WaveTimer(float _waveTimer)
    {
        float _time = _waveTimer;

        while (_time > 0)
        {
            _time -= Time.deltaTime;
            yield return null;
        }

        //s_WaveEnded?.Invoke(_waveIndex);
        StopAllCoroutines();
    }

    public IEnumerator WaveAnnouncement()
    {
        _waveIndicator.GetComponent<TextMeshPro>().text = "WAVE " + (_waveIndex + 1);
        _waveIndicator.SetActive(true);
        if (_isCheckpointWave)
        {
            _waveCheckpointIndicator.SetActive(true);
        }
        yield return new WaitForSeconds(2f);
        _waveIndicator.SetActive(false);
        _waveCheckpointIndicator.SetActive(false);
    }
    private void OnEnable()
    {
        cs_WaveManager.s_SpawnWave += SpawnEnemies;
        cs_Enemy.s_EnemyDied += WaveProgress;
    }

    private void OnDisable()
    {
        cs_WaveManager.s_SpawnWave -= SpawnEnemies;
        cs_Enemy.s_EnemyDied -= WaveProgress;
    }

}
