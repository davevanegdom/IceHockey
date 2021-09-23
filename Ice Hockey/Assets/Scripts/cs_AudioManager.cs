using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class cs_AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSourceEffects;
    [SerializeField] private AudioSource _audioSourceMusic;

    // Start is called before the first frame update
    void Start()
    {
        _audioSourceEffects = GetComponent<AudioSource>();
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

    private void OnEnable()
    {
        cs_Puck.s_pickUpSound += PlayAudioClip;
        cs_PlayerController.s_ShootEffects += PlayAudioClip;
        cs_PlayerController.s_PlayerEffects += PlayAudioClip;
    }
    private void OnDisable()
    {
        cs_Puck.s_pickUpSound -= PlayAudioClip;
        cs_PlayerController.s_ShootEffects -= PlayAudioClip;
        cs_PlayerController.s_PlayerEffects -= PlayAudioClip;
    }
}
