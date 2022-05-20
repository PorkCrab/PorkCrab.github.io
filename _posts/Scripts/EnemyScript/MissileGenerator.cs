using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileGenerator : MonoBehaviour
{
    public GameObject missilePrefab;
    float spawn = 2.0f;
    float delta = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (InGameMgr.m_InGState == InGameState.GameIng)
        {
            this.delta += Time.deltaTime;
            if (this.delta > this.spawn)
            {
                this.delta = 0;
                GameObject go = Instantiate(missilePrefab) as GameObject;

                int dropPos = Random.Range(-2, 3);
                go.GetComponent<MissileCtrl>().Set(dropPos);

                spawn = 2.0f;
                if (spawn < 0.5f)
                    spawn = 0.5f;
            }
        }
        
        else
        {
            this.delta = 0;
            spawn = 2.0f;
        }
    }
}
