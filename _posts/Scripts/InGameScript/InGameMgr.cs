using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PlayFab;
using PlayFab.ClientModels;
using SimpleJSON;

enum JoyStickType
{
    Fixed = 0,
    //m_JoySBackObj.activeSelf == true && m_JoystickPickPanel.activeSelf == false
    Flexible = 1,
    //m_JoySBackObj.activeSelf == true && m_JoystickPickPanel.activeSelf == true
    FlexibleOnOff = 2
    //m_JoySBackObj.activeSelf == false && m_JoystickPickPanel.activeSelf == true
}

public enum InGameState
{
    GameReady,
    GameIng,
    GameEnd,
    GameExit,
}

public enum PacketType
{
    BestScore,       //최고점수
    UserGold,        //유저골드
    NickUpdate,      //닉네임갱신
    UpdateExp        //경험치갱신
}


public class InGameMgr : MonoBehaviour
{
    public static InGameMgr Inst = null;
    public static InGameState m_InGState = InGameState.GameReady;
    public static int level = 0;
    public static float m_GameTime = 1.0f;
    public static Player m_refHero = null;

    //스크린의 월드 좌표
    public static Vector3 m_SceenWMin = new Vector3(-10.0f, -5.0f, 0.0f);
    public static Vector3 m_SceenWMax = new Vector3(10.0f, 5.0f, 0.0f);

    [Header("Normal")]
    public Text m_CurScoreTxt = null;
    public Text m_BestScoreTxt = null;
    public Text m_GoldTxt = null;
    public Button DlgBtn = null;
    public GameObject readyPanel;
    public GameObject bossWarning;
    public Text countTxt;
    public Text GameTimeTxt = null;
    Text ExpLevel_Txt = null;

    int m_KillCount = 0;
    int m_CurScore = 0;
    int m_CurGold = 1000000;

    float countDown = 4.0f;

    [Header("ConfigBox")]
    public Button m_CfgBtn = null;
    public GameObject Canvas_Dialog = null;
    public GameObject m_ConfigBoxObj = null;

    public string m_TempStrBuff = "";

    public static GameObject m_CoinItem = null;
    public static GameObject m_PowerItem = null;

    [Header("DamageText")]
    public Transform m_HUD_Canvas = null;
    public GameObject m_DamageObj = null;
    GameObject a_DamClone;
    DamageTxt a_DamageTx;
    Vector3 a_StCacPos;


    [Header("-------- Inventory Show OnOff --------")]
    public Button m_InVen_Btn = null;
    public Transform m_InVenScrollTr = null;
    private bool m_InVen_ScOnOff = false;
    private float m_ScSpeed = 9000.0f;
    private Vector3 m_ScOnPos = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 m_ScOffPos = new Vector3(-1000.0f, 0.0f, 0.0f);
    private Vector3 m_BtnOnPos = new Vector3(-316.3f, -559.9f, 0.0f);
    private Vector3 m_BtnOffPos = new Vector3(-316.3f, -559.9f, 0.0f);

    public Transform m_ScContent;
    public GameObject m_CrSmallPrefab;

    [Header("-------- Skill Timer --------")]
    public GameObject m_SkCoolObj = null;
    public Transform m_SkillTimeTr = null;

    //---- 서버에 전송할 패킷 처리용 큐 관련 변수
    bool isNetworkLock = false;  //Network
    List<PacketType> m_PacketBuff = new List<PacketType>();

    //-----------Fixed JoyStick 처리 부분
    JoyStickType m_JoyStickType = JoyStickType.Fixed;

    [Header("-------- JoyStick --------")]
    public GameObject m_JoySBackObj = null;
    public Image m_JoyStickImg = null;
    float m_Radius = 0.0f;
    Vector3 m_OrignPos = Vector3.zero;
    Vector3 m_Axis = Vector3.zero;
    Vector3 m_JsCacVec = Vector3.zero;
    float m_JsCacDist = 0.0f;

    //-----------Flexible JoyStick 처리 부분
    public GameObject m_JoystickPickPanel = null;
    private Vector2 posJoyBack;
    private Vector2 dirStick;

