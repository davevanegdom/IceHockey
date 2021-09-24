using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cs_PulseEffect : MonoBehaviour
{
    public bool IsPlaying = false;
    public float TargetTime;
    public float Size;
    private float _startSize;
    private float _time;
    private SpriteRenderer _sr;
    private Color _serializedColor;


    // Start is called before the first frame update
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.enabled = false;
        _serializedColor = _sr.color;
        IsPlaying = false;
        _startSize = transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPlaying)
        {
            _sr.enabled = true;
            timer();
        }
        else
        {
            _sr.enabled = false;
            _time = 0f;
        }
    }

    private void timer()
    {
        _time += Time.deltaTime / TargetTime;

        if (_time <= TargetTime)
        {
            Animate(_time);
        }
        else
        {
            _time = 0f;
        }
    }

    void Animate(float _effectTime)
    {
        //size
        transform.localScale = Vector2.Lerp(Vector2.zero, Vector2.one * (Size * _startSize), _effectTime);

        //alpha
        _sr.color = Vector4.Lerp(_serializedColor, new Color(_serializedColor.r, _serializedColor.g, _serializedColor.b, 0), _effectTime);
    }

}
