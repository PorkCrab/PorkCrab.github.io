using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubHero_Ctrl : MonoBehaviour
{
    public GameObject m_BulletObj = null;
    float m_AttSpeed = 0.5f;
    float m_CacAtTick = 0.0f;

    GameObject a_NewObj = null;
    BulletCtrl a_BulletSC = null;

    float angle = 0.0f;
    float radius = 1.0f;
    float speed = 100.0f;

    GameObject parent_Obj = null;
    Vector3 parent_Pos = Vector3.zero;

    float m_lifeTime = 0.0f;

    bool doule_OnOff = false;

    Player m_RefHero = null;

    // Start is called before the first frame update
    void Start()
    {
        m_RefHero = GameObject.FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_RefHero != null)
        {
            if (0.0f < m_RefHero.m_Doule_OnTime)
                doule_OnOff = true;
            else //if (0.0f < m_RefHero.m_Doule_OnTime)
                doule_OnOff = false;
        }

        angle += Time.deltaTime * speed;
        if (360.0f < angle)
            angle -= 360.0f;

        parent_Pos = parent_Obj.transform.position;

        transform.position = parent_Pos +
            new Vector3(radius * Mathf.Cos(angle * Mathf.Deg2Rad),
            radius * Mathf.Sin(angle * Mathf.Deg2Rad));

        Attack_Update();

        m_lifeTime -= Time.deltaTime;

        if (m_lifeTime <= 0.0f)
            Destroy(this.gameObject);
    }

    public void SubHeroSpwan(GameObject a_Paren, float a_Angle, float a_lifeTime)
    {
        parent_Obj = a_Paren;
        angle = a_Angle;
        m_lifeTime = a_lifeTime;
    }

    void Attack_Update()
    {
        if (0.0f < m_CacAtTick)
            m_CacAtTick = m_CacAtTick - Time.deltaTime;

        if (m_CacAtTick <= 0.0f)
        {
            if (doule_OnOff == true)
            {
                Vector3 pos;
                for (int ii = 0; ii < 2; ii++)
                {
                    a_NewObj = (GameObject)Instantiate(m_BulletObj);
                    a_BulletSC = a_NewObj.GetComponent<BulletCtrl>();
                    pos = transform.position;
                    pos.y += 0.2f - (ii * 0.4f);
                    a_BulletSC.BulletSpawn(pos, Vector3.up);

                }
            }
            else
            {
                a_NewObj = (GameObject)Instantiate(m_BulletObj);
                a_BulletSC = a_NewObj.GetComponent<BulletCtrl>();
                a_BulletSC.BulletSpawn(this.transform.position, Vector3.up);

            }

            m_CacAtTick = m_AttSpeed;
        }

    }
}
