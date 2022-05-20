using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissileCtrl : MonoBehaviour
{
    GameObject player;
    [HideInInspector] public float speed = 5f;
    public Image warningImg;
    float waitTime = 1.0f;

    float alpha = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        if (this.player == null)
            this.player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (InGameMgr.m_InGState == InGameState.GameIng)
        {
            if (0 < waitTime)
            {
                waitTime -= Time.deltaTime;
                warningDirect();
                return;
            }


            if (warningImg.enabled == true)
                warningImg.enabled = false;

            transform.Translate(0, -speed * Time.deltaTime, 0);
            if (transform.position.y < player.transform.position.y - 10.0f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Set(int a_Pos)
    {
        this.player = GameObject.Find("Player");
        transform.position = new Vector3(a_Pos * 1.1f,
            player.transform.position.y + 10.0f);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        warningImg.transform.position = new Vector3(screenPos.x,
              warningImg.transform.position.y, warningImg.transform.position.z);
    }

    void warningDirect()
    {
        if (warningImg.color.a >= 1.0f)
            alpha = -6.0f;
        else if (warningImg.color.a <= 0.0f)
            alpha = 6.0f;

        warningImg.color = new Color(1.0f, 1.0f, 1.0f,
            warningImg.color.a + alpha * Time.deltaTime);
    }

}
