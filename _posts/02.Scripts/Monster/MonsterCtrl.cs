using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MonKind
{
    Skeleton_sword,
    Skeleton_Giant,
    Skeleton_King,
}

public class MonsterCtrl : MonoBehaviour
{

    //몬스터의 현재 상태 정보를 저장할 Enum 변수
    public AnimState MonState = AnimState.idle;

    public MonKind monkind = MonKind.Skeleton_sword;
    //AnimSupporter.cs 쪽에 정의되어 있다

    //----MonsterAI
    [HideInInspector] public GameObject m_AggroTarget = null;
    int m_AggroTgID = -1;             //이 몬스터가 공격해야할 캐럭터의 고유번호
    Vector3 m_MoveDir = Vector3.zero; //수평 진행 노멀 방향 벡터
    Vector3 m_CacVLen = Vector3.zero; //주인공을 향하는 벡터
    float a_CacDist = 0.0f;           //거리 계산용 변수
    float traceDist = 100.0f;           //추적 거리
    public float attackDist = 1.8f;          //공격 거리
    Quaternion a_TargetRot;             //회전 계산용 변수
    float m_RotSpeed = 5.0f;            //초당 회전 속도
    float m_NowStep = 0.0f;                //이동 계산용 변수
    Vector3 a_MoveNextStep = Vector3.zero; //이동 계산용 변수
    public float m_MoveVelocity = 4.0f;        //평면 초당 이동 속도...
    //----MonsterAI

    //인스펙터뷰에 표시할 애니메이션 클래스 변수
    public Anim anim;  //AnimSupporter.cs 쪽에 정의되어 있음

    //public MonType m_MonType = MonType.Skeleton;
    Animation m_RefAnimation = null; //Skeleton
    AnimatorStateInfo animatorStateInfo;

    //------------------------ HP바 표시
    public float Curhp = 100;
    public float Maxhp = 100;
    public Image imgHpbar;
    //------------------------ HP바 표시

    //--------죽는 연출
    protected Vector3 m_DieDir = Vector3.zero;
    protected float m_DieDur = 0.0f;
    protected float m_DieTimer = 0.0f;
    //--------죽는 연출

    // Start is called before the first frame update
    void Start()
    {
        m_RefAnimation = GetComponentInChildren<Animation>();
        //자신의 게임오브젝트 하위 게임오브젝트 중에서 
        //Animation 찾게된 첫번째 Animation 컴포넌트를 리턴
    }


    // Update is called once per frame
    void Update()
    {
        if (MonState == AnimState.die)
            return;

        MonStateUpdate();
        MonActionUpdate();

    }

