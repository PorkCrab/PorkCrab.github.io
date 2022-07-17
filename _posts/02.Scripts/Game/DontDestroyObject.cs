using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyObject : MonoBehaviour
{
    static public DontDestroyObject Inst;

    public GameObject Player;
    public GameObject UI;
    public GameObject Cam;
    public GameObject MousePos;
    public GameObject GameMgr;

    void Awake()
    {
        var obj = FindObjectsOfType<DontDestroyObject>();

        if (obj.Length == 1)
        {
            DontDestroyOnLoad(Player);
            DontDestroyOnLoad(UI);
            DontDestroyOnLoad(Cam);
            DontDestroyOnLoad(MousePos);
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(Player);
            Destroy(UI);
            Destroy(Cam);
            Destroy(MousePos);
            Destroy(this.gameObject);
        }
    }
}