    void Awake()
    {
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_GameTime = 0.0f;
        Time.timeScale = 1.0f;
        m_InGState = InGameState.GameReady;

        RenewMyCharList();

        m_refHero = FindObjectOfType<Player>();

        readyPanel.SetActive(true);
        bossWarning.SetActive(false);

        //--- Refrash Info 
        m_BestScoreTxt.text = "최고점수 : " + GlobalValue.g_BestScore + " 점";
        m_GoldTxt.text = "보유골드 : " + GlobalValue.g_UserGold + " 원";

        Vector3 a_ScMin = new Vector3(0.0f, 0.0f, 0.0f); //ScreenViewPort 좌측하단
        m_SceenWMin = Camera.main.ViewportToWorldPoint(a_ScMin);

        Vector3 a_ScMax = new Vector3(1.0f, 1.0f, 0.0f); //ScreenViewPort 우측상단
        m_SceenWMax = Camera.main.ViewportToWorldPoint(a_ScMax);

        if (m_CfgBtn != null)
            m_CfgBtn.onClick.AddListener(() =>
            {
                if (m_ConfigBoxObj == null)
                    m_ConfigBoxObj = Resources.Load("ConfigBox") as GameObject;

                GameObject a_CfgBoxObj = (GameObject)Instantiate(m_ConfigBoxObj);
                a_CfgBoxObj.transform.SetParent(Canvas_Dialog.transform, false);

                Time.timeScale = 0.0f;
            });

        if (m_InVen_Btn != null)
        {
            m_InVen_Btn.onClick.AddListener(() =>
            {
                m_InVen_ScOnOff = !m_InVen_ScOnOff;
            });
        }

        m_CoinItem = Resources.Load("CoinItemPrefab") as GameObject;
        m_PowerItem = Resources.Load("powerItem") as GameObject;

        //-----------Fixed JoyStick 처리 부분
        if (m_JoySBackObj != null && m_JoyStickImg != null &&
            m_JoySBackObj.activeSelf == true &&
            m_JoystickPickPanel.activeSelf == false)
        {
            m_JoyStickType = JoyStickType.Fixed;

            Vector3[] v = new Vector3[4];
            m_JoySBackObj.GetComponent<RectTransform>().GetWorldCorners(v);
            //[0]:좌측하단 [1]:좌측상단 [2]:우측상단 [3]:우측하단
            //v[0] 촤측하단이 0, 0 좌표인 스크린 좌표(Screen.width, Screen.height)를 기준으로   
            //RectTransform : 즉 UGUI 좌표 기준
            m_Radius = v[2].y - v[0].y;
            m_Radius = m_Radius / 3.0f;

            m_OrignPos = m_JoyStickImg.transform.position;

            //스크립트로만 대기하고자 할 때 //using UnityEngine.EventSystems;
            EventTrigger trigger = m_JoySBackObj.GetComponent<EventTrigger>();
            // Inspector에서 GameObject.Find("Button"); 에 꼭 
            // AddComponent--> EventTrigger 가 되어 있어야 한다.
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) =>
            {
                OnDragJoyStick((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.EndDrag;
            entry.callback.AddListener((data) =>
            {
                OnEndDragJoyStick((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

        } //if (m_JoySBackObj != null ...)
        //-----------Fixed JoyStick 처리 부분

        //-----------Flexible JoyStick 처리 부분
        if (m_JoystickPickPanel != null && m_JoySBackObj != null
            && m_JoyStickImg != null
            && m_JoystickPickPanel.activeSelf == true)
        {
            if (m_JoySBackObj.activeSelf == true)
            {
                m_JoyStickType = JoyStickType.Flexible;
            }
            else
            {
                m_JoyStickType = JoyStickType.FlexibleOnOff;
            }

            EventTrigger a_JBTrigger = m_JoySBackObj.GetComponent<EventTrigger>();
            if (a_JBTrigger != null)
            {
                Destroy(a_JBTrigger);
            }// 조이스틱 백에 설치되어 있는 이벤트 트리거는 제거한다.

            Vector3[] v = new Vector3[4];
            m_JoySBackObj.GetComponent<RectTransform>().GetWorldCorners(v);
            m_Radius = v[2].y - v[0].y;
            m_Radius = m_Radius / 3.0f;

            m_OrignPos = m_JoyStickImg.transform.position;
            m_JoySBackObj.GetComponent<Image>().raycastTarget = false;
            m_JoyStickImg.raycastTarget = false;

            EventTrigger trigger = m_JoystickPickPanel.GetComponent<EventTrigger>(); // 인스펙터에 EventTrigger 컴포넌트 추가해줘야함
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => {
                OnPointerDown_Flx((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((data) => {
                OnPointerUp_Flx((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) => {
                OnDragJoyStick_Flx((PointerEventData)data);
            });
            trigger.triggers.Add(entry);
        }
        //-----------Flexible JoyStick 처리 부분
    }

    // Update is called once per frame
    void Update()
    {
        if (isNetworkLock == false) //지금 패킷 처리 중인 상태가 아니면...
        {
            if (0 < m_PacketBuff.Count) //대기 패킷이 존재한다면...
            {
                Req_NetWork();
            }
            else //처리할 패킷이 하나도 없다면...
            {
                Exe_GameEnd();
                //매번 처리할 패킷이 하나도 없다면 종료처리 해야 할지 확인한다.
            }
        }

        if (m_InGState == InGameState.GameReady)
        {
            countDown -= Time.deltaTime;
            if (0 <= countDown)
                countTxt.text = ((int)countDown).ToString();
            else if (-1.0f <= countDown)
                countTxt.text = "Start!";
            else
            {
                m_InGState = InGameState.GameIng;
                readyPanel.SetActive(false);
            }
        }

        ScrollViewOnOff_Update();

        if (Input.GetKeyDown(KeyCode.Alpha1) ||
        Input.GetKeyDown(KeyCode.Keypad1))
        {
            UseSkill_Key(CharType.Char_0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Keypad2))
        {
            UseSkill_Key(CharType.Char_1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) ||
              Input.GetKeyDown(KeyCode.Keypad3))
        {
            UseSkill_Key(CharType.Char_2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Keypad4))
        {
            UseSkill_Key(CharType.Char_3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) ||
            Input.GetKeyDown(KeyCode.Keypad5))
        {
            UseSkill_Key(CharType.Char_4);
        }
    }

    void Exe_GameEnd() //execute //실행하다.
    {
        //게임 종료 상태이고 
        if (InGameMgr.m_InGState == InGameState.GameExit)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }
    }

#region  // JoyStick 컨트롤
     void OnDragJoyStick(PointerEventData _data) //Delegate
    {
        if (m_JoyStickImg == null)
            return;

        m_JsCacVec = Input.mousePosition - m_OrignPos;
        m_JsCacVec.z = 0.0f;
        m_JsCacDist = m_JsCacVec.magnitude;
        m_Axis = m_JsCacVec.normalized;

        //조이스틱 백그라운드를 벗어나지 못하게 막는 부분
        if (m_Radius < m_JsCacDist)
        {
            m_JoyStickImg.transform.position =
                                    m_OrignPos + m_Axis * m_Radius;
        }
        else
        {
            m_JoyStickImg.transform.position =
                                    m_OrignPos + m_Axis * m_JsCacDist;
        }

        if (1.0f < m_JsCacDist)
            m_JsCacDist = 1.0f;

        //캐릭터 이동 처리
        if (m_refHero != null)
            m_refHero.SetJoyStickMv(m_JsCacDist, m_Axis);
    }

    void OnEndDragJoyStick(PointerEventData _data) //Delegate
    {
        if (m_JoyStickImg == null)
            return;

        m_Axis = Vector3.zero;
        m_JoyStickImg.transform.position = m_OrignPos;

        m_JsCacDist = 0.0f;

        //캐릭터 정지 처리
        if (m_refHero != null)
            m_refHero.SetJoyStickMv(0.0f, m_Axis);
    }
    //-----------Fixed JoyStick 처리 부분

    //-----------Flexible JoyStick 처리 부분
    void OnPointerDown_Flx(PointerEventData eventData) //마우스 클릭시
    {
        if (eventData.button != PointerEventData.InputButton.Left) //마우스 왼쪽 버튼만
            return;

        if (m_JoySBackObj == null)
            return;

        if (m_JoyStickImg == null)
            return;

        m_JoySBackObj.transform.position = eventData.position;
        m_JoyStickImg.transform.position = eventData.position;

        m_JoySBackObj.SetActive(true);
    }

    void OnPointerUp_Flx(PointerEventData eventData)   //마우스 클릭 해제시
    {
        if (eventData.button != PointerEventData.InputButton.Left) //마우스 왼쪽 버튼만
            return;

        if (m_JoySBackObj == null)
            return;

        if (m_JoyStickImg == null)
            return;

        m_JoySBackObj.transform.position = m_OrignPos;
        m_JoyStickImg.transform.position = m_OrignPos;

        if (m_JoyStickType == JoyStickType.FlexibleOnOff)
        {
            m_JoySBackObj.SetActive(false);  //<---꺼진 상태로 시작하는 방식일 때는 활성화필요
        }

        m_Axis = Vector3.zero;
        m_JsCacDist = 0.0f;
        //m_JoyStickImg.gameObject.SetActive(false);
        //캐릭터 정지 처리
        if (m_refHero != null)
        {
            m_refHero.SetJoyStickMv(0.0f, Vector3.zero);
        }
    }

    void OnDragJoyStick_Flx(PointerEventData eventData) //마우스 드래그
    {
        if (eventData.button != PointerEventData.InputButton.Left) //마우스 왼쪽 버튼만
            return;

        //eventData.position 현재 마우스의 UI 기준의 월드 좌표 : 
        //좌측 하단 0, 0 //우측 상단 Screen.width, Screen.height

        if (m_JoyStickImg == null)
            return;

        posJoyBack = (Vector2)m_JoySBackObj.transform.position;
        //조이스틱 백 그라운드 현재 위치 기준
        m_JsCacDist = Vector2.Distance(posJoyBack, eventData.position); //거리 
        dirStick = eventData.position - posJoyBack;  //방향

        if (m_Radius < m_JsCacDist)
        {
            m_JsCacDist = m_Radius;
            m_JoyStickImg.transform.position =
                (Vector3)(posJoyBack + (dirStick.normalized * m_Radius));
        }
        else
        {
            m_JoyStickImg.transform.position = (Vector3)eventData.position;
        }

        if (1.0f < m_JsCacDist)
            m_JsCacDist = 1.0f;

        m_Axis = (Vector3)dirStick.normalized;

        if (m_refHero != null)
        {
            m_refHero.SetJoyStickMv(m_JsCacDist, m_Axis);
        }
        ////캐릭터 이동 처리  
    }
#endregion

    public void AddGold(int a_Value = 10)
    {
        GlobalValue.g_UserGold += a_Value;
        if (GlobalValue.g_UserGold < 0)
            GlobalValue.g_UserGold = 0;
            
        m_CurGold += a_Value;
        if (m_CurGold < 0)
            m_CurGold = 0;

        m_GoldTxt.text = "보유골드 : " + GlobalValue.g_UserGold + "원";

        PushPacket(PacketType.UserGold);
    }

    public void AddScore(int a_Value = 10)
    {
        m_CurScore += a_Value;
        if (m_CurScore < 0)
            m_CurScore = 0;
   
        int a_MaxValue = int.MaxValue - 10;
        if (a_MaxValue < m_CurScore)
            m_CurScore = a_MaxValue;

        m_CurScoreTxt.text = "현재점수 : " + m_CurScore + "점";
        if (GlobalValue.g_BestScore < m_CurScore)
        {
            GlobalValue.g_BestScore = m_CurScore;
            m_BestScoreTxt.text = "최고점수 : " + GlobalValue.g_BestScore + "점";
           
            PushPacket(PacketType.BestScore);
        }

    }

    public void DamageTxt (float a_Value, Transform txtTr, Color a_Color)
    {

        if (m_DamageObj == null || m_HUD_Canvas == null)
            return;

        a_DamClone = (GameObject)Instantiate(m_DamageObj);
        a_DamClone.transform.SetParent(m_HUD_Canvas);
        a_DamageTx = a_DamClone.GetComponent<DamageTxt>();
        if (a_DamageTx != null)
            a_DamageTx.InitDamage(a_Value, a_Color);
        a_StCacPos = new Vector3(txtTr.position.x, txtTr.position.y + 1.15f, 0.0f);
        a_DamClone.transform.position = a_StCacPos;
    }

    void ScrollViewOnOff_Update()
    {
        if (m_InVenScrollTr == null)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            m_InVen_ScOnOff = !m_InVen_ScOnOff;
        }

        if (m_InVen_ScOnOff == false)
        {
            if (m_InVenScrollTr.localPosition.x > m_ScOffPos.x)
            {
                m_InVenScrollTr.localPosition =
                            Vector3.MoveTowards(m_InVenScrollTr.localPosition,
                                      m_ScOffPos, m_ScSpeed * Time.deltaTime);
            }

            if (m_InVen_Btn.transform.localPosition.x > m_BtnOffPos.x)
            {
                m_InVen_Btn.transform.localPosition =
                        Vector3.MoveTowards(m_InVen_Btn.transform.localPosition,
                                        m_BtnOffPos, m_ScSpeed * Time.deltaTime);
            }
        }

        else //if (m_InVen_ScOnOff == true)
        {
            if (m_ScOnPos.x > m_InVenScrollTr.localPosition.x)
            {
                m_InVenScrollTr.localPosition =
                        Vector3.MoveTowards(m_InVenScrollTr.localPosition,
                                          m_ScOnPos, m_ScSpeed * Time.deltaTime);
            }

            if (m_BtnOnPos.x > m_InVen_Btn.transform.localPosition.x)
            {
                m_InVen_Btn.transform.localPosition =
                        Vector3.MoveTowards(m_InVen_Btn.transform.localPosition,
                                        m_BtnOnPos, m_ScSpeed * Time.deltaTime);
            }
        }
    }

    public void SkillTimeFunc(CharInfo a_CrInfo, float a_Time, float a_Delay)
    {
        GameObject obj = Instantiate(m_SkCoolObj) as GameObject;
        obj.transform.SetParent(m_SkillTimeTr);
        SkillCoolCtrl skill = obj.GetComponent<SkillCoolCtrl>();
        skill.InitState(a_CrInfo, a_Time, a_Delay);
    }

    void RenewMyCharList()  //보유 CharacterItem 목록을 UI에 복원하는 함수
    {
        for (int ii = 0; ii < GlobalValue.m_CrDataList.Count; ii++)
        {
            GlobalValue.m_CrDataList[ii].m_CurSkillCount =
                            GlobalValue.m_CrDataList[ii].m_Level;

            if (GlobalValue.m_CrDataList[ii].m_Level <= 0)
                break;

            GameObject a_CharClone = Instantiate(m_CrSmallPrefab);
            a_CharClone.GetComponent<SkSmallNode>().InitState(GlobalValue.m_CrDataList[ii]);
            a_CharClone.transform.SetParent(m_ScContent, false);
        }
    }

    public void UseSkill_Key(CharType a_CrType)
    {
        if (GlobalValue.m_CrDataList[(int)a_CrType].m_CurSkillCount <= 0)
        {  //스킬 소진으로 사용할 수 없음
            return;
        }

        Player a_Hero = GameObject.FindObjectOfType<Player>();
        if (a_Hero != null)
            a_Hero.UseItem(a_CrType);

        SkSmallNode[] m_CrSmallList;
        if (m_ScContent == null)
            return;

        //--- 아이템 사용 에 대한 UI 갱신 코드
        m_CrSmallList = m_ScContent.GetComponentsInChildren<SkSmallNode>();
        for (int ii = 0; ii < m_CrSmallList.Length; ii++)
        {
            if (m_CrSmallList[ii].m_CrType == a_CrType)
            {
                m_CrSmallList[ii].Refresh_UI(a_CrType);
                break;
            }
        }
        //--- 아이템 사용 에 대한 UI 갱신 코드
    }

    public void PushPacket(PacketType a_PType)
    {
        bool a_isExist = false;
        for (int ii = 0; ii < m_PacketBuff.Count; ii++)
        {
            if (m_PacketBuff[ii] == a_PType) //아직 처리 되지 않은 패킷이 존재하면
                a_isExist = true;
            //또 추가하지 않고 기존 버퍼의 패킷으로 업데이트한다.
        }

        if (a_isExist == false)
            m_PacketBuff.Add(a_PType);
        //대기 중인 이 타입의 패킷이 없으면 새로 추가한다.
    }

    void Req_NetWork() //RequestNetWork
    {
        if (m_PacketBuff[0] == PacketType.BestScore)
        {
            UpdateScoreCo();
        }
        else if (m_PacketBuff[0] == PacketType.UserGold)
        {
            UpdateGoldCo(); //Playfab 서버에 골드갱신 요청 함수
        }
        else if (m_PacketBuff[0] == PacketType.NickUpdate)
        {
            NickChangeCo(m_TempStrBuff);
        }
        else if (m_PacketBuff[0] == PacketType.UpdateExp)
        {
            UpdateExpCo();
        }

        m_PacketBuff.RemoveAt(0);

    } //void Req_NetWork()

    void UpdateScoreCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        var request = new UpdatePlayerStatisticsRequest
        {
            //BestScore, BestLevel, ...
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "BestScore",
                                     Value = GlobalValue.g_BestScore} ,
                //new StatisticUpdate {StatisticName = "BestLevel",
                //                     Value = GlobalValue.g_BestLevel}
            }
        };

        isNetworkLock = true;
        PlayFabClientAPI.UpdatePlayerStatistics(
                request,

                (result) =>
                { //업데이트 성공시 응답 함수
                    isNetworkLock = false;
                },

                (error) =>
                { //업데이트 실패시 응답 함수
                    isNetworkLock = false;
                }
            );
    }

    void UpdateGoldCo() //Playfab 서버에 골드갱신 요청 함수
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        //< 플레이어 데이터(타이틀) >값 활용 코드
        var request = new UpdateUserDataRequest()
        {

            Data = new Dictionary<string, string>()
            {
                    { "UserGold", GlobalValue.g_UserGold.ToString() }
            }
        };

        isNetworkLock = true;
        //PlayFabClientAPI.UpdateUserData(request, UpdateSuccess, UpdateFailure);
        PlayFabClientAPI.UpdateUserData(request,
            (result) =>
            {
                isNetworkLock = false;
                //StatText.text = "데이터 저장 성공"; 
            },
            (error) =>
            {
                isNetworkLock = false;
                //StatText.text = "데이터 저장 실패"; 
                //성공하든 실패하든 로비로 나가는 프로세스는 계속 진행한다.
            }
        );
    } //void UpdateGoldCo()

    void NickChangeCo(string a_NickName)
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        if (a_NickName == "")
            return;

        PlayFabClientAPI.UpdateUserTitleDisplayName(
               new UpdateUserTitleDisplayNameRequest
               {
                   DisplayName = a_NickName
               },
               (result) => {

                   GlobalValue.g_NickName = result.DisplayName;
               },
               (error) =>
               {
                   //Debug.LogError(error.GenerateErrorReport());
                   Debug.Log(error.GenerateErrorReport());
               }
            );
    } //void NickChangeCo(string a_NickName)

    public void UpdateExpCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            return;

        JSONObject a_MkJSON = new JSONObject();
        a_MkJSON["UserExp"] = GlobalValue.g_Exp;
        a_MkJSON["AvatarUrl"] = "TestUrl";
        string a_strJon = a_MkJSON.ToString();
        ////---JSON생성 

        var request = new UpdateAvatarUrlRequest()
        {
            ImageUrl = a_strJon
        };

        isNetworkLock = true;
        PlayFabClientAPI.UpdateAvatarUrl(request,
            (result) =>
            {
                //Debug.Log("데이터 저장 성공");
                isNetworkLock = false;
            },
            (error) =>
            {
                //Debug.Log("데이터 저장 실패");
                //Debug.LogError(error.GenerateErrorReport());
                isNetworkLock = false;
            }
        );
        //----AvatarUrl(유저얼굴사진)을 이용해서 유저의 Level을 저장하는 꼼수
    }


    public void AddExpLevel()
    {
        m_KillCount++;
        if (10 < m_KillCount)
        {
            GlobalValue.g_Exp++; //경험치 Experience
            PushPacket(PacketType.UpdateExp);
            RefreshExpLevel();

            m_KillCount = 0;
        }
    }

    public void RefreshExpLevel()
    {
        int a_CurLv = (int)Mathf.Sqrt((float)GlobalValue.g_Exp); //루트(근 √)
        int a_CurExp = 0;
        int a_TargetExp = 1;
        if (a_CurLv <= 0)  //a_CurLv == 0 일때
        {
            a_CurLv = 0;
        }
        else
        {
            int a_BaseExp = (int)Mathf.Pow(a_CurLv, 2);         //제곱
            int a_NextExp = (int)Mathf.Pow((a_CurLv + 1), 2);
            a_CurExp = GlobalValue.g_Exp - a_BaseExp;
            a_TargetExp = a_NextExp - a_BaseExp;
        }

        if (ExpLevel_Txt == null)
        {
            GameObject a_Extxt = GameObject.Find("ExpLevel_Txt");
            if (a_Extxt != null)
                ExpLevel_Txt = a_Extxt.GetComponent<Text>();
        }

        ExpLevel_Txt.text = "레벨 : " + a_CurLv +  " 경험치 : " + a_CurExp + " / " + a_TargetExp;
                            

    }//public void RefreshExpLevel()
}

