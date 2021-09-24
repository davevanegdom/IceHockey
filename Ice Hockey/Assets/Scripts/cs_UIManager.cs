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
        if(_identifier == 0)
        {
            //_leftGoalText.text = _puckCount.ToString();
        }
        else
        {
            //_rightGoalText.text = _puckCount.ToString();
        }
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

        for (int i = 0; i < _playerLives; i++)
        {
            Instantiate(_playerHeartIcon, _heartsPanelParent);
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
