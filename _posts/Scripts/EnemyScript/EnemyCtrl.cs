using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCtrl : MonoBehaviour
{
    public string enemyName;

    public float m_CurHp;
    public float m_MaxHp;
   
    public float m_Speed = 2.5f;
    public Image m_HPSdBar = null;

    public Sprite[] sprites;
    public GameObject explosionEffect;

    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public float curShotDelay;
    public float maxShotDelay;

    public GameObject player;

    SpriteRenderer spriteRenderer;

    // 보스패턴
    public int patternIndex;
    public int curPatternCount;
    public int[] maxPatternCount;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        if (enemyName == "B")
        {
            Invoke("Stop", 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyName == "B")
            return;

        if (InGameMgr.m_InGState == InGameState.GameIng)
        {
            Fire();
            Reload();
        }

    }


    void Stop()
    {
        if (!gameObject.activeSelf)
            return;

        Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        rigid.velocity = Vector2.zero;

        Invoke("Think", 2);
    }

    void Think()
    {
        patternIndex = patternIndex == 3 ? 0 : patternIndex + 1;
        curPatternCount = 0;

        switch (patternIndex)
        {
            case 0:
                FireFoward();
                break;
            case 1:
                FireShot();
                break;
            case 2:
                FireArc();
                break;
            case 3:
                FireAround();
                break;

        }
    }

    void FireFoward()
    {
        GameObject bulletR = Instantiate(bulletObjB, transform.position + Vector3.right * 0.3f, transform.rotation);
        GameObject bulletRR = Instantiate(bulletObjB, transform.position + Vector3.right * 0.45f, transform.rotation);
        GameObject bulletL = Instantiate(bulletObjB, transform.position + Vector3.left * 0.3f, transform.rotation);
        GameObject bulletLL = Instantiate(bulletObjB, transform.position + Vector3.left * 0.45f, transform.rotation);


        Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
        Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();

        rigidR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidRR.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);
        rigidLL.AddForce(Vector2.down * 8, ForceMode2D.Impulse);

        Destroy(bulletR, 4.0f);
        Destroy(bulletRR, 4.0f);
        Destroy(bulletL, 4.0f);
        Destroy(bulletLL, 4.0f);

        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireForward", 2);
        else
            Invoke("Think", 3);

    }

    void FireShot()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject bullet = Instantiate(bulletObjA, transform.position, transform.rotation);
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector2 dirVec = player.transform.position - transform.position;
            Vector2 ranVec = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0f, 2f));
            dirVec += ranVec;
            rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);

            Destroy(bullet, 4.0f);
        }
        

        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireShot", 3.5f);
        else
            Invoke("Think", 3);
    }

    void FireArc()
    {
        GameObject bullet = Instantiate(bulletObjA);
        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.identity;

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 2 * curPatternCount/ maxPatternCount[patternIndex]), -1);
        rigid.AddForce(dirVec.normalized * 5, ForceMode2D.Impulse);

        Destroy(bullet, 5.0f);

        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireArc", 0.15f);
        else
            Invoke("Think", 3);
    }

    void FireAround()
    {
        int roundNumA = 50;
        int roundNumB = 40;
        int roundNum = curPatternCount % 2 == 0 ? roundNumA : roundNumB;

        for (int i = 0; i < roundNumA; i++)
        {
            GameObject bullet = Instantiate(bulletObjB);
            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;

            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 2 * i / roundNum),
                                         Mathf.Sin(Mathf.PI * 2 * i / roundNum));
            rigid.AddForce(dirVec.normalized * 5, ForceMode2D.Impulse);

            Vector3 rotVec = Vector3.forward * 360 * i / roundNum + Vector3.forward * 90;
            bullet.transform.Rotate(rotVec);

            Destroy(bullet, 5.0f);
        }

        curPatternCount++;

        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireAround", 0.7f);
        else
            Invoke("Think", 2);
    }


    void Fire()
    {
        if (curShotDelay < maxShotDelay)
            return;

        if (enemyName == "L")
        {
            GameObject bulletR = Instantiate(bulletObjB, transform.position + Vector3.right * 0.3f, transform.rotation);
            GameObject bulletL = Instantiate(bulletObjB, transform.position + Vector3.left * 0.3f, transform.rotation);

            Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
            Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();

            Vector3 dirVecR = player.transform.position - (transform.position + Vector3.right * 0.3f);
            Vector3 dirVecL = player.transform.position - (transform.position + Vector3.left * 0.3f);
             
            rigidR.AddForce(dirVecR.normalized * 4, ForceMode2D.Impulse);
            rigidL.AddForce(dirVecL.normalized * 4, ForceMode2D.Impulse);

            Destroy(bulletR, 6.0f);
            Destroy(bulletL, 6.0f);

        }

        if (enemyName == "S")
        {
            GameObject bullet = Instantiate(bulletObjA, transform.position, transform.rotation);
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Vector3 dirVec = player.transform.position - transform.position;
            rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);

            Destroy(bullet, 6.0f);
        }

        curShotDelay = 0;
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    public void TakeDamage(float a_Value)
    {
        if (m_CurHp <= 0.0f)
            return;

        InGameMgr.Inst.DamageTxt(-a_Value, transform, Color.red);

        m_CurHp -= a_Value;
        spriteRenderer.sprite = sprites[1]; 
        Invoke("ReturnSprite", 0.1f);

        if (m_CurHp <= 0)
            m_CurHp = 0;

        if (m_HPSdBar != null)
            m_HPSdBar.fillAmount = m_CurHp / m_MaxHp;


        if (m_CurHp <= 0)
        {
            ExplosionEffect();
            InGameMgr.Inst.AddScore();

            // 코인드랍
            int dice = Random.Range(0, 10);
            if (dice < 4)
                if (InGameMgr.m_CoinItem != null)
                {
                    GameObject a_CoinObj = (GameObject)Instantiate(InGameMgr.m_CoinItem);
                    a_CoinObj.transform.position = this.transform.position;
                    Destroy(a_CoinObj, 10.0f);
                }

            int dice2 = Random.Range(0, 10);
            if (dice2 < 2)
                if (InGameMgr.m_PowerItem != null)
                {
                    GameObject a_PowerObj = (GameObject)Instantiate(InGameMgr.m_PowerItem);
                    a_PowerObj.transform.position = this.transform.position;
                    Destroy(a_PowerObj, 7.0f);
                }

            Destroy(gameObject);
        }

    }

    void ReturnSprite()
    {
        spriteRenderer.sprite = sprites[0];
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag == "Bullet")
        {
            TakeDamage(30.0f);
        }

        if (coll.tag == "Border")
        {
            Destroy(gameObject);
        }
    }

    void ExplosionEffect()
    {
        GameObject exEffect = (GameObject)Instantiate(explosionEffect, this.transform.position, Quaternion.identity);
        exEffect.GetComponent<ParticleSystem>().Play();
        Destroy(exEffect, 2.0f);
    }
}
