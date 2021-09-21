using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class cs_Puck : MonoBehaviour
{

    public static event Action s_PuckPickedUp;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ShootPuck(int _pucks, Vector2 _spawnPos, List<Vector2> _shootDirections, float _shootForce)
    {

    }

    private void PickUpPuck()
    {
        s_PuckPickedUp?.Invoke();
    }

    private void OnEnable()
    {
        cs_PlayerController.s_ShootPuck += ShootPuck;
    }

    private void OnDisable()
    {
        cs_PlayerController.s_ShootPuck -= ShootPuck;
    }
}
