using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAtk : MonoBehaviour
{
    GameObject[] m_EnemyList = null;

    public GameObject SlashPrefabs;
    private ParticleSystem particleSystems;
    public Transform weaponPoint;

    //---------- 키보드 입력값 변수 선언
    float h = 0, v = 0;
    Vector3 MoveNextStep;            //보폭을 계산해 주기 위한 변수
    Vector3 MoveHStep;
    Vector3 MoveVStep;
    float m_MoveVelocity = 10.0f;
    float a_CalcRotY = 0.0f;
    float rotSpeed = 150.0f;
    float m_AttackDist = 4.0f;

    public float Curhp = 1000;
    public float Maxhp = 1000;
    public Image imgHpbar = null;

    public float CurMp = 800;
    public float MaxMp = 800;
    public Image imgMpbar = null;

    public int HpCount = 3;
    public int MpCount = 3;
    public Text hptxt;
    public Text mptxt;

    //------ Animator 관련 변수 
    Animator m_RefAnimator = null;
    string m_prevState = "";
    AnimState m_CurState = AnimState.idle; //IsMine == true 일때
    AnimState CurrState = AnimState.idle;  //IsMine == false 일때

    public Anim anim;  //AnimSupporter.cs 쪽에 정의되어 있음
    AnimatorStateInfo animaterStateInfo;

    bool m_AttRotPermit = false;

    GameObject a_EffObj = null;
    Vector3 a_EffPos = Vector3.zero;

    // public HitShake hitshake; // 타격감

    // Start is called before the first frame update
    void Start()
    {
        Maxhp = 1000;
        Curhp = Maxhp;

        m_RefAnimator = this.gameObject.GetComponent<Animator>();

        {
            GameMgr.Inst.m_refHero = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        KeyBDMove();

        if (Input.GetMouseButtonDown(0))
        {
            NormalAttack();
        }

        if ((0.0f == h && 0.0f == v) && ISAttack() == false)
            MySetAnim(AnimState.idle);

        if (Input.GetKeyDown(KeyCode.Alpha1))
            HpUp();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            MpUp();
    }

    void KeyBDMove()      //키보드 이동
    {
        //-------------- 가감속 없이 이동 처리 하는 방법
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        if (v < 0.0f)
            v = 0.0f;

        if (0.0f != h || 0.0f != v) //키보드 이동처리
        {
            if (IsSkill() == true)
                return;

            a_CalcRotY = transform.eulerAngles.y;
            a_CalcRotY = a_CalcRotY + (h * rotSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0.0f, a_CalcRotY, 0.0f);

            MoveVStep = transform.forward * v;
            MoveNextStep = MoveVStep.normalized * m_MoveVelocity * Time.deltaTime;
            transform.position = transform.position + MoveNextStep;

            MySetAnim(AnimState.move);

        }//if (0.0f != h || 0.0f != v) //키보드 이동처리

    }//void KeyBDMove()      //키보드 이동

    #region
    Vector3 a_DistVec = Vector3.zero;
    float a_CacLen = 0.0f;
    int a_iCount = 0;
    public void Event_AttDamage(string Type) //애니메이션 이벤트 함수로 호출
    {
        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
        a_iCount = m_EnemyList.Length;

        if (Type == AnimState.attack.ToString()) //공격 애니메이션이면...
        {
            for (int i = 0; i < a_iCount; ++i)
            {
                a_DistVec = m_EnemyList[i].transform.position - this.transform.position;
                a_CacLen = a_DistVec.magnitude;
                a_DistVec.y = 0.0f;

                //공격각도 안에 있는 경우
                if (Vector3.Dot(transform.forward, a_DistVec.normalized) < 0.45f)
                    continue;

                //공격 범위 밖에 있는 경우
                if (m_AttackDist + 0.1f < a_CacLen)
                    continue;

                a_EffObj = EffectPool.Inst.GetEffectObj("Sword Slash 3",
                                Vector3.zero, Quaternion.identity);
                a_EffPos = m_EnemyList[i].transform.position;
                a_EffPos.y += 1.1f;
                a_EffObj.transform.position = a_EffPos + (-a_DistVec.normalized * 1.13f);
                a_EffObj.transform.LookAt(a_EffPos + (a_DistVec.normalized * 2.0f));

                m_EnemyList[i].GetComponent<MonsterCtrl>().TakeDamage(this.gameObject, 50);

            }
        }

        else if (Type == AnimState.skill.ToString())
        {
            a_EffObj = EffectPool.Inst.GetEffectObj("Sword Slash 4",
                                           Vector3.zero, Quaternion.identity);
            a_EffPos = transform.position;
            a_EffPos.y = a_EffPos.y + 1.0f;
            a_EffObj.transform.position = a_EffPos + (transform.forward * 2.3f);
            a_EffObj.transform.LookAt(a_EffPos + (-transform.forward * 2.0f));

            //---------주변 모든 몬스터를 찾아서 데이지를 준다.(범위공격)
            for (int i = 0; i < a_iCount; ++i)
            {
                a_DistVec = m_EnemyList[i].transform.position - transform.position;
                a_DistVec.y = 0.0f;

                //공격 범위 밖에 있는 경우
                if (m_AttackDist + 0.1f < a_DistVec.magnitude)
                    continue;

                a_EffObj = EffectPool.Inst.GetEffectObj("Sword Slash Combo 2",
                                        Vector3.zero, Quaternion.identity);
                a_EffPos = m_EnemyList[i].transform.position;
                a_EffPos.y += 1.1f;
                a_EffObj.transform.position = a_EffPos + (-a_DistVec.normalized * 1.13f); //0.7f);
                a_EffObj.transform.LookAt(a_EffPos + (a_DistVec.normalized * 2.0f));
                m_EnemyList[i].GetComponent<MonsterCtrl>().TakeDamage(this.gameObject, 100);


            }//for (int i = 0; i < iCount; ++i)

        }//else if (Type == AnimState.skill.ToString())
    }

    void Event_AttFinish(string Type)

    { //공격애니메이션 끝났는지? 판단하는 이벤트 함수
        if (0.0f != h || 0.0f != v)
        {
            MySetAnim(AnimState.move);
            return;
        }

        if (m_prevState.ToString() == AnimState.skill.ToString() &&
            Type == AnimState.attack.ToString())
            return;

        if (m_prevState.ToString() == AnimState.attack.ToString() &&
           Type == AnimState.skill.ToString())
            return;

        else
        {
            MySetAnim(AnimState.idle);
        }
    }
    #endregion

    public void TakeDamage(float a_Damage = 10.0f)
    {
        if (Curhp <= 0.0f)
            return;

        Curhp -= a_Damage;
        if (Curhp < 0.0f)
            Curhp = 0.0f;

        GameMgr.Inst.SpawnDamageTxt((int)a_Damage, this.transform, 2);

        imgHpbar.fillAmount = (float)Curhp / (float)Maxhp;

        if (Curhp <= 0) //사망처리
        {
            MySetAnim(AnimState.die);
            GameMgr.Inst.GameOver();
        }
    }

    public void MySetAnim(AnimState newAnim,
                 float CrossTime = 1.0f, string AnimName = "")
    {
        if (m_RefAnimator == null)
            return;

        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState))
        {
            if (m_prevState.ToString() == newAnim.ToString())
                return;
        }

        if (!string.IsNullOrEmpty(m_prevState))
        {
            m_RefAnimator.ResetTrigger(m_prevState.ToString());
            m_prevState = null;
        }

        m_AttRotPermit = false;// 모든 애니메이션이 시작할 때 우선 꺼주고

        if (0.0f < CrossTime)
        {
            m_RefAnimator.SetTrigger(newAnim.ToString());
        }
        else
        {
            m_RefAnimator.Play(AnimName, -1, 0f);
            //가운데는 Layer Index, 뒤에 0f는 처음부터 다시시작
        }

        m_prevState = newAnim.ToString(); //이전스테이트에 현재스테이트 저장
        m_CurState = newAnim;

    }//public void MySetAnim()

    public bool ISAttack()
    {
        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState))
        {
            if (m_prevState.ToString() == AnimState.attack.ToString() ||
                m_prevState.ToString() == AnimState.skill.ToString())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsSkill()
    {
        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState))
        {
            if (m_prevState.ToString() == AnimState.skill.ToString())
            {
                return true;
            }
        }

        return false;
    }

    void NormalAttack()
    {
        if (m_prevState == AnimState.idle.ToString()
           || m_prevState == AnimState.move.ToString())
        {
            if (0.0f != h || 0.0f != v)
            {
                return;
            }

            MySetAnim(AnimState.attack);
        }
    }

    public void HpUp()
    {
        if (HpCount <= 0)
            return;

        if (Curhp == Maxhp)
            return;

        Curhp += Maxhp * 0.5f;

        if (Maxhp < Curhp)
            Curhp = Maxhp;

        if (imgHpbar != null)
            imgHpbar.fillAmount = Curhp / Maxhp;

        HpCount -= 1;
        hptxt.text = "" + HpCount;

    }

    public void MpUp()
    {
        if (MpCount <= 0)
            return;

        if (CurMp == MaxMp)
            return;

        CurMp += MaxMp * 0.5f;

        if (MaxMp < CurMp)
            CurMp = MaxMp;

        if (imgMpbar != null)
            imgMpbar.fillAmount = CurMp / MaxMp;

        MpCount -= 1;
        mptxt.text = "" + MpCount;
    }

    public void SkillOrder(string Type, ref float CooltmLen, ref float CurCooltm)
    {
        if (0.0f < CurCooltm) //tm == time
            return;

        if (m_prevState != AnimState.skill.ToString())
        {

            CurMp -= 100;
            MpCount -= 1;
            MySetAnim(AnimState.skill);



            CooltmLen = 5.0f;
            CurCooltm = CooltmLen;
        }
    }

    void OnTriggerEnter(Collider other) //Item먹기
    {
        if (other.gameObject.name.Contains("Item_Obj") == true)
        {
            //인벤토리에 넣기...
            GameMgr.Inst.InvenAddItem(other.gameObject);
            Destroy(other.gameObject);

            Debug.Log("줍기");
        }
    }
}