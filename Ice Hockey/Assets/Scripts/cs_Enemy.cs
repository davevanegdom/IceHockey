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

    [SerializeField] private int _enemyLives;

    #region Movement & Shooting
    [SerializeField] private bool _shootAbility;
    [SerializeField] private bool _canShoot;
    [SerializeField] private int _moveSpeed;
    [SerializeField] private int _turnRate;
    [SerializeField] private float _shootRate;
    [SerializeField] private float _shootForce;
    [SerializeField] private float _shootRange;
    [SerializeField] private GameObject[] _shootPrefabs;
    [Range(0, 1)] private float _moveAccuracy;
    [Range(0, 1)] private float _shootAccuracy;
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
        //_srReaction.enabled = false;
        _cEnemy.isTrigger = true;

        InvokeRepeating("ChaseOffset", 1f, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        LookAtPlayer();
        MoveEnemy();

    }

    private void MoveEnemy()
    {
        _rbEnemy.AddForce(transform.right * _moveSpeed * 10 * Time.deltaTime);
    }

    private void LookAtPlayer()
    {
        Vector2 _desiredDirection = new Vector2((_player.transform.position.x + _followPos.x)- transform.position.x, (_player.transform.position.y + _followPos.y) - transform.position.y);
        transform.right = Vector3.Lerp(transform.right, _desiredDirection, _turnRate * Time.deltaTime);
    }

    private void ChaseOffset()
    {
        Vector2 _playerPos = _player.transform.position;
        Vector2 _offSet = UnityEngine.Random.insideUnitCircle * 2 * _moveAccuracy;
        _followPos = _playerPos + _offSet;
    }

    private void TakeDamage(int _damage)
    {
        if(_enemyLives - _damage > 0)
        {
            _enemyLives -= _damage;

            if(_enemyLives == 1)
            {
                Reaction(_Reactions.Angry);
            }
        }
        else
        {
            s_EnemyDied?.Invoke();
            Destroy(gameObject);
        }
        
    }

    private void Unclutter()
    {
         
    }

    private void ShootAtPlayer()
    {
        if (_shootAbility && _canShoot && Vector2.Distance(_player.transform.position, transform.position) < _shootRange)
        {
            GameObject _objectToShoot = Instantiate(_shootPrefabs[UnityEngine.Random.Range(0, _shootPrefabs.Length - 1)], transform.position, Quaternion.identity);
            Rigidbody2D _rbProjectile = _objectToShoot.GetComponent<Rigidbody2D>();
            _rbProjectile.AddForce(transform.right * _shootForce);
            _canShoot = false;
        }

        StartCoroutine(CooldownTimer());
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
        ShootAtPlayer();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Puck")
        {
            TakeDamage(1);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "ArenaColliders")
        {
            _cEnemy.isTrigger = false;
            
        }

        Debug.Log("Enable Collider");
    }
}
