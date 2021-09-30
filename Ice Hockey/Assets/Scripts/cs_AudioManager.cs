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
        if (!_audioSourceEffects.isPlaying)
        {
            _audioSourceEffects.clip = _audioClip;
            _audioSourceEffects.Play();
        }
        else
        {
            AudioSource _tempSource = gameObject.AddComponent<AudioSource>();
            _tempSource.volume = _audioSourceEffects.volume;
            _tempSource.clip = _audioClip;
            _tempSource.Play();
            Destroy(_tempSource, _tempSource.clip.length);
        }
    }

    private void SetVolumes(float _masterVolume, float _musicVolume, float _effectsVolume)
    {
        _audioSourceMusic.volume = _musicVolume * _masterVolume;
        _audioSourceEffects.volume = _effectsVolume * _masterVolume;
    }

    private void OnEnable()
    {
        cs_Puck.s_pickUpSound += PlayAudioClip;
        cs_PlayerController.s_SoundEffects += PlayAudioClip;
        cs_PlayerController.s_PlayerEffects += PlayAudioClip;
        cs_UIManager.s_SetVolumes += SetVolumes;
        cs_PuckGoal.s_GoalPickupAudio += PlayAudioClip;
        cs_WaveController.s_StartSoundEffect += PlayAudioClip;
        cs_Enemy.s_HitEffect += PlayAudioClip;
    }
    private void OnDisable()
    {
        cs_Puck.s_pickUpSound -= PlayAudioClip;
        cs_PlayerController.s_SoundEffects -= PlayAudioClip;
        cs_PlayerController.s_PlayerEffects -= PlayAudioClip;
        cs_UIManager.s_SetVolumes -= SetVolumes;
        cs_PuckGoal.s_GoalPickupAudio -= PlayAudioClip;
        cs_WaveController.s_StartSoundEffect -= PlayAudioClip;
        cs_Enemy.s_HitEffect -= PlayAudioClip;
    }
}
