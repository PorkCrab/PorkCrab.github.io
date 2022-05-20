using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    private float h = 0.0f;
    private float v = 0.0f;
    private float moveSpeed = 6.0f;

    Vector3 moveDir = Vector3.zero;

    float m_CurHP = 30000.0f;
    float m_MaxHP = 30000.0f;
    public Image m_HpBar = null;

    Animator anim;

    // 지형밖으로 못나가게 하기
    Vector3 HalfSize = Vector3.zero;   
    Vector3 m_CacCurPos = Vector3.zero;

    float a_LmtBdLeft = 0;
    float a_LmtBdTop = 0;
    float a_LmtBdRight = 0;
    float a_LmtBdBottom = 0;

    [Header("Bullet")]
    public GameObject[] Bullet = null;
    public int Power = 1;

    [Header("BulletSetting")]
    float m_AttSpeed = 0.15f; // 공속
    float m_CacAtTick = 0.0f; // 연속발사 틱
    GameObject a_NewObj = null; // 복사본을 받을 임시변수
    BulletCtrl a_BulletSc = null; // BulletCtrl을 받을 임시변수

    [Header("GameOver")]
    public GameObject GameOverPanel;
    public Button Lobby_Btn;

    //------ 쉴드 스킬
    float m_SdDuration = 10.0f;
    float m_SdOnTime = 0.0f;
    public GameObject ShieldObj = null;
    //------ 쉴드 스킬

    //------laser shot
    [HideInInspector] public float m_Doule_OnTime = 0.0f;
    float doule_Delay = 1.0f;
    //------laser shot

    //------ Sub Hero
    int sub_Count = 0;
    float m_Sub_OnTime = 0.0f;
    float sub_Delay = 10.0f;
    public GameObject sub_Obj = null;
    public GameObject sub_Parent = null;
    //------ Sub Hero

    //---JoyStick 이동 처리 변수
    private float m_JoyMvLen = 0.0f;
    private Vector3 m_JoyMvDir = Vector3.zero;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer sprRend = gameObject.GetComponentInChildren<SpriteRenderer>();

        HalfSize.x = sprRend.bounds.size.x / 2.0f - 0.23f;
        HalfSize.y = sprRend.bounds.size.y / 2.0f - 0.05f;

        HalfSize.z = 1.0f;

        if (Lobby_Btn != null)
            Lobby_Btn.onClick.AddListener(GotoLobby);
    }

    // Update is called once per frame
    void Update()
    {
        if (InGameMgr.m_InGState == InGameState.GameIng)
        {
            Move();
            Fire();

            JoyStickMvUpdate();
        }

        LimitMove();

        //------ Doule Shoot
        if (0.0f < m_Doule_OnTime)
        {
            m_Doule_OnTime -= Time.deltaTime;

            if (m_Doule_OnTime <= 0.0f)
                m_Doule_OnTime = 0.0f;
        }
        //------ Doule Shoot

        //------ Sub Hero 업데이트
        if (0.0f < m_Sub_OnTime)
        {
            m_Sub_OnTime -= Time.deltaTime;

            if (m_Sub_OnTime <= 0.0f)
                m_Sub_OnTime = 0.0f;

        }

        //------------------- 쉴드 상태 업데이트
        if (0.0f < m_SdOnTime)
        {
            m_SdOnTime -= Time.deltaTime;
            if (ShieldObj != null && ShieldObj.activeSelf == false)
                ShieldObj.SetActive(true);
        }
        else
        {
            if (ShieldObj != null && ShieldObj.activeSelf == true)
                ShieldObj.SetActive(false);
        }
    }

    void GotoLobby()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        InGameMgr.m_InGState = InGameState.GameExit;
    }

    public void SetJoyStickMv(float a_JoyMvLen, Vector3 a_JoyMvDir)
    {
        m_JoyMvLen = a_JoyMvLen;
        if (0.0f < a_JoyMvLen)
        {
            m_JoyMvDir = new Vector3(a_JoyMvDir.x, a_JoyMvDir.y, 0.0f);
        }
    }

    public void JoyStickMvUpdate()
    {
        if (0.0f != h || 0.0f != v)
            return;

        ////--- 조이스틱 코드
        if (0.0f < m_JoyMvLen)  //조이스틱으로 움직일 때 
        {
            moveDir = m_JoyMvDir;

            float amtToMove = moveSpeed * Time.deltaTime;

            transform.Translate(m_JoyMvDir * m_JoyMvLen * amtToMove, Space.Self);

            Debug.Log("조이스틱 이동");
        }

    }//public void JoyStickMvUpdate()

    void Move()
    {
        // KeyControl
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        if (h != 0.0f || v != 0.0f)
        {
            moveDir = new Vector3(h, v, 0);
            if (1.0f < moveDir.magnitude)
                moveDir.Normalize();
            transform.position +=
                moveDir * moveSpeed * Time.deltaTime;
        }

        // 애니메이션 움직임
        if (Input.GetButtonDown("Horizontal") || Input.GetButtonUp("Horizontal"))
        {
            anim.SetInteger("Input", (int)h);
        }
    }

    void Fire()
    {
        if (0.0f < m_CacAtTick)
            m_CacAtTick = m_CacAtTick - Time.deltaTime;

  
        switch (Power)
        {
            case 1:
                if (m_CacAtTick <= 0.0f)
                {
                    // 총알의 프리펩 가져오기
                    a_NewObj = (GameObject)Instantiate(Bullet[0]);
                    a_BulletSc = a_NewObj.GetComponent<BulletCtrl>();
                    a_BulletSc.BulletSpawn(this.transform.position, Vector3.up);
                
                    m_CacAtTick = m_AttSpeed; // 연속발사틱 = 플레이어공속
                }
                else if (0.0f < m_Doule_OnTime) //더블샷
                {
                    Vector3 pos;
                    for (int ii = 0; ii < 2; ii++)
                    {
                        a_NewObj = (GameObject)Instantiate(Bullet[0]);
                        a_BulletSc = a_NewObj.GetComponent<BulletCtrl>();
                        pos = transform.position;
                        pos.x += 0.2f - (ii * 0.4f);
                        a_BulletSc.BulletSpawn(pos, Vector3.up);
                    }
                    m_CacAtTick = m_AttSpeed;
                }
                break;

            case 2:
                if (m_CacAtTick <= 0.0f)
                {
                    // 총알의 프리펩 가져오기
                    a_NewObj = (GameObject)Instantiate(Bullet[1]);
                    a_BulletSc = a_NewObj.GetComponent<BulletCtrl>();
                    a_BulletSc.BulletSpawn(this.transform.position, Vector3.up);

                    m_CacAtTick = m_AttSpeed * 0.8f; // 연속발사틱 = 플레이어공속
                }
                else if (0.0f < m_Doule_OnTime) //더블샷
                {
                    Vector3 pos;
                    for (int ii = 0; ii < 2; ii++)
                    {
                        a_NewObj = (GameObject)Instantiate(Bullet[0]);
                        a_BulletSc = a_NewObj.GetComponent<BulletCtrl>();
                        pos = transform.position;
                        pos.x += 0.2f - (ii * 0.4f);
                        a_BulletSc.BulletSpawn(pos, Vector3.up);
                    }

                    m_CacAtTick = m_AttSpeed;
                }
                break;

            case 3:
                if (m_CacAtTick <= 0.0f)
                {
                    // 총알의 프리펩 가져오기
                    a_NewObj = (GameObject)Instantiate(Bullet[2]);
                    a_BulletSc = a_NewObj.GetComponent<BulletCtrl>();
                    a_BulletSc.BulletSpawn(this.transform.position, Vector3.up);

                    m_CacAtTick = m_AttSpeed * 0.7f; // 연속발사틱 = 플레이어공속
                }
                else if (0.0f < m_Doule_OnTime) //더블샷
                {
                    Vector3 pos;
                    for (int ii = 0; ii < 2; ii++)
                    {
                        a_NewObj = (GameObject)Instantiate(Bullet[0]);
                        a_BulletSc = a_NewObj.GetComponent<BulletCtrl>();
                        pos = transform.position;
                        pos.x += 0.2f - (ii * 0.4f);
                        a_BulletSc.BulletSpawn(pos, Vector3.up);
                    }

                    m_CacAtTick = m_AttSpeed;
                }
                break;
        }

        if (4 <= Power)
            Power =  3;
    }

    void LimitMove()
    {
        m_CacCurPos = transform.position;

        a_LmtBdLeft = InGameMgr.m_SceenWMin.x + HalfSize.x;
        a_LmtBdTop = InGameMgr.m_SceenWMin.y + HalfSize.y;
        a_LmtBdRight = InGameMgr.m_SceenWMax.x - HalfSize.x;
        a_LmtBdBottom = InGameMgr.m_SceenWMax.y - HalfSize.y;

        if (m_CacCurPos.x < a_LmtBdLeft)
            m_CacCurPos.x = a_LmtBdLeft;

        if (a_LmtBdRight < m_CacCurPos.x)
            m_CacCurPos.x = a_LmtBdRight;

        if (m_CacCurPos.y < a_LmtBdTop)
            m_CacCurPos.y = a_LmtBdTop;

        if (a_LmtBdBottom < m_CacCurPos.y)
            m_CacCurPos.y = a_LmtBdBottom;

        transform.position = m_CacCurPos;
    }

    public void TakeDamage(float a_Value)
    {
        if (m_CurHP <= 0.0f)
            return;

        if (0.0 < m_SdOnTime) //쉴드 스킬 발동 중일 때
            return;

        InGameMgr.Inst.DamageTxt(-a_Value, transform, Color.blue);

        m_CurHP = m_CurHP - a_Value;
        if (m_CurHP < 0.0f)
            m_CurHP = 0.0f;

        if (m_HpBar != null)
            m_HpBar.fillAmount = m_CurHP / m_MaxHP;

        if (m_CurHP <= 0.0f)
        {
            Time.timeScale = 0.0f;
            GameOverPanel.SetActive(true);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Enemy")
        {
            TakeDamage(30);
        }

        if (col.tag == "Missile")
        {
            TakeDamage(40);
            Destroy(col.gameObject);
        }

        if (col.tag == "Boss")
        {
            TakeDamage(50);
        }

        else if (col.tag == "EnemyBullet")
        {
            TakeDamage(30);
            Destroy(col.gameObject);
        }

        else if (col.gameObject.name.Contains("CoinItem") == true)
        {
            InGameMgr.Inst.AddGold();
            Destroy(col.gameObject);
        }

        else if (col.gameObject.name.Contains("powerItem") == true)
        {
            Power += 1;
            Destroy(col.gameObject);
        }
    }

    public void UseItem(CharType a_CrType)
    {
        if (a_CrType < 0 || CharType.CrCount <= a_CrType)//선택이없는 상태
            return;

        if (a_CrType == CharType.Char_0)
        {
            m_CurHP += m_MaxHP * 0.5f;
            InGameMgr.Inst.DamageTxt(m_MaxHP * 0.5f,
                                    transform, new Color(0.18f, 0.5f, 0.34f));

            if (m_MaxHP < m_CurHP)
                m_CurHP = m_MaxHP;
            if (m_HpBar != null)
                m_HpBar.fillAmount = m_CurHP / m_MaxHP;
        }//if (a_CrType == CharType.Char_0)
        else if (a_CrType == CharType.Char_1)
        {
            m_CurHP = m_MaxHP;
            InGameMgr.Inst.DamageTxt(m_MaxHP,
                                    transform, new Color(0.18f, 0.5f, 0.34f));

            if (m_MaxHP < m_CurHP)
                m_CurHP = m_MaxHP;
            if (m_HpBar != null)
                m_HpBar.fillAmount = m_CurHP / m_MaxHP;
        }
        else if (a_CrType == CharType.Char_2)
        {
            if (0.0f < m_SdOnTime)
                return;

            m_SdOnTime = m_SdDuration;

            InGameMgr.Inst.SkillTimeFunc(
                GlobalValue.m_CrDataList[(int)a_CrType], m_SdOnTime, m_SdDuration);
        }
        else if (a_CrType == CharType.Char_3)
        {
            if (0.0f < m_Doule_OnTime)
                return;

            m_Doule_OnTime = doule_Delay;

            InGameMgr.Inst.SkillTimeFunc(
                GlobalValue.m_CrDataList[(int)a_CrType], m_Doule_OnTime, doule_Delay);
        }
        else if (a_CrType == CharType.Char_4)
        { 
            if (0.0f < m_Sub_OnTime)
                return;

            sub_Count = 3;
            m_Sub_OnTime = sub_Delay;
            for (int ii = 0; ii < sub_Count; ii++)
            {
                GameObject obj = Instantiate(sub_Obj) as GameObject;
                obj.transform.SetParent(sub_Parent.transform);
                SubHero_Ctrl sub = obj.GetComponent<SubHero_Ctrl>();
                sub.SubHeroSpwan(sub_Parent, (360 / sub_Count) * ii, m_Sub_OnTime);
            }

            InGameMgr.Inst.SkillTimeFunc(
                GlobalValue.m_CrDataList[(int)a_CrType], m_Sub_OnTime, sub_Delay);
        }
        else
        {
            Debug.Log("미등록 스킬");
            return;
        }

        GlobalValue.m_CrDataList[(int)a_CrType].m_CurSkillCount--;
    }
}
