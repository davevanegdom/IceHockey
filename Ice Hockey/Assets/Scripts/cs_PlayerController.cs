﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_PlayerController : MonoBehaviour
{

    private cs_GameManager _gameManager;
    private Rigidbody2D _rbPlayer;
    private SpriteRenderer _srPlayer;
    private Camera _mCamera;
    public int PlayerLives;
    [SerializeField] private AudioClip _playerDeath;
    [SerializeField] private AudioClip _playerHit;

    #region Movement
    [SerializeField] private float _defaultMoveSpeed;
    public float MaxMoveSpeed;
    [SerializeField] private float _minMoveSpeed;
    [SerializeField] private float _defaultDecelarationRate;
    [SerializeField] private float _turnRate;
    [SerializeField] private float _dashMultiplier = 1f;
    [SerializeField] private AudioClip _dashSound;
    [SerializeField] [Range(0, 1)] float _retainMomemtumPercentage;
    [SerializeField] [Range(0, 5)] float _dashCooldown;
    private bool _canDash = true;
    public enum DashSystem {DashInLookDirection, DashInInputDirection}
    [SerializeField] private DashSystem _selectedDashSystem = DashSystem.DashInLookDirection;
    #endregion

    #region Shooting
    [SerializeField] private float _defaultFireRate;
    public int PuckCount;

    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private AudioClip _chargeSound;
    [SerializeField] private AudioClip _superShotSound;

    [SerializeField] private GameObject _prefabDynamicPuck;
    [SerializeField] private GameObject _prefabStaticPuck;
    [SerializeField] private float _shootForce;
    [SerializeField] private float _chargeTime;
    [SerializeField] private float _chargeMultiplier;
    private Coroutine _co;
    [SerializeField] private Transform _puckSpawn;
    private List<GameObject> _displayedStaticPucks;
    private List<Vector2> _shootDirections;
    #endregion

    #region Actions
    public static event Action<Vector2, float> s_ShootPuck;
    public static event Action<AudioClip> s_ShootEffects;
    public static event Action s_PlayerDied;
    public static event Action<int> s_UpdatePlayerPucks;
    public static event Action<AudioClip> s_PlayerEffects;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<cs_GameManager>();
        _rbPlayer = GetComponent<Rigidbody2D>();
        _srPlayer = GetComponent<SpriteRenderer>();
        _mCamera = Camera.main;

        DisplayPucks(1);
    }

    // Update is called once per frame
    void Update()
    {
        #region Movement
        float _dHorMove = 0f;
        float _dVerMove = 0f;

        if(Input.GetAxis("Horizontal") != 0)
        {
            _dHorMove = Input.GetAxis("Horizontal");
        }

        if (Input.GetAxis("Vertical") != 0)
        {
            _dVerMove = Input.GetAxis("Vertical");
        }

        MovePlayer(new Vector2(_dHorMove, _dVerMove));

        if(Input.GetKeyDown(KeyCode.Space) && _canDash)
        {
            DashPlayer(new Vector2(_dHorMove, _dVerMove));
        }
        #endregion

        #region shooting

        if(Input.GetMouseButtonDown(0) && PuckCount > 0)
        {
            PlayerShootPuck();
        }

        if(Input.GetMouseButtonDown(1) && PuckCount > 0)
        {
            _co = StartCoroutine(ChargeShot());
        }

        if(Input.GetMouseButtonUp(1) && PuckCount > 0)
        {
            _chargeMultiplier = 0.75f;
            StopCoroutine(_co);
            DisplayPucks(1);
        }
        #endregion

        #region Other
        LookAtMouse(Input.mousePosition);
        DeceleratePlayer();
        #endregion
    }

    private void MovePlayer(Vector2 _moveDir)
    {
        float speed = _rbPlayer.velocity.magnitude;

        if (_rbPlayer.velocity.magnitude < MaxMoveSpeed)
        {
            Vector2 deltaMove = _moveDir.normalized;
            Debug.DrawLine(transform.position, new Vector2(transform.position.x + deltaMove.x, transform.position.y + deltaMove.y), Color.blue);

            if (_rbPlayer.velocity.magnitude < _minMoveSpeed)
            {
                _rbPlayer.AddForce(deltaMove * (1 - (speed / MaxMoveSpeed)) * (_defaultMoveSpeed * 2 * 50 * Time.deltaTime));
                Vector2 _debugPos = new Vector2(transform.position.x + (_rbPlayer.velocity.x * speed), transform.position.y + (_rbPlayer.velocity.y * speed));
                Debug.DrawLine(transform.position, _debugPos, Color.red);
            }
        }
    }

    private void LookAtMouse(Vector2 _mousePos)
    {
        Vector2 _lookAtPos = Camera.main.ScreenToWorldPoint(new Vector2(_mousePos.x, _mousePos.y));
        Vector3 _desiredDirection = new Vector3(_lookAtPos.x - transform.position.x, _lookAtPos.y - transform.position.y, 0);
        transform.right = Vector3.Lerp(transform.right, _desiredDirection, _turnRate * Time.deltaTime);
    }

    private void DeceleratePlayer()
    {
        float _decelarationValue = Vector2.Dot(_rbPlayer.velocity, new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized);
        _rbPlayer.drag = Mathf.Abs(_decelarationValue) * _defaultDecelarationRate;

        if ((Input.GetAxis("Horizontal") > 0.05f || Input.GetAxis("Horizontal") < -0.05f) && (Input.GetAxis("Vertical") > 0.05f || Input.GetAxis("Vertical") < -0.05f))
        {
            _rbPlayer.drag = _defaultDecelarationRate;
        }
    }

    private void DashPlayer(Vector2 _moveDir)
    {
        switch (_selectedDashSystem)
        {
            case DashSystem.DashInLookDirection:
                //rbPlayer.velocity = transform.right * (rbPlayer.velocity.magnitude * dashMultiplier);
                _rbPlayer.velocity -= _rbPlayer.velocity * (1 - _retainMomemtumPercentage);
                _rbPlayer.AddForce(transform.right * (_dashMultiplier * (1 - (_rbPlayer.velocity.magnitude / MaxMoveSpeed))));
                break;
                
            case DashSystem.DashInInputDirection:
                _rbPlayer.velocity -= _rbPlayer.velocity * (1 - _retainMomemtumPercentage);
                Vector2 _dashDir = new Vector2(_moveDir.x, _moveDir.y).normalized;
                _rbPlayer.AddForce(_dashDir * (_dashMultiplier * (1 - (_rbPlayer.velocity.magnitude / MaxMoveSpeed))));
                break;
        }

        s_PlayerEffects?.Invoke(_dashSound);
        _canDash = false;
        StartCoroutine(CooldownTimer(_dashCooldown));
    }

    private void PlayerShootPuck()
    {
        foreach (GameObject _staticPuck in _displayedStaticPucks)
        {
            Destroy(_staticPuck);
        }

        DisplayPucks(_shootDirections.Count);

        foreach (Vector2 _shootDirection in _shootDirections)
        {
            GameObject _puck = Instantiate(_prefabDynamicPuck, _puckSpawn.position, Quaternion.identity);
            _shootForce = _shootForce * _chargeMultiplier;
            _puck.GetComponent<cs_Puck>().ShootPuck(_shootDirection, (_shootForce * _chargeMultiplier));
        }

        PuckCount -=  _displayedStaticPucks.Count;
        Debug.Log(PuckCount);
        _shootForce = 50f;
        s_ShootEffects?.Invoke(_shootSound);

        DisplayPucks(1);
    }

    private void PlayerHit(int _damage)
    {
        if(PlayerLives - _damage > 0)
        {
            PlayerLives -= _damage;
            s_PlayerEffects?.Invoke(_playerHit);
        }
        else
        {
            s_PlayerEffects?.Invoke(_playerDeath);
            s_PlayerDied?.Invoke();
        }

        
    }

    private void DisplayPucks(int _pucks)
    {
        
        if (_puckSpawn.childCount > 0 && _displayedStaticPucks.Count > 0)
        {
            foreach (GameObject _puck in _displayedStaticPucks)
            {
                Destroy(_puck);
            }
        }

        _displayedStaticPucks = new List<GameObject>();
        _shootDirections = new List<Vector2>();
        float _puckInterval = 0.2f;

        if (_pucks > 1)
        {
            float _length = _puckInterval * _pucks;
            float _startPos = (-_length / 2) + (_puckInterval / 2);

            for (int i = 0; i < _pucks; i++)
            {
                GameObject _staticPuck = Instantiate(_prefabStaticPuck, _puckSpawn);
                _staticPuck.transform.localPosition = new Vector2(.1f, _startPos + _puckInterval * i);
                _displayedStaticPucks.Add(_staticPuck);
                _shootDirections.Add(new Vector2(transform.right.x - (_startPos + _puckInterval * i), transform.right.y).normalized);
            }
        }
        else
        {
            GameObject _staticPuck = Instantiate(_prefabStaticPuck, _puckSpawn);
            _staticPuck.transform.localPosition = new Vector2(.1f, 0);
            _displayedStaticPucks.Add(_staticPuck);
            _shootDirections.Add((transform.right).normalized);
        }
    }

    private void PickUpPuck(int _pucks)
    {
        PuckCount += _pucks;
        s_UpdatePlayerPucks?.Invoke(PuckCount);
    }

    public IEnumerator ChargeShot()
    {
        s_ShootEffects?.Invoke(_chargeSound);
        float _time = 0;

        while(_time > _chargeTime)
        {
            _time += Time.deltaTime;
            _chargeMultiplier = Mathf.Lerp(0.75f, 1.25f, _time / _chargeTime);
            yield return null;
        }

        if(PuckCount > 2)
        {
            DisplayPucks(PuckCount);
        }
    }

    private IEnumerator CooldownTimer(float _cooldownTime)
    {
        float _time = _cooldownTime;

        while(_time > 0)
        {
            _time -= Time.deltaTime;
            yield return null;
        }

        _canDash = true;
    }

    private void OnEnable()
    {
        cs_Puck.s_PuckPickedUp += PickUpPuck;
        cs_PuckGoal.s_PlayerCollectPucks += PickUpPuck;
    }
    private void OnDisable()
    {
        cs_Puck.s_PuckPickedUp -= PickUpPuck;
        cs_PuckGoal.s_PlayerCollectPucks -= PickUpPuck;
    }
}