    //일정한 간격으로 몬스터의 행동 상태를 체크하고 monsterState 값 변경
    void MonStateUpdate()
    {
        if (m_AggroTarget != null)      //어그로 타겟이 있을 경우
        {
            m_CacVLen = m_AggroTarget.transform.position - this.transform.position;

            m_CacVLen.y = 0.0f;
            m_MoveDir = m_CacVLen.normalized; //주인공을 향해 바라 보도록...
            a_CacDist = m_CacVLen.magnitude;

            if (a_CacDist <= attackDist) //공격거리 범위 이내로 들어왔는지 확인
            {
                MonState = AnimState.attack;
            }
            else if (a_CacDist <= traceDist)
            {  //추적거리 범위 이내로 들어왔는지 확인
                MonState = AnimState.trace;  //몬스터의 상태를 추적으로 설정
            }
            else
            {
                MonState = AnimState.idle;   //몬스터의 상태를 idle 모드로 설정
                m_AggroTarget = null;
                m_AggroTgID = -1;
            }

        }//if (m_AggroTarget != null) 
        else //if (m_AggroTarget == null)
        {
            GameObject[] a_players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < a_players.Length; i++)
            {
                m_CacVLen = a_players[i].transform.position - this.transform.position;
                m_CacVLen.y = 0.0f;
                m_MoveDir = m_CacVLen.normalized; //주인공을 향해 바라 보도록...
                a_CacDist = m_CacVLen.magnitude;

                if (a_CacDist <= attackDist) //공격거리 범위 이내로 들어왔는지 확인
                {
                    MonState = AnimState.attack;
                    m_AggroTarget = a_players[i].gameObject;  //타겟설정
                    break;
                }
                else if (a_CacDist <= traceDist) //추적거리 범위 이내로 들어왔는지 확인
                {
                    MonState = AnimState.trace; //몬스터의 상태를 추적으로 설정
                    m_AggroTarget = a_players[i].gameObject;  //타겟설정
                    break;
                }//else if (a_CacDist <= traceDist) 

            }//for (int i = 0; i < a_players.Length; i++)
        }//if (m_AggroTarget == null)
    }//void MonStateUpdate()

    void MonActionUpdate()
    {
        if (MonState == AnimState.attack) //공격상태 일 때
        {//아직 공격 애니메이션 중이라면...
            if (m_AggroTarget == null)
            {
                MySetAnim(anim.Idle.name, 0.13f); //애니메이션 적용
                //애니메이션 상태를 Idle 상태로 돌려놓고..
                return;
            }

            if (0.0001f < m_MoveDir.magnitude)
            {
                a_TargetRot = Quaternion.LookRotation(m_MoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                     a_TargetRot, Time.deltaTime * m_RotSpeed);
            }

            MySetAnim(anim.Attack1.name, 0.12f); //애니메이션 적용
        }
        else if (MonState == AnimState.trace) //추적상태 일 때
        {
            if (m_AggroTarget == null)
            {
                MySetAnim(anim.Idle.name, 0.13f); //애니메이션 적용
                //애니메이션 상태를 Idle 상태로 돌려놓고..
                return;
            }

            if (IsAttackAnim() == true)
            { //아직 공격 애니메이션 중이라면 공격 애니가 끝난 경우에만 추적 이동하도록...
                return;
            }

            //----------- 길찾기를 안했을 때 이동시 회전 시켜준다.
            if (0.0001f < m_MoveDir.magnitude)
            {
                a_TargetRot = Quaternion.LookRotation(m_MoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                      a_TargetRot, Time.deltaTime * m_RotSpeed);
            }
            //----------- 길찾기를 안했을 때 이동시 회전 시켜준다.

            //------------- 일반 이동 코드
            m_NowStep = m_MoveVelocity * Time.deltaTime; //한걸음 크기
            a_MoveNextStep = m_MoveDir * m_NowStep;      //한걸음 벡터
            a_MoveNextStep.y = 0.0f;

            this.transform.position = this.transform.position + a_MoveNextStep;
            //------------- 일반 이동 코드

            //----------- 길찾기를 안했을 때 이동 계산
            MySetAnim(anim.Move.name, 0.12f);  //애니메이션 적용
            //----------- 길찾기를 안했을 때 이동 계산
        }
        else if (MonState == AnimState.idle) //숨쉬기상태 일 때
        {
            MySetAnim(anim.Idle.name, 0.13f); //애니메이션 적용
        }

    } //void MonActionUpdate()

    public void MySetAnim(string newAnim, float CrossTime = 0.0f)
    {
        if (m_RefAnimation != null)
        {
            if (0.0f < CrossTime)
                m_RefAnimation.CrossFade(newAnim, CrossTime);
            else
                m_RefAnimation.Play(newAnim);

        }//if (m_RefAnimation != null)

    } //public void MySetAnim(string newAnim, float CrossTime = 0.0f)

    //-------------공격 애니 관련 변수
    float a_CacRate = 0.0f;
    float a_NormalTime = 0.0f;

    public bool IsAttackAnim() //공격애니메이션 상태 체크 함수
    {
        if (m_RefAnimation == null)
            return false;

        if (m_RefAnimation.IsPlaying(anim.Attack1.name) == true)
        {
            a_NormalTime = m_RefAnimation[anim.Attack1.name].time
                             / m_RefAnimation[anim.Attack1.name].length;

            //소수점 한동작이 몇프로 진행되었는지 계산 변수
            a_CacRate = a_NormalTime - (float)((int)a_NormalTime);

            if (a_CacRate < 0.95f) //공격 애니메이션 끝부분이 아닐 때(공격애니메이션 중이라는 뜻)
                return true;
        }

        return false;
    }

    Vector3 a_DistVec = Vector3.zero;
    float a_CacLen = 0.0f;
    public void Event_AttDamage(string Type) //애니메이션 이벤트 함수로 호출
    {
        if (m_AggroTarget == null)
            return;

        a_DistVec = m_AggroTarget.transform.position - transform.position;
        a_CacLen = a_DistVec.magnitude;
        a_DistVec.y = 0.0f;

        //공격각도 안에 있는 경우
        if (Vector3.Dot(transform.forward, a_DistVec.normalized) < 0.0f) 
            return;  //90도를 넘는 범위에 있다는 뜻

        //공격 범위 밖에 있는 경우
        if ((attackDist + 1.7f) < a_CacLen)
            return;

        {
            m_AggroTarget.GetComponent<PlayerAtk>().TakeDamage(10);
        }

    }//public void Event_AttDamage(string Type) 

    public void TakeDamage(GameObject a_Attacker, float a_Damage)
    {

        if (Curhp <= 0.0f)
            return;

        Curhp -= a_Damage;
        if (Curhp < 0.0f)
            Curhp = 0.0f;

        imgHpbar.fillAmount = (float)Curhp / (float)Maxhp;

        GameMgr.Inst.SpawnDamageTxt((int)a_Damage, this.transform);
        // CameraShake.Inst.ShakeCamera(1.0f);

        // 히트모션

        if (Curhp <= 0)  //사망 처리
        {
            GameMgr.Inst.AddCoin();
            ItemDrop();
            GameMgr.Inst.AddMonKill();
            MonState = AnimState.die;
            MySetAnim(anim.Die.name, 0.1f);
            Destroy(this.gameObject, 1.0f);

            if (monkind == MonKind.Skeleton_King)
            {
                GameMgr.Inst.AddCoin();
                MonState = AnimState.die;
                MySetAnim(anim.Die.name, 0.1f);
                Destroy(this.gameObject, 1.0f);
                GameMgr.Inst.StartCoroutine("StageClear");
            }
        }
    }

    public void ItemDrop()
    {
        int a_Rnd = Random.Range(0, 4);

        if (a_Rnd < 0 || 3 < a_Rnd)
            return;

        GameObject m_Itme = null;
        m_Itme = (GameObject)Instantiate(Resources.Load("Item_Obj"));
        m_Itme.transform.position = this.transform.position;
        if (a_Rnd == 0)
        {
            m_Itme.name = "hpCrystalsv08";
        }
        else if (a_Rnd == 1)
        {
            m_Itme.name = "mpCrystalsv07";
        }
        else
        {
            Item_Type a_ItType = (Item_Type)a_Rnd;
            m_Itme.name = a_ItType.ToString() + "_Item_Obj";
        }

        ItemObjInfo a_RefItemInfo = m_Itme.GetComponent<ItemObjInfo>();
        if (a_RefItemInfo != null)
        {
            a_RefItemInfo.InitItem((Item_Type)a_Rnd, m_Itme.name,
                                    Random.Range(0, 4), Random.Range(0, 4));
        }

        Destroy(m_Itme, 15.0f);  //15초내에 먹어야 한다.
    }



}//public class MonsterCtrl
