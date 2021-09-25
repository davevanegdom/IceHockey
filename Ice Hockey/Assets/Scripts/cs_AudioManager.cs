using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class cs_AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSourceEffects;
    [SerializeField] private AudioSource _audioSourceMusic;

    public float MasterVolume;
    public float MusicVolume;
    public float EffectsVolume;

    // Start is called before the first frame update
    void Start()
    {
        SetVolumes(MasterVolume, MusicVolume, EffectsVolume);
        _audioSourceMusic.Play();
    }

    private void PlayActionMusic()
    {
        _audioSourceMusic.Play();
    }

    private void PlayAudioClip(AudioClip _audioClip)
    {
        _audioSourceEffects.clip = _audioClip;
        _audioSourceEffects.Play();
    }

    private void StopAudioClip(AudioClip _audioClip)
    {

    }

    private void SetVolumes(float _masterVolume, float _musicVolume, float _effectsVolume)
    {
        _audioSourceMusic.volume = _musicVolume * _masterVolume;
        _audioSourceEffects.volume = _effectsVolume * _masterVolume;

        Debug.Log("Set Volumes");
    }

    private void OnEnable()
    {
        cs_Puck.s_pickUpSound += PlayAudioClip;
        cs_PlayerController.s_ShootEffects += PlayAudioClip;
        cs_PlayerController.s_PlayerEffects += PlayAudioClip;
        cs_UIManager.s_SetVolumes += SetVolumes;
    }
    private void OnDisable()
    {
        cs_Puck.s_pickUpSound -= PlayAudioClip;
        cs_PlayerController.s_ShootEffects -= PlayAudioClip;
        cs_PlayerController.s_PlayerEffects -= PlayAudioClip;
        cs_UIManager.s_SetVolumes -= SetVolumes;
    }
}
