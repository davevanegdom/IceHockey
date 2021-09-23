using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cs_UIManager : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void UpdateWaveIndex(int _waveIndex)
    {

    }

    private void UpdateWaveProgress(float progress)
    {

    }

    private void UpdatePlayerPuckCount(int _puckCount)
    {

    }

    private void UpdateCollectedPuckCount(int _puckCount)
    {

    }

    private void UpdateTimer(string _time)
    {

    }

    private void OnEnable()
    {
        cs_WaveManager.s_SetWaveIndex += UpdateWaveIndex;
        cs_WaveController.s_SetWaveProgress += UpdateWaveProgress;
        cs_PuckGoal.s_SetCollectedPucks += UpdateCollectedPuckCount;
        cs_PlayerController.s_UpdatePlayerPucks += UpdatePlayerPuckCount;
    }

    private void OnDisable()
    {
        cs_WaveManager.s_SetWaveIndex -= UpdateWaveIndex;
        cs_WaveController.s_SetWaveProgress -= UpdateWaveProgress;
        cs_PuckGoal.s_SetCollectedPucks -= UpdateCollectedPuckCount;
        cs_PlayerController.s_UpdatePlayerPucks += UpdatePlayerPuckCount;
    }
}
