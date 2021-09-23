using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_PuckGoal : MonoBehaviour
{
    [SerializeField] private GameObject _staticPuckPrefab;
    public int CollectedPucks = 0;
    [SerializeField] private int _maxCollectablePucks;

    public static event Action<int> s_SetCollectedPucks;
    public static event Action<int> s_PlayerCollectPucks;

    // Start is called before the first frame update
    void Start()
    {
        UpdateCollectedCount(CollectedPucks);
    }

    private void UpdateCollectedCount(int _collectedPucks)
    {
        s_SetCollectedPucks?.Invoke(_collectedPucks);
        DisplayPucks(_collectedPucks);
    }

    private void DisplayPucks(int _collectedPucks)
    {
        if (transform.childCount > 0)
        {
            //Clear all previous stored pucks
            foreach (Transform child in transform)
            {
             
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < _collectedPucks; i++)
        {
            Instantiate(_staticPuckPrefab, transform);
        }

        float intervalDistance = 0.15f;
        float length = intervalDistance * (_collectedPucks - 1);
        float startPos = transform.position.y - length * 1.5f;
        int loopInt = 0;

        foreach (Transform puck in transform)
        {
            puck.transform.position = new Vector2((transform.position.x + transform.position.normalized.x), startPos + (loopInt * intervalDistance));
            loopInt++;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerPickUpPucks(CollectedPucks);
        }

        if (collision.gameObject.tag == "Puck" && CollectedPucks < _maxCollectablePucks)
        {
            CollectedPucks++;
            UpdateCollectedCount(CollectedPucks);
            Destroy(collision.gameObject);
        }
    }

}
