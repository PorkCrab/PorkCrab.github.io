using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    // 총알 이동변수
    Vector3 m_DirTgVec = Vector3.up; // 총알의 방향지정
    Vector3 a_StartPos = Vector3.zero; // 총알의 시작방향
    float m_MoveSpeed = 10.0f; // 총알의 속도

    public float BulletAtt = 30.0f;

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "BorderBullet")
        {
            Destroy(gameObject);
        }

        if (coll.gameObject.tag == "Boss")
        {
            Destroy(gameObject);
        }

        if (coll.gameObject.tag == "Enemy")
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {

        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.position += m_DirTgVec * Time.deltaTime * m_MoveSpeed;
    }

    public void BulletSpawn(Vector3 a_OwnPos, Vector3 a_DirTgVec,
        float a_MvSpeed = 10.0f, float att = 20.0f)
    {
        m_DirTgVec = a_DirTgVec; // 날아갈방향
        a_StartPos = a_OwnPos + (m_DirTgVec * 0.5f);
        transform.position = new Vector3(a_StartPos.x,
            a_StartPos.y, 0.0f);

        m_MoveSpeed = a_MvSpeed;
        BulletAtt = att;
    }

}
