using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameMgr : MonoBehaviour
{
    public static GameMgr Inst = null;

    public Text DunName;
    public Text alarmMsg;
    public Text m_gameTime;

    float gameTime = 0.0f;

    public GameObject GameOverPanel;
    public Button retryBtn;

    [Header("Mon")]
    public Button StageChoice_Btn;
    public Transform[] SpawnPoints;
    public Transform bossSpawnPoint;
    public GameObject m_Skeleton;

    [HideInInspector] public PlayerAtk m_refHero = null;

    [Header("--- Shader ---")]
    public Shader g_AddTexShader = null;   //주인공 데미지 연출용(빨간색으로 변했다 돌아올 때)

    [Header("--- DamageText ---")]
    //----------------- 머리위에 데미지 띄우기용 변수 선언
    public Transform m_Damage_Canvas = null;
    public GameObject m_DamagePrefab = null;
    RectTransform CanvasRect;
    Vector2 screenPos = Vector2.zero;
    Vector2 WdScPos = Vector2.zero;
    //-------------- DamageTxt 카메라 반대편에 있을 때 컬링하기 위한 변수들...

    public Transform[] spawnPoint;

    public GameObject SkelPrefab;
    public GameObject SkelKingPrefab;

    public Text m_GoldTxt;

    [Header("-------- ItemIconImg --------")]
    public Texture[] m_ItemImg = null;

    //---------------------------- Inventory ScrollView
    [Header("-------- Inventory ScrollView OnOff --------")]
    public Button m_InVen_Btn = null;
    public Transform m_InVenScrollTr = null;
    private bool m_InVen_ScOnOff = false;
    private float m_ScSpeed = 1800.0f;
    private Vector3 m_ScOnPos = new Vector3(490.0f, 0.0f, 0.0f);
    private Vector3 m_ScOffPos = new Vector3(800.0f, 0.0f, 0.0f);

    public Transform m_MkInvenContent = null;
    public GameObject m_MkItemMyNode = null;
    public Button m_ItemSell_Btn = null;

    public int m_MonKillCount = 0;

    [Header("-------- Boss --------")]
    public GameObject bossTrigger;
    public GameObject bossAlarm;
    public Button bossOkBtn;
    public Button bossCloseBtn;
    public Text bossAlarmTxt;
    public GameObject bossPrefab;

    // 스킬쿨타임
    private Text m_Skill_Cool_Label = null;
    private Image m_Skell_Cool_Mask = null;
    private Button m_SkillUIBtn = null;
    public Button m_Skill_Btn = null;
    [HideInInspector] public float m_Skill_Cooltime = 0.0f;
    float m_SkillCoolLen = 5.0f;

    void Awake()
    {
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        DunName.gameObject.SetActive(true);
        StartCoroutine("StageStart");

        if (StageChoice_Btn != null)
            StageChoice_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("StageChoice");
            });

        //-------- 인벤토리 판넬 OnOff
        if (m_InVen_Btn != null)
        {
            m_InVen_Btn.onClick.AddListener(() =>
            {
                m_InVen_ScOnOff = !m_InVen_ScOnOff;
                if (m_ItemSell_Btn != null)
                    m_ItemSell_Btn.gameObject.SetActive(m_InVen_ScOnOff);
            });
        }

        //----------------------------- 아이템 판매 버튼 처리
        if (m_ItemSell_Btn != null)
            m_ItemSell_Btn.onClick.AddListener(ItemSellMethod);

        if (bossOkBtn != null)
        {
            bossOkBtn.onClick.AddListener(() =>
            {
                if (m_MonKillCount >= 10)
                {
                    StartCoroutine("bossSpawnAlarm");
                    Destroy(bossAlarm);
                }
                else if (m_MonKillCount < 10)
                {
                    StartCoroutine("bossWarningAlarm");
                }
            });
        }

        if (bossCloseBtn != null)
            retryBtn.onClick.AddListener(() =>
            {
                bossAlarm.SetActive(false);
            });
          
        if (retryBtn != null)
            retryBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("StageChoice");
            });

        m_Skill_Cooltime = 0.0f;

        if (m_Skill_Btn != null)
        {
            m_Skill_Btn.onClick.AddListener(() =>
            {
                if (m_refHero != null)
                    m_refHero.SkillOrder("RainArrow",
                        ref m_SkillCoolLen, ref m_Skill_Cooltime);

                Debug.Log("스킬");
            });

            m_Skill_Cool_Label =
                m_Skill_Btn.transform.GetComponentInChildren<Text>(true);
            m_Skell_Cool_Mask =
                m_Skill_Btn.transform.Find("SkillCoolMask").GetComponent<Image>();

            m_SkillUIBtn = m_Skill_Btn.GetComponent<Button>();
        }
        //------ Skill Button 처리 코드


    }

    // Update is called once per frame
    void Update()
    {
        SkillCool_Update(ref m_Skill_Cooltime, ref m_Skill_Cool_Label,
                                 ref m_Skell_Cool_Mask, m_SkillCoolLen);

        gameTime += Time.deltaTime;
        m_gameTime.text = "" + gameTime.ToString("N2");

        InvenScOnOffUpdate();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (m_refHero != null)
                m_refHero.SkillOrder("RainArrow",
                    ref m_SkillCoolLen, ref m_Skill_Cooltime);

            Debug.Log("스킬");

            m_Skill_Cool_Label =
               m_Skill_Btn.transform.GetComponentInChildren<Text>(true);
            m_Skell_Cool_Mask =
                m_Skill_Btn.transform.Find("SkillCoolMask").GetComponent<Image>();

            m_SkillUIBtn = m_Skill_Btn.GetComponent<Button>();
        }
    }

    void InvenScOnOffUpdate()
    {    //------------- 인벤토리 판넬 OnOff 연출

        if (m_InVenScrollTr == null)
            return;

        if (m_InVen_ScOnOff == false)
        {
            if (m_InVenScrollTr.localPosition.x < m_ScOffPos.x)
            {
                m_InVenScrollTr.localPosition =
                    Vector3.MoveTowards(m_InVenScrollTr.localPosition,
                                        m_ScOffPos, m_ScSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (m_ScOnPos.x < m_InVenScrollTr.localPosition.x)
            {
                m_InVenScrollTr.localPosition =
                    Vector3.MoveTowards(m_InVenScrollTr.localPosition,
                                        m_ScOnPos, m_ScSpeed * Time.deltaTime);
            }
        }
    }

    public void InvenAddItem(GameObject a_Obj)
    {
        ItemObjInfo a_RefItemInfo = a_Obj.GetComponent<ItemObjInfo>();
        if (a_RefItemInfo != null)
        {
            ItemValue a_Node = new ItemValue();
            a_Node.UniqueID = a_RefItemInfo.m_ItemValue.UniqueID;
            a_Node.m_Item_Type = a_RefItemInfo.m_ItemValue.m_Item_Type;
            a_Node.m_ItemName = a_RefItemInfo.m_ItemValue.m_ItemName;
            a_Node.m_ItemLevel = a_RefItemInfo.m_ItemValue.m_ItemLevel;
            a_Node.m_ItemStar = a_RefItemInfo.m_ItemValue.m_ItemStar;
            GlobalUserData.g_ItemList.Add(a_Node);

            AddNodeScrollView(a_Node); //스크롤 뷰에 추가
            GlobalUserData.ReflashItemSave();  //파일 저장
        }
    }

    public void AddNodeScrollView(ItemValue a_Node)
    {
        //---------- ScrollView UI 에 Node 추가
        GameObject m_ItemObj = (GameObject)Instantiate(m_MkItemMyNode);
        //Resources.Load("Prefab/ItemMyNode"));
        m_ItemObj.transform.SetParent(m_MkInvenContent, false);
        //false일 경우 : 로컬 기준의 정보를 유지한 채 차일드화된다.
        ItemNode a_MyItemInfo = m_ItemObj.GetComponent<ItemNode>();

        if (a_MyItemInfo != null)
            a_MyItemInfo.SetItemRsc(a_Node);
        //---------- ScrollView UI 에 Node 추가
    }

    void ItemSellMethod()
    {
        //스크롤뷰의 노드를 모두 돌면서 선택되어 있는 것들만 판매하고
        //해당 유니크ID를 g_ItemList에서 찾아서 제거해 준다.
        ItemNode[] a_MyNodeList =
            m_MkInvenContent.GetComponentsInChildren<ItemNode>(true); //true : Active 꺼져 있는 오브젝트까지 모두 가져오게 됨
        for (int ii = 0; ii < a_MyNodeList.Length; ii++)
        {
            if (a_MyNodeList[ii].m_SelOnOff == false)
                continue;

            for (int a_bb = 0; a_bb < GlobalUserData.g_ItemList.Count; a_bb++)
            {
                if (a_MyNodeList[ii].m_UniqueID ==
                                      GlobalUserData.g_ItemList[a_bb].UniqueID)
                {
                    GlobalUserData.g_ItemList.RemoveAt(a_bb);
                    break;
                }
            }//for (int a_bb = 0; a_bb < GlobalUserData.Instance.g_ItemList.Count; a_bb++)

            Destroy(a_MyNodeList[ii].gameObject);

            AddCoin(1000); //골드 증가

        }//for(int ii = 0; ii < a_MyNodeList.Length; ii++)

        GlobalUserData.ReflashItemSave(); //리스트 다시 저장

    }

    public void ReflashInGameItemScV()  //<---- InGame의 ScrollView  갱신
    { //GlobalUserData.g_ItemList 저장된 값을 ScrollView에 복원해 주는 함수

        ItemNode[] a_MyNodeList =
              m_MkInvenContent.GetComponentsInChildren<ItemNode>(true);
        for (int ii = 0; ii < a_MyNodeList.Length; ii++)
        {
            Destroy(a_MyNodeList[ii].gameObject);

        }//for(int ii = 0; ii < a_MyNodeList.Length; ii++)

        for (int a_ii = 0; a_ii < GlobalUserData.g_ItemList.Count; a_ii++)
        {
            AddNodeScrollView(GlobalUserData.g_ItemList[a_ii]); //In Game Scroll View 아이템 추가 함수)
        }
    }//public void ReflashInGameItemScV() 

    IEnumerator StageStart()
    {
        // Wave1
        DunName.text = "해골 묘지";
        yield return new WaitForSeconds(4f);
        DunName.gameObject.SetActive(false);
        alarmMsg.gameObject.SetActive(true);
        alarmMsg.text = "몰려오는 적들을 처치하세요";
        yield return new WaitForSeconds(2f);
        alarmMsg.text = "Wave 1 Start";
        yield return new WaitForSeconds(4f);
        alarmMsg.gameObject.SetActive(false);

        for (int i = 0; i < spawnPoint.Length; i++)
        {
            Instantiate(SkelPrefab, spawnPoint[i].transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(25f);

        // Wave2
        alarmMsg.gameObject.SetActive(true);
        alarmMsg.text = "Wave 2 Start";
        yield return new WaitForSeconds(4f);
        alarmMsg.gameObject.SetActive(false);
        Instantiate(SkelKingPrefab, spawnPoint[5].transform.position, Quaternion.identity);
        Instantiate(SkelKingPrefab, spawnPoint[12].transform.position, Quaternion.identity);


    }

    IEnumerator StageClear()
    {
        alarmMsg.gameObject.SetActive(true);
        alarmMsg.text = "해골 묘지의 주인을 처치했습니다";
        yield return new WaitForSeconds(1.5f);
        alarmMsg.text = "Stage Clear";
        yield return new WaitForSeconds(1.5f);
        alarmMsg.gameObject.SetActive(false);
    }


    Vector3 a_StCacPos = Vector3.zero;
    public void SpawnDamageTxt(int dmg, Transform txtTr, int a_ColorIdx = 0)
    {
        if (m_DamagePrefab != null && m_Damage_Canvas != null)
        {
            GameObject m_DamageObj = (GameObject)Instantiate(m_DamagePrefab);
            a_StCacPos = new Vector3(txtTr.position.x,
                                     txtTr.position.y + 2.65f, txtTr.position.z);

            m_DamageObj.transform.SetParent(m_Damage_Canvas, false);
            DamageText a_DamageTx = m_DamageObj.GetComponent<DamageText>();
            a_DamageTx.m_BaseWdPos = a_StCacPos;
            a_DamageTx.m_DamageVal = (int)dmg;

            //초기 위치 잡아 주기 //--World 좌표를 UGUI 좌표로 환산해 주는 코드
            CanvasRect = m_Damage_Canvas.GetComponent<RectTransform>();
            screenPos = Camera.main.WorldToViewportPoint(a_StCacPos);
            WdScPos.x = ((screenPos.x * CanvasRect.sizeDelta.x) -
                                        (CanvasRect.sizeDelta.x * 0.5f));
            WdScPos.y = ((screenPos.y * CanvasRect.sizeDelta.y) -
                                        (CanvasRect.sizeDelta.y * 0.5f));
            m_DamageObj.GetComponent<RectTransform>().anchoredPosition = WdScPos;
            //--World 좌표를 UGUI 좌표로 환산해 주는 코드

            if (a_ColorIdx == 1) //주인공 일때 데미지 택스트 색 바꾸기...
            {
                Outline a_Outline = m_DamageObj.GetComponentInChildren<Outline>();
                a_Outline.effectColor = new Color32(255, 255, 255, 0);
                a_Outline.enabled = false;

                Text a_RefText = m_DamageObj.GetComponentInChildren<Text>();
                a_RefText.color = new Color32(255, 255, 230, 255);
            }

            if (a_ColorIdx == 2) //주인공 일때 데미지 택스트 색 바꾸기...
            {
                Outline a_Outline = m_DamageObj.GetComponentInChildren<Outline>();
                a_Outline.effectColor = new Color32(255, 255, 255, 0);
                a_Outline.enabled = false;

                Text a_RefText = m_DamageObj.GetComponentInChildren<Text>();
                a_RefText.color = new Color32(255, 255, 0, 255);
            }
        }
    }

    public void AddCoin(int a_Val = 500)
    {
        GlobalUserData.s_GoldCount = GlobalUserData.s_GoldCount + a_Val;
        m_GoldTxt.text = "Gold : " + GlobalUserData.s_GoldCount.ToString("N0");

        PlayerPrefs.SetInt("GoldCount", GlobalUserData.s_GoldCount);
    }

    public void AddCristal(int a_Val = 1)
    {
        GlobalUserData.s_CristalCount = GlobalUserData.s_CristalCount + a_Val;
        m_GoldTxt.text = "Cristal : " + GlobalUserData.s_CristalCount.ToString("N0");

        PlayerPrefs.SetInt("CristalCount", GlobalUserData.s_CristalCount);
    }

    public void AddMonKill(int a_Val = 1)
    {
        m_MonKillCount = m_MonKillCount + a_Val;
    }

    IEnumerator bossSpawnAlarm()
    {
        alarmMsg.gameObject.SetActive(true);
        alarmMsg.text = "해골 묘지의 주인이 나타났습니다";
        yield return new WaitForSeconds(2.0f);
        Instantiate(bossPrefab, bossSpawnPoint.transform.position, Quaternion.identity);
        alarmMsg.gameObject.SetActive(false);
    }

    IEnumerator bossWarningAlarm()
    {
        bossAlarmTxt.gameObject.SetActive(true);
        bossAlarmTxt.text = "조건이 충족되지 않습니다";
        yield return new WaitForSeconds(1.5f);
        bossAlarmTxt.gameObject.SetActive(false);
    }

    public void GameOver()
    {
        GameOverPanel.SetActive(true);
    }

    void SkillCool_Update(ref float Cool_float, ref Text Cool_Label,
                               ref Image Cool_Sprite, float Max_Cool)
    {
        if (0.0f < Cool_float)
        {
            Cool_float -= Time.deltaTime;
            Cool_Label.text = ((int)Cool_float).ToString();
            Cool_Sprite.fillAmount = Cool_float / Max_Cool;

            if (m_SkillUIBtn != null)
                m_SkillUIBtn.enabled = false;
        }
        else
        {
            Cool_float = 0.0f;
            Cool_Sprite.fillAmount = 0.0f;
            Cool_Label.text = "";

            if (m_SkillUIBtn != null)
                m_SkillUIBtn.enabled = true;
        }
    }
}
