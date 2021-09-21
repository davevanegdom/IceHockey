using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cs_UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdatePuckCount()
    {

    }

    private void OnEnable()
    {
        cs_Puck.s_PuckPickedUp += UpdatePuckCount;
    }

    private void OnDisable()
    {
        cs_Puck.s_PuckPickedUp -= UpdatePuckCount;
    }
}
