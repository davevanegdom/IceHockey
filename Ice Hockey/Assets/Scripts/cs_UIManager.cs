using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class cs_UIManager : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _waveText;
    [SerializeField] private TMPro.TextMeshProUGUI _progressText;
    [SerializeField] private Slider _progressbar;

    [SerializeField] private Transform _heartsPanelParent;
    [SerializeField] private GameObject _playerHeartIcon;
    private int _savedLiveIndex;
    [SerializeField] private TMPro.TextMeshProUGUI _playerPuckCount;
    [SerializeField] private Animation _puckCountAnimation;
    [SerializeField] private Animation _heartAnimation;

    [SerializeField] private Slider _masterVolume;
    [SerializeField] private Slider _musicVolume;
    [SerializeField] private Slider _effectsVolume;

    [SerializeField] private GameObject _menuPanel;

    [SerializeField] private GameObject _dashCover;
    [SerializeField] private GameObject _pickUpCover;
    [SerializeField] private GameObject _superCover;

    public static event Action<float, float, float> s_SetVolumes;

    private void Start()
    {
        _dashCover.SetActive(false);
        _pickUpCover.SetActive(false);
        _superCover.SetActive(false);
    }

    private void UpdateWaveIndex(int _waveIndex)
    {
        _waveText.text = "WAVE " + _waveIndex;
    }

    private void UpdateWaveProgress(float _progress)
    {
        _progressText.text = Mathf.RoundToInt(_progress * 100).ToString() + "%";
        _progressbar.value = _progress;
    }

    private void UpdatePlayerPuckCount(int _puckCount)
    {
        _playerPuckCount.text = _puckCount.ToString();
        _puckCountAnimation.Play();
    }

    public void LoadScene(int _buildIndex)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(_buildIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenMenu()
    {
        Time.timeScale = 0f;
        cs_PlayerController _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<cs_PlayerController>();
        _playerController.enabled = false;
    }

    public void CloseMenu()
    {
        Time.timeScale = 1f;
        cs_PlayerController _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<cs_PlayerController>();
        _playerController.enabled = true;
    }

    public void UpdateSoundSettings()
    {
        float _master = _masterVolume.value;
        float _music = _musicVolume.value;
        float _effects = _effectsVolume.value;

        s_SetVolumes?.Invoke(_master, _music, _effects);
    }

    private void UpdatePlayerLives(int _playerLives)
    {
        if(_heartsPanelParent.childCount > 0)
        {
            foreach (Transform _heart in _heartsPanelParent)
            {
                Destroy(_heart.gameObject);
            }
        }

        float _interval = 35;
        float _startX = -_interval * (_playerLives / 2);

        for (int i = 0; i < _playerLives; i++)
        {
            GameObject _heartIcon = Instantiate(_playerHeartIcon, _heartsPanelParent);
            _heartIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(_startX + _interval * i, 0);
        }

        _heartAnimation.Play();
    }

    private void UpdateTimer(string _time)
    {
        
    }

    private void ResetUI(int _waveIndex, float _waveProgress, int _playerPucks, int _playerLives)
    {
        UpdateWaveIndex(_waveIndex);
        UpdateWaveProgress(_waveProgress);
        UpdatePlayerPuckCount(_playerPucks);
        UpdatePlayerLives(_playerLives);
    }

    private void AbilityCovers(int _identifier, bool _visibilty)
    {
        switch (_identifier)
        {
            case 0:
                _dashCover.SetActive(_visibilty);
                break;
            case 1:
                _pickUpCover.SetActive(_visibilty);
                break;
            case 2:
                _superCover.SetActive(_visibilty);
                break;
        }
    }

    private void ResetCovers()
    {
        AbilityCovers(0, false);
        AbilityCovers(1, false);
        AbilityCovers(2, false);
    }

    private void OnEnable()
    {
        cs_WaveManager.s_SetWaveIndex += UpdateWaveIndex;
        cs_WaveController.s_SetWaveProgress += UpdateWaveProgress;
        cs_PlayerController.s_UpdatePlayerPucks += UpdatePlayerPuckCount;
        cs_PlayerController.s_TakeDamage += UpdatePlayerLives;
        cs_GameManager.s_ResetUI += ResetUI;
        cs_WaveController.s_ResetUI += ResetUI;
        cs_PlayerController.s_AbilityCover += AbilityCovers;
        cs_GameManager.s_Reset += ResetCovers;
    }

    private void OnDisable()
    {
        cs_WaveManager.s_SetWaveIndex -= UpdateWaveIndex;
        cs_WaveController.s_SetWaveProgress -= UpdateWaveProgress;
        cs_PlayerController.s_UpdatePlayerPucks -= UpdatePlayerPuckCount;
        cs_PlayerController.s_TakeDamage -= UpdatePlayerLives;
        cs_GameManager.s_ResetUI -= ResetUI;
        cs_WaveController.s_ResetUI -= ResetUI;
        cs_PlayerController.s_AbilityCover -= AbilityCovers;
        cs_GameManager.s_Reset -= ResetCovers;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenMenu();
            _menuPanel.SetActive(true);
        }
    }
}
