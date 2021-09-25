using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_Enemy : MonoBehaviour
{
    private GameObject _player;
    private Rigidbody2D _rbEnemy;
    private CapsuleCollider2D _cEnemy;
    private SpriteRenderer _srEnemy;
    private bool _enteredArena;

    [SerializeField] private float _enemyHealth;

    #region Movement & Shooting
    [SerializeField] private bool _shootAbility = true;
    [SerializeField] private bool _canShoot = true;
    [SerializeField] private int _moveSpeed;
    [SerializeField] private int _turnRate;
    [SerializeField] private float _shootRate;
    [SerializeField] private float _shootForce;
    [SerializeField] private float _shootRange;
    [SerializeField] private GameObject _shootPrefab;
    [Range(0, 1)] private float _moveAccuracy;
    [Range(0, 1)] private float _shootAccuracy;
    [SerializeField] private float _defaultDecelarationRate;
    private Vector2 _followPos = Vector2.zero;
    #endregion

    #region Reaction
    private enum _Reactions {Laughing, Angry}
    private _Reactions _reaction;
    [SerializeField] private Sprite[] _reactionBubbles;
    private SpriteRenderer _srReaction;
    #endregion

    #region Actions
    public static event Action s_EnemyDied;
    #endregion 


    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _rbEnemy = GetComponent<Rigidbody2D>();
        _cEnemy = GetComponent<CapsuleCollider2D>();
        _srEnemy = GetComponent<SpriteRenderer>();
        _srReaction = GetComponentInChildren<SpriteRenderer>();
        InvokeRepeating("ChaseOffset", 1f, 3f);
        InvokeRepeating("Unclutter", 5f, 3f);
        InvokeRepeating("ShootAtPlayer", 3f, _shootRate);
    }

    // Update is called once per frame
    void Update()
    {
        LookAtPlayer();
        MoveEnemy();
        //DecelerateEnemy();
    }

    private void MoveEnemy()
    {
        Vector2 _moveDir = ((Vector2)transform.right + (_player.GetComponent<Rigidbody2D>().velocity * 0.9f).normalized) * _moveSpeed * (5 * _rbEnemy.mass) * Time.deltaTime;
        _rbEnemy.AddForce(_moveDir);

        float _decelarationValue = Vector2.Dot(_rbEnemy.velocity, _moveDir);
        _rbEnemy.drag = (1 -_decelarationValue) * _defaultDecelarationRate;
    }

    private void DecelerateEnemy()
    {
        float _decelarationValue = Vector2.Dot(_rbEnemy.velocity.normalized, (_player.transform.position - transform.position).normalized);
        _rbEnemy.drag = (1 - _decelarationValue) * _defaultDecelarationRate;

    }

    private void LookAtPlayer()
    {
        if(Vector2.Distance(_player.transform.position, transform.position) > 1f)
        {
            Vector2 _desiredDirection = ((Vector2)_player.transform.position + _followPos) - (Vector2)transform.position;
            transform.right = Vector3.Lerp(transform.right, _desiredDirection, _turnRate * Time.deltaTime);
        }
        else
        {
            Vector2 _desiredDirection = ((Vector2)_player.transform.position) - (Vector2)transform.position;
            transform.right = Vector3.Lerp(transform.right, _desiredDirection, _turnRate * Time.deltaTime);
        }
        
    }

    private void ChaseOffset()
    {
        Vector2 _playerPos = _player.transform.position;
        Vector2 _offSet = UnityEngine.Random.insideUnitCircle * 2f * _moveAccuracy;
        _followPos = _offSet;
    }

    private void TakeDamage(float _damage)
    {
        if(_enemyHealth - _damage > 0)
        {
            _enemyHealth -= _damage;
        }
        else
        {
            s_EnemyDied?.Invoke();
            Destroy(gameObject);
        }
    }

    private void Unclutter()
    {
        Collider2D[] _enemies = Physics2D.OverlapCircleAll(transform.position, 2f, LayerMask.NameToLayer("Gameplay"));
        if(_enemies.Length > 0)
        {
            foreach (Collider2D _enemy in _enemies)
            {
                if (_enemy.gameObject.tag == "Enemy")
                {
                    _enemy.GetComponent<Rigidbody2D>().AddForce((new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * 100f));
                }
            }
            Debug.Log("Unclutter");
        }
        
    }

    private void ShootAtPlayer()
    {
        if (_shootAbility && _canShoot && Vector2.Distance(_player.transform.position, transform.position) < _shootRange && !ObstacleCheck())
        {
            GameObject _objectToShoot = Instantiate(_shootPrefab, new Vector2(transform.localPosition.x + 0.25f, transform.localPosition.y), Quaternion.identity);
            Rigidbody2D _rbProjectile = _objectToShoot.GetComponent<Rigidbody2D>();
            _rbProjectile.AddForce(transform.right * _shootForce);
            _canShoot = false;
            Debug.Log("Shoot");

            StartCoroutine(CooldownTimer());
        }
    }

    private bool ObstacleCheck()
    {
        RaycastHit2D _hit = Physics2D.Raycast(transform.position, transform.right, _shootRange, LayerMask.NameToLayer("Gameplay"));

        if (_hit.collider == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void Reaction(_Reactions _reaction)
    {

    }

    private IEnumerator CooldownTimer()
    {
        float _time = _shootRate;

        while(_time > 0)
        {
            _time -= Time.deltaTime;
            yield return null;
        }

        int _waitTime = UnityEngine.Random.Range(1, 10);
        yield return new WaitForSeconds(_waitTime);

        _canShoot = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Puck")
        {
            float _rbPuckVelocity = collision.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude;
            if(_rbPuckVelocity > 10f)
            {
                float _puckDamage = _rbPuckVelocity; 
                TakeDamage(_puckDamage);
                Debug.Log(_puckDamage);
            }
        }
    }
}
