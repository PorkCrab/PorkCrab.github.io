using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossTriggert : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            GameMgr.Inst.bossAlarm.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider col)
    {
        GameMgr.Inst.bossAlarm.SetActive(false);
    }

}
