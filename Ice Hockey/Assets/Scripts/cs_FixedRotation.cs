using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cs_FixedRotation : MonoBehaviour
{
    public GameObject RelativeTo;
    public GameObject Parent;
    public Vector2 PositionOffset;


    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, (-(RelativeTo.transform.rotation.z) + 90));
        transform.position = new Vector2(Parent.transform.position.x + PositionOffset.x, Parent.transform.position.y + PositionOffset.y);
    }
}
