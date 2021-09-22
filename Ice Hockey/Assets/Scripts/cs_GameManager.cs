using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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



    private void Awake()
    {
        _uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<cs_UIManager>();
        _audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<cs_AudioManager>();
        _waveManager = GameObject.FindGameObjectWithTag("WaveManager").GetComponent<cs_WaveManager>();

        SpawnPlayer();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnPlayer()
    {
        GameObject _player = Instantiate(_playerPrefab, Vector2.zero, Quaternion.identity);
        _playerController = _player.GetComponent<cs_PlayerController>();

        _savedPucks = _playerController.PuckCount;
        _savedLives = _playerController.PlayerLives;
    }

    private void CheckPoint()
    {

    }

    private void GameOver()
    {

    }

    private void OnEnable()
    {
        cs_PlayerController.s_PlayerDied += GameOver;
    }

    private void OnDisable()
    {
        cs_PlayerController.s_PlayerDied -= GameOver;
    }
}
