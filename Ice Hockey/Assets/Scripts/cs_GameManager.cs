using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    private cs_PlayerController _playerController;
    private cs_UIManager _uiManager;
    private cs_AudioManager _audioManager;
    private cs_WaveManager _waveManager;
    

    private bool _gameIsActive = true;
    private int _savedPucks;
    private int _savedLives;
    private int _savedWave;

    public static event Action<int> s_StartWaves;
    public static event Action<int, float, int, int> s_ResetUI;
    public static event Action s_SetupCamera;
    public static event Action s_ClearGoals;
    public static event Action s_Reset;

    private void Awake()
    {
        _uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<cs_UIManager>();
        _audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<cs_AudioManager>();
        _waveManager = GameObject.FindGameObjectWithTag("WaveManager").GetComponent<cs_WaveManager>();

        SpawnPlayer();
    }


    private void SpawnPlayer()
    {
        GameObject _player = Instantiate(_playerPrefab, Vector2.zero, Quaternion.identity);
        _playerController = _player.GetComponent<cs_PlayerController>();

        _savedPucks = _playerController.PuckCount;
        _savedLives = _playerController.PlayerLives;

        s_SetupCamera?.Invoke();
        s_StartWaves?.Invoke(_savedWave);
        s_ResetUI?.Invoke(_savedWave + 1, 0f, _savedPucks, _savedLives);
    }

    private void CheckPoint(int _waveIndex)
    {
        _savedWave = _waveIndex;
        _savedLives = _playerController.PlayerLives;
        _playerController._canSuperMode = true;
        _playerController.AbilityCover(2, false);
    }

    private void GameOver(GameObject _player)
    {
        Debug.Log("GameOver");
        StartCoroutine(EnableDisablePlayer(_player, false, 0f));
        _player.transform.position = Vector2.zero;
        StartCoroutine(EnableDisablePlayer(_player, true, 3f));
    }

    private IEnumerator EnableDisablePlayer(GameObject _player, bool _state, float _time)
    {
        yield return new WaitForSeconds(_time);
        _player.GetComponent<cs_PlayerController>().enabled = _state;
        _player.GetComponent<Collider2D>().enabled = _state;
        _player.GetComponent<Rigidbody2D>().simulated = _state;
        _player.GetComponent<SpriteRenderer>().enabled = _state;

        foreach (Transform _child in _player.transform)
        {
            _child.gameObject.SetActive(_state);
        }

        if (!_state)
        {
            s_ClearGoals?.Invoke();
            s_Reset?.Invoke();
        }
        else
        {
            s_StartWaves?.Invoke(_savedWave);
            _playerController.PuckCount = _savedPucks;
            _playerController.PlayerLives = _savedLives;
            _playerController._canSuperMode = true;
            _playerController._canDash = true;
            _playerController._canPickUp = true;
            _playerController.DisplayPucks(1);
            s_ResetUI?.Invoke(_savedWave + 1, 0f, _savedPucks, _savedLives);
        }
    }

    private void ResetUI(int _waveIndex, float _progress, int _playerPucks, int _playerLives)
    {
        s_ResetUI?.Invoke(_waveIndex, _progress, _playerController.PuckCount, _playerController.PlayerLives);
    }

    private void OnEnable()
    {
        cs_PlayerController.s_PlayerDied += GameOver;
        cs_WaveController.s_ResetUI += ResetUI;
        cs_WaveController.s_CheckpointWave += CheckPoint;

    }

    private void OnDisable()
    {
        cs_PlayerController.s_PlayerDied -= GameOver;
        cs_WaveController.s_ResetUI -= ResetUI;
        cs_WaveController.s_CheckpointWave -= CheckPoint;
    }
}
