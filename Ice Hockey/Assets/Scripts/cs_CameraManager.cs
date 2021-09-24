using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class cs_CameraManager : MonoBehaviour
{
    private CinemachineVirtualCamera _cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin _cinemachineShake;
    private GameObject _player;
    private Rigidbody2D _rbPlayer;
    private cs_PlayerController _playerController;
    [SerializeField] private float _cameraMinSize;
    [SerializeField] private float _cameraMaxSize;
    [SerializeField] private float _smoothRate;
    private float _cameraShakeIntensity;
    [SerializeField] private float _cameraShakeTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void LateUpdate()
    {
        float _cSize = Mathf.Lerp(_cameraMinSize, _cameraMaxSize, (_rbPlayer.velocity.magnitude / _playerController.MaxMoveSpeed));
        float _size = Mathf.Lerp(_cinemachineCamera.m_Lens.OrthographicSize, _cSize, _smoothRate * Time.deltaTime);
        _cinemachineCamera.m_Lens.OrthographicSize = _size;
    }

    private void CameraSetup()
    {
        _cinemachineCamera = GetComponent<CinemachineVirtualCamera>();
        _cinemachineShake = _cinemachineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _cinemachineCamera.m_Follow = _player.transform;
        _rbPlayer = _player.GetComponent<Rigidbody2D>();
        _playerController = _player.GetComponent<cs_PlayerController>();

        if (_cinemachineShake == null)
        {
            Debug.Log("Null");
        }
    }

    private void CameraShake(float _intensity)
    {
        StartCoroutine(ShakeTimer(_intensity));
    }

    private IEnumerator ShakeTimer(float _shakeIntensity)
    {
        _cinemachineShake.m_AmplitudeGain = _shakeIntensity;

        yield return new WaitForSeconds(_cameraShakeTime);

        _cinemachineShake.m_AmplitudeGain = 0;
    }

    private void OnEnable()
    {
        cs_GameManager.s_SetupCamera += CameraSetup;
        cs_PlayerController.s_ShakeCamera += CameraShake;
    }

    private void OnDisable()
    {
        cs_GameManager.s_SetupCamera -= CameraSetup;
        cs_PlayerController.s_ShakeCamera -= CameraShake;
    }
}
