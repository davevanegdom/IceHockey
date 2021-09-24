using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class cs_UIManager : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _waveText;
    [SerializeField] private TMPro.TextMeshProUGUI _progressText;
    [SerializeField] private Slider _progressbar;

    [SerializeField] private Transform _heartsPanelParent;
    [SerializeField] private GameObject _playerHeartIcon;
    private int _savedLiveIndex;
    [SerializeField] private TMPro.TextMeshProUGUI _playerPuckCount;

    [SerializeField] private TMPro.TextMeshPro _leftGoalText;
    [SerializeField] private TMPro.TextMeshPro _rightGoalText;

    [SerializeField] private Slider _masterVolume;
    [SerializeField] private Slider _musicVolume;
    [SerializeField] private Slider _effectsVolume;


    private void UpdateWaveIndex(int _waveIndex)
    {
        _waveText.text = "WAVE " + _waveIndex;
    }

    private void UpdateWaveProgress(float _progress)
    {
        _progressText.text = (_progress * 100).ToString() + "%";
        _progressbar.value = _progress;
    }

    private void UpdatePlayerPuckCount(int _puckCount)
    {
        _playerPuckCount.text = _puckCount.ToString();
    }

    private void UpdateCollectedPuckCount(int _puckCount, int _identifier)
    {
      
    }

    private void ExitGame()
    {

    }

    private void OpenMenu()
    {

    }

    private void CloseMenu()
    {

    }

    private void UpdateSoundSettings()
    {

    }

    private void UpdatePlayerLives(int _playerLives)
    {
        Debug.Log(_playerLives);
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

    private void OnEnable()
    {
        cs_WaveManager.s_SetWaveIndex += UpdateWaveIndex;
        cs_WaveController.s_SetWaveProgress += UpdateWaveProgress;
        cs_PuckGoal.s_SetCollectedPucks += UpdateCollectedPuckCount;
        cs_PlayerController.s_UpdatePlayerPucks += UpdatePlayerPuckCount;
        cs_PlayerController.s_TakeDamage += UpdatePlayerLives;
        cs_GameManager.s_ResetUI += ResetUI;
        cs_WaveController.s_ResetUI += ResetUI;
    }

    private void OnDisable()
    {
        cs_WaveManager.s_SetWaveIndex -= UpdateWaveIndex;
        cs_WaveController.s_SetWaveProgress -= UpdateWaveProgress;
        cs_PuckGoal.s_SetCollectedPucks -= UpdateCollectedPuckCount;
        cs_PlayerController.s_UpdatePlayerPucks -= UpdatePlayerPuckCount;
        cs_PlayerController.s_TakeDamage -= UpdatePlayerLives;
        cs_GameManager.s_ResetUI -= ResetUI;
        cs_WaveController.s_ResetUI -= ResetUI;
    }
}
