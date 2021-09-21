using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_PlayerController : MonoBehaviour
{

    private cs_GameManager _gameManager;
    private Rigidbody2D _rbPlayer;
    private SpriteRenderer _srPlayer;
    private Camera _mCamera;

    #region Movement
    [SerializeField] private float _defaultMoveSpeed;
    public float MaxMoveSpeed;
    [SerializeField] private float _minMoveSpeed;
    private float _moveSpeed;
    [SerializeField] private float _defaultDecelarationRate;
    [SerializeField] private float _turnRate;
    [SerializeField] private float _dashMultiplier = 1f;
    [SerializeField] [Range(0, 1)] float _retainMomemtumPercentage;
    [SerializeField] [Range(0, 5)] float _dashCooldown;
    private bool _canDash;
    public enum DashSystem {DashInLookDirection, DashInInputDirection}
    [SerializeField] private DashSystem _selectedDashSystem = DashSystem.DashInLookDirection;
    #endregion

    #region Shooting
    [SerializeField] private float _defaultFireRate;
    public int PuckCount;
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
    public static event Action<int, Vector2, List<Vector2>, float> s_ShootPuck;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<cs_GameManager>();
        _rbPlayer = GetComponent<Rigidbody2D>();
        _srPlayer = GetComponent<SpriteRenderer>();
        _mCamera = Camera.main;
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

        if (Input.GetAxis("Vertial") != 0)
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
                _rbPlayer.AddForce(deltaMove * ((1 - (speed / MaxMoveSpeed))) * (_moveSpeed * 50 * Time.deltaTime));
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

        _canDash = false;
        StartCoroutine(CooldownTimer(_dashCooldown));
    }

    private void PlayerShootPuck()
    {
        s_ShootPuck?.Invoke(1, _puckSpawn.position, _shootDirections, (_shootForce * _chargeMultiplier));
    }

    private void DisplayPucks(int _pucks)
    {
        if (_puckSpawn.childCount > 0 && _displayedStaticPucks.Count > 0)
        {
            foreach (GameObject puck in _displayedStaticPucks)
            {
                Destroy(puck);
            }
        }

        _displayedStaticPucks = new List<GameObject>();
        float _puckInterval = 0.2f;

        if (_pucks > 1)
        {
            float _length = _puckInterval * _pucks; //* pucks;
            float _startPos = (-_length / 2) + (_puckInterval / 2);

            for (int i = 0; i < _pucks; i++)
            {
                GameObject staticPuck = Instantiate(_prefabStaticPuck, _puckSpawn);
                staticPuck.transform.localPosition = new Vector2(.05f, _startPos + _puckInterval * i);
                _displayedStaticPucks.Add(staticPuck);
            }
        }
        else
        {
            GameObject staticPuck = Instantiate(_prefabStaticPuck, _puckSpawn);
            staticPuck.transform.localPosition = new Vector2(.05f, 0);
            _displayedStaticPucks.Add(staticPuck);
        }
    }

    private void PickUpPuck()
    {
        PuckCount++;
    }

    private IEnumerator ChargeShot()
    {
        float _time = 0f;

        while(_time > 0)
        {
            _time -= Time.deltaTime;
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
    }
    private void OnDisable()
    {
        cs_Puck.s_PuckPickedUp -= PickUpPuck;
    }
}
