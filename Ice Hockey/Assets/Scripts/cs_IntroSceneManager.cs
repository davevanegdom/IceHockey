using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class cs_IntroSceneManager : MonoBehaviour
{
    [SerializeField] private Image _imageFade;
    [SerializeField] private float _fadeInTime;
    [SerializeField] private float _fadeOutTime;
    [SerializeField] private float _waitTime;
    private bool _isLoading = false;

    private void Start()
    {
        StartCoroutine(Fade(_fadeInTime, 1, 0, false));
        StartCoroutine(Timer());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !_isLoading)
        {
            StopAllCoroutines();
            StartCoroutine(Fade(_fadeOutTime, 0, 1, true));
        }
    }


    private IEnumerator Fade(float _fadeTime, int _startValue, int _endValue, bool _loadGame)
    {
        float _time = 0;
        while(_time < _fadeTime)
        {
            _time += Time.deltaTime;
            _imageFade.color = new Color(_imageFade.color.r, _imageFade.color.g, _imageFade.color.b, Mathf.Lerp(_startValue, _endValue, _time / _fadeTime));
            yield return null;
        }

        if (_loadGame)
        {
            SceneManager.LoadScene(2);
            _isLoading = false;
        }
    }

    private IEnumerator Timer()
    {
        float _time = 0;

        while(_time < _waitTime)
        {
            _time += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(Fade(_fadeOutTime, 0, 1, true));
    }
}
