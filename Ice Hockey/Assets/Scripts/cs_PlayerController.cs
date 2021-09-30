using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_PlayerController : MonoBehaviour
{
    [Header("General")]
    private cs_GameManager _gameManager;
    private Rigidbody2D _rbPlayer;
    private SpriteRenderer _srPlayer;
    private Camera _mCamera;
    public int PlayerLives;
    [SerializeField] private AudioClip _playerDeath;
    [SerializeField] private AudioClip _playerHit;
    [SerializeField] private GameObject _iceParticle;
    [SerializeField] private TrailRenderer _rightTrail;
    [SerializeField] private TrailRenderer _leftTrail;

    #region Movement
    [Header("Movement")]
    [SerializeField] private float _defaultMoveSpeed;
    public float MaxMoveSpeed;
    [SerializeField] private float _minMoveSpeed;
    [SerializeField] private float _defaultDecelarationRate;
    [SerializeField] private float _turnRate;
    [SerializeField] private float _dashMultiplier = 1f;
    [SerializeField] private AudioClip _dashSound;
    [SerializeField] [Range(0, 1)] float _retainMomemtumPercentage;
    [SerializeField] [Range(0, 5)] float _dashCooldown;
    public bool _canDash = true;
    public enum DashSystem {DashInLookDirection, DashInInputDirection}
    [SerializeField] private DashSystem _selectedDashSystem = DashSystem.DashInLookDirection;
    #endregion

    #region Shooting
    [Header("Shooting")]
    [SerializeField] private float _defaultFireRate;
    public int PuckCount;
    [SerializeField] private GameObject _chargeBar;

    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private AudioClip _noPuckSound;
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

    #region Super Mode
    [Header("Super Mode")]
    public bool _canSuperMode = true;
    [SerializeField] private float _superModeDuration;
    [SerializeField] private float _speedMultiplier;
    [SerializeField] private float _shootingMultiplier;
    private bool _isInvincible = false;
    [SerializeField] private float _playerSizeIncrease;
    [SerializeField] private AudioClip _superSound;
    private GameObject _superEffect;
    [SerializeField] private ParticleSystem _superModeParticle;
    #endregion

    #region Pick Up Ability
    [Header("Pick up ability")]
    [SerializeField] private float _pickUpCoolDown;
    [SerializeField] private AudioClip _pickUpAbilitySound;
    public bool _canPickUp = true;
    [SerializeField] private float _pickUpRange;
    #endregion

    #region Actions
    public static event Action<Vector2, float> s_ShootPuck;
    public static event Action<AudioClip> s_SoundEffects;
    public static event Action<GameObject> s_PlayerDied;
    public static event Action<int> s_UpdatePlayerPucks;
    public static event Action<AudioClip> s_PlayerEffects;
    public static event Action<int> s_TakeDamage;
    public static event Action<float> s_ShakeCamera;
    public static event Action<Vector2, GameObject> s_ParticleEffect;
    public static Action<int, bool> s_AbilityCover;
    public void AbilityCover(int _identifier, bool _visibility) => s_AbilityCover?.Invoke(_identifier, _visibility);
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<cs_GameManager>();
        _rbPlayer = GetComponent<Rigidbody2D>();
        _srPlayer = GetComponent<SpriteRenderer>();
        _mCamera = Camera.main;
        _superEffect = GameObject.FindGameObjectWithTag("SuperMode");
        _superEffect.SetActive(false);
        _superModeParticle.Stop();
        _chargeBar.SetActive(false);
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
        else if(Input.GetMouseButtonDown(0) && PuckCount == 0)
        {
            s_SoundEffects?.Invoke(_noPuckSound);
        }

        if(Input.GetMouseButtonDown(1) && PuckCount > 0)
        {
            _co = StartCoroutine(ChargeShot());
        }

        if(Input.GetMouseButtonUp(1) && PuckCount > 0)
        {
            _chargeMultiplier = 0.75f;
            _chargeBar.SetActive(false);
            StopCoroutine(_co);
            DisplayPucks(1);
        }
        
        #endregion

        #region Abilities
        if(Input.GetKeyDown(KeyCode.E) && _canPickUp)
        {
            PickUpAbility();
        }

        if(Input.GetKeyDown(KeyCode.F) && _canSuperMode)
        {
            SuperAbility();
        }
        #endregion

        #region Other
        LookAtMouse(Input.mousePosition);
        DeceleratePlayer();


        // !!-DISABLE FOR BUILD-!!
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayerHit(PlayerLives);
        }

        #endregion
    }

    private void MovePlayer(Vector2 _moveDir)
    {
        float speed = _rbPlayer.velocity.magnitude;

        _rightTrail.time = speed / MaxMoveSpeed;
        _leftTrail.time = speed / MaxMoveSpeed;

        if (speed < MaxMoveSpeed * _speedMultiplier)
        {
            Vector2 deltaMove = _moveDir.normalized;
            Debug.DrawLine(transform.position, new Vector2(transform.position.x + deltaMove.x, transform.position.y + deltaMove.y), Color.blue);

            _rbPlayer.AddForce(deltaMove * (1 - (speed / (MaxMoveSpeed * _speedMultiplier))) * (_defaultMoveSpeed * (100 * _rbPlayer.mass) * Time.deltaTime));
            Vector2 _debugPos = new Vector2(transform.position.x + (_rbPlayer.velocity.x * speed), transform.position.y + (_rbPlayer.velocity.y * speed));
            Debug.DrawLine(transform.position, _debugPos, Color.red); 
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
                //Dot product
                _rbPlayer.velocity -= _rbPlayer.velocity * (1 - _retainMomemtumPercentage);
                _rbPlayer.AddForce(transform.right * (_dashMultiplier * (1 - (_rbPlayer.velocity.magnitude / MaxMoveSpeed))));
                break;
                
            case DashSystem.DashInInputDirection:
                //Dot product
                _rbPlayer.velocity -= _rbPlayer.velocity * (1 - _retainMomemtumPercentage);
                Vector2 _dashDir = new Vector2(_moveDir.x, _moveDir.y).normalized;
                _rbPlayer.AddForce(_dashDir * (_dashMultiplier * (1 - (_rbPlayer.velocity.magnitude / MaxMoveSpeed))));
                break;
        }

        s_PlayerEffects?.Invoke(_dashSound);
        s_AbilityCover?.Invoke(0, true);
        _canDash = false;
        StartCoroutine(CooldownTimer(_dashCooldown, 0));
    }

    private void PlayerShootPuck()
    {
        _shootDirections = new List<Vector2>();
        foreach (GameObject _staticPuck in _displayedStaticPucks)
        {
            if(_displayedStaticPucks.Count > 1)
            {
                //Vector2 _puckDir = (_staticPuck.transform.position - transform.position);
                Vector2 _shootDir = (_staticPuck.transform.position - transform.position).normalized;
                _shootDirections.Add(_shootDir);
                s_SoundEffects?.Invoke(_superShotSound);
            }
            else
            {
                _shootDirections.Add(transform.right);
                s_SoundEffects?.Invoke(_shootSound);
            }
            Destroy(_staticPuck);
        }

        DisplayPucks(_shootDirections.Count);

        foreach (Vector2 _shootDirection in _shootDirections)
        {
            GameObject _puck = Instantiate(_prefabDynamicPuck, _puckSpawn.position, Quaternion.identity);
            _puck.GetComponent<cs_Puck>().ShootPuck(_shootDirection, (_shootForce * _chargeMultiplier * _shootingMultiplier));
        }


        PuckCount -=  _displayedStaticPucks.Count;
        
        s_ShakeCamera?.Invoke(0.25f);
        s_UpdatePlayerPucks?.Invoke(PuckCount);

        if(PuckCount > 0)
        {
            DisplayPucks(1);
        }
        else
        {
            foreach (GameObject _staticPuck in _displayedStaticPucks)
            {
                Destroy(_staticPuck);
            }
        }
    }

    private void PlayerHit(int _damage)
    {
        if (!_isInvincible)
        {
            s_ShakeCamera?.Invoke(0.3f);
            int _remainingLives = PlayerLives - _damage;
            if (_remainingLives > 0)
            {
                PlayerLives -= _damage;
                s_PlayerEffects?.Invoke(_playerHit);
                s_TakeDamage?.Invoke(PlayerLives);
            }
            else
            {
                PlayerLives = 0;
                s_TakeDamage?.Invoke(PlayerLives);
                s_PlayerEffects?.Invoke(_playerDeath);
                s_PlayerDied?.Invoke(gameObject);
                ExitSuperMode();
                StopAllCoroutines();
            }
        }
        
    }

    public void DisplayPucks(int _pucks)
    {
        
        if (_puckSpawn.childCount > 0 && _displayedStaticPucks.Count > 0)
        {
            foreach (GameObject _puck in _displayedStaticPucks)
            {
                Destroy(_puck);
            }
        }

        _displayedStaticPucks = new List<GameObject>();
        float _puckInterval = 0.2f * 1.5f;

        if (_pucks > 1)
        {
            float _length = _puckInterval * _pucks;
            float _startPos = (-_length / 2) + (_puckInterval / 2);

            for (int i = 0; i < _pucks; i++)
            {
                GameObject _staticPuck = Instantiate(_prefabStaticPuck, _puckSpawn);
                _staticPuck.transform.localPosition = new Vector2(.1f, _startPos + _puckInterval * i);
                _displayedStaticPucks.Add(_staticPuck);
            }
        }
        else
        {
            GameObject _staticPuck = Instantiate(_prefabStaticPuck, _puckSpawn);
            _staticPuck.transform.localPosition = new Vector2(.1f, 0);
            _displayedStaticPucks.Add(_staticPuck);
        }
    }

    private void PickUpPuck(int _pucks)
    {
        PuckCount += _pucks;
        s_UpdatePlayerPucks?.Invoke(PuckCount);
        DisplayPucks(1);
    }

    private void PickUpAbility()
    {
        Collider2D[] _hitPucks = Physics2D.OverlapCircleAll(transform.position, _pickUpRange);
        foreach (Collider2D _hitPuck in _hitPucks)
        {
            if(_hitPuck.gameObject.tag == "Puck")
            {
                _hitPuck.gameObject.GetComponent<cs_Puck>().PickUpPuck();
            }
        }

        s_SoundEffects?.Invoke(_pickUpAbilitySound);
        s_AbilityCover(1, true);
        _canPickUp = false;
        StartCoroutine(CooldownTimer(_pickUpCoolDown, 1));
    }

    private void SuperAbility()
    {
        StartCoroutine(SuperMode());
    }

    private void ExitSuperMode()
    {
        _superModeParticle.Stop();
        _superEffect.SetActive(false);
        _speedMultiplier = 1f;
        _shootingMultiplier = 1f;
        _isInvincible = false;
        transform.localScale = Vector2.one;
        s_AbilityCover?.Invoke(2, true);
    }

    public IEnumerator ChargeShot()
    {
        s_SoundEffects?.Invoke(_chargeSound);
        _chargeBar.SetActive(true);
        SpriteRenderer _chargeBarSprite = _chargeBar.GetComponent<SpriteRenderer>();
        float _time = 0;

        while(_time < _chargeTime)
        {
            _time += Time.deltaTime;
            _chargeMultiplier = Mathf.Lerp(0.75f, 1.25f, _time / _chargeTime);
            _chargeBar.transform.localScale = new Vector3(_chargeBar.transform.localScale.x, _chargeMultiplier - 0.75f, _chargeBar.transform.localScale.z);
            _chargeBarSprite.color = Color.Lerp(Color.green, Color.red, _time / _chargeTime);
            yield return null;
        }

        _chargeBar.SetActive(false);

        if(PuckCount > 2)
        {
            int _pucksToDisplay = PuckCount;
            DisplayPucks(Mathf.Clamp(_pucksToDisplay, 3, 5));
        }
    }

    private IEnumerator CooldownTimer(float _cooldownTime, int _identifier)
    {
        float _time = _cooldownTime;

        while(_time > 0)
        {
            _time -= Time.deltaTime;
            yield return null;
        }

        switch (_identifier)
        {
            case 0:
                _canDash = true;
                s_AbilityCover?.Invoke(0, false);
                break;
            case 1:
                _canPickUp = true;
                s_AbilityCover?.Invoke(1, false);
                break;
        }
    }

    private IEnumerator SuperMode()
    {
        s_SoundEffects?.Invoke(_superSound);
        _superModeParticle.Play();
        _superEffect.SetActive(true);
        _speedMultiplier = 1.5f;
        _shootingMultiplier = 1.2f;
        _isInvincible = true;
        transform.localScale = Vector2.one * _playerSizeIncrease;
        _canSuperMode = false;
        s_AbilityCover?.Invoke(2, true);
        yield return new WaitForSeconds(_superModeDuration);

        ExitSuperMode();
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "ArenaColliders")
        {
            s_ParticleEffect?.Invoke(transform.position, _iceParticle);
            s_ShakeCamera?.Invoke(0.1f);
        }

        if(collision.gameObject.tag == "Enemy" && _rbPlayer.velocity.magnitude > 4f && !_canDash)
        {
            collision.gameObject.GetComponent<cs_Enemy>().TakeDamage(_rbPlayer.velocity.magnitude / 5);
        }
    }

    private void OnEnable()
    {
        cs_Puck.s_PuckPickedUp += PickUpPuck;
        cs_PuckGoal.s_PlayerCollectPucks += PickUpPuck;
        cs_Projectile.s_ProjectileDamage += PlayerHit;
        cs_Enemy.s_HitPlayer += PlayerHit;
    }
    private void OnDisable()
    {
        cs_Puck.s_PuckPickedUp -= PickUpPuck;
        cs_PuckGoal.s_PlayerCollectPucks -= PickUpPuck;
        cs_Projectile.s_ProjectileDamage -= PlayerHit;
        cs_Enemy.s_HitPlayer -= PlayerHit;
    }
}
