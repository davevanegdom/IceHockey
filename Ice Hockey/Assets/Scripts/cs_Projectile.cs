using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_Projectile : MonoBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private int _destroyAfterTime;
    private SpriteRenderer _srProjectile;
    private Rigidbody2D _rbProjectile;
    [SerializeField] private List<Sprite> _projectiles;

    public static event Action<int> s_ProjectileDamage;
    // Start is called before the first frame update
    void Start()
    {
        _srProjectile = GetComponent<SpriteRenderer>();
        _srProjectile.sprite = _projectiles[UnityEngine.Random.Range(0, _projectiles.Count - 1)];
        _rbProjectile = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(_rbProjectile.velocity.magnitude < 2f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player" && _rbProjectile.velocity.magnitude > 2f)
        {
            s_ProjectileDamage?.Invoke(_damage);
            Destroy(gameObject);
        }
    }
}
