using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGCtrl : MonoBehaviour
{
    float Speed = -2.5f;
    float endPos = -30.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       transform.Translate(0, Speed * Time.deltaTime, 0);
       if (transform.position.y <= endPos)
           transform.Translate(0, -transform.position.y, 0);
    }
}
