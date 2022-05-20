using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageTxt : MonoBehaviour
{
    float m_EffTime = 0.0f;
    float MvVelocity = 1.1f / 1.05f;  //1.05f초 동안에 1.1m 간다면
    float ApVelocity = 1.0f / (1.0f - 0.4f);
    //alpha  0.4(0.0f)초부터 연출 1.0초(1.0f)까지

    Vector3 m_CurPos = Vector3.zero;

    Text m_ThisText = null;
    Color m_Color;

    // Start is called before the first frame update
    void Start()
    {
        m_ThisText = this.GetComponent<Text>();
        m_Color = m_ThisText.color;
        m_Color.a = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        m_EffTime += Time.deltaTime;

        if (m_EffTime < 1.05f)
        {
            m_CurPos = transform.position;
            m_CurPos.y += Time.deltaTime * MvVelocity;
            transform.position = m_CurPos;
        }

        if (0.4f < m_EffTime)
        {
            m_Color.a -= Time.deltaTime * ApVelocity;
            if (m_Color.a < 0.0f)
                m_Color.a = 0.0f;
            m_ThisText.color = m_Color;
        }

        if (0.5f <= m_EffTime)
        {
            Destroy(this.gameObject);
        }
    }

    public void InitDamage(float a_Damage, Color a_Color)
    {
        m_ThisText = this.GetComponent<Text>();
        if (a_Damage <= 0.0f)
        {
            int a_Dmg = (int)Mathf.Abs(a_Damage);
            m_ThisText.text = "- " + a_Dmg;
        }
        else
        {
            m_ThisText.text = "+ " + (int)a_Damage;
        }

        m_ThisText.color = a_Color;
        m_Color = m_ThisText.color;
        m_Color.a = 1.0f;
    }
}
