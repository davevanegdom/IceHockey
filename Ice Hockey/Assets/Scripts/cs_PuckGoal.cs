using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_PuckGoal : MonoBehaviour
{
    public int Identifier;
    [SerializeField] private GameObject _staticPuckPrefab;
    public int CollectedPucks = 0;
    [SerializeField] private int _maxCollectablePucks;


    public static event Action<int, int> s_SetCollectedPucks;
    public static event Action<int> s_PlayerCollectPucks;

    // Start is called before the first frame update
    void Start()
    {
        UpdateCollectedCount(CollectedPucks);
    }

    private void UpdateCollectedCount(int _collectedPucks)
    {
        s_SetCollectedPucks?.Invoke(_collectedPucks, Identifier);
        DisplayPucks(_collectedPucks);
    }

    private void DisplayPucks(int _collectedPucks)
    {
        if (transform.childCount > 0)
        {
            //Clear all previous stored pucks
            foreach (Transform _child in transform)
            {
             
                Destroy(_child.gameObject);
            }
        }

        for (int i = 0; i < _collectedPucks; i++)
        {
            Instantiate(_staticPuckPrefab, transform);
        }

        float _intervalDistance = 0.15f;
        float _length = _intervalDistance * (_collectedPucks - 1);
        float _startPos = transform.position.y - _length * 1.5f;
        int _loopInt = 0;

        foreach (Transform _puck in transform)
        {
            _puck.transform.localPosition = new Vector2(_startPos + (_loopInt * _intervalDistance), - 0.5f);
            _loopInt++;
            Debug.Log(_puck.transform.position);
        }
    }
    
    private void PlayerPickUpPucks(int _collectedPucks)
    {
        s_PlayerCollectPucks?.Invoke(_collectedPucks);
        

        foreach (Transform _staticPuck in transform)
        {
            Destroy(_staticPuck.gameObject);
        }

        CollectedPucks = 0;
        UpdateCollectedCount(CollectedPucks);
    }

    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            PlayerPickUpPucks(CollectedPucks);
        }

        if(collision.gameObject.tag == "Puck" && CollectedPucks < _maxCollectablePucks)
        {
            CollectedPucks++;
            UpdateCollectedCount(CollectedPucks);
        }
    }*/

    private void OnCollisionEnter2D(Collision2D _collision)
    {
        if (_collision.gameObject.tag == "Player")
        {
            PlayerPickUpPucks(CollectedPucks);
        }

        if (_collision.gameObject.tag == "Puck" && CollectedPucks < _maxCollectablePucks)
        {
            CollectedPucks++;
            UpdateCollectedCount(CollectedPucks);
            Destroy(_collision.gameObject);
        }
    }

}
