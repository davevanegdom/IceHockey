using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_Puck : MonoBehaviour
{

    public static event Action<int> s_PuckPickedUp;
    public static event Action<AudioClip> s_pickUpSound;
    [SerializeField] private AudioClip _puckPickUpSound;

    private Rigidbody2D _rbPuck;
    private TrailRenderer _puckTrail;
    [SerializeField] private float _pickUpRadius;
    [SerializeField] private CircleCollider2D _puckCollider;
    [SerializeField] private CircleCollider2D _puckTrigger;
    private cs_PulseEffect _pulseEffect;

    private GameObject _player;

    // Start is called before the first frame update
    void Awake()
    {
        _rbPuck = GetComponent<Rigidbody2D>();
        _puckTrail = GetComponent<TrailRenderer>();
        _pulseEffect = GetComponentInChildren<cs_PulseEffect>();
        _puckCollider = GetComponent<CircleCollider2D>();
        _puckTrigger.enabled = false;

        _player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (!_puckTrigger.enabled && Vector2.Distance(_player.transform.position, transform.position) > _pickUpRadius)
        {
            _puckTrigger.enabled = true;
        }


        if (_rbPuck.velocity.magnitude < 2)
        {
            _pulseEffect.IsPlaying = true;

            if (_puckTrail.enabled)
            {
                _puckTrail.enabled = false;
            }
        }
        else
        {
            _pulseEffect.IsPlaying = false;

            if (!_puckTrail.enabled)
            {
                _puckTrail.enabled = true;
            }
        }
    }

    public void ShootPuck(Vector2 _shootDirection, float _shootForce)
    {
        Vector2 _shootDir = _shootDirection * _shootForce;
        _rbPuck.AddForce(_shootDir, ForceMode2D.Impulse);

    }

    private void PickUpPuck()
    {
        s_PuckPickedUp?.Invoke(1);
        s_pickUpSound?.Invoke(_puckPickUpSound);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            PickUpPuck();
        }
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }
}
