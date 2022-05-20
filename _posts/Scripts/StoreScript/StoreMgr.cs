using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class StoreMgr : MonoBehaviour
{
    public Button m_ReturnBtn = null;
    public Text m_UserInfoTxt;

    public GameObject m_Item_ScrollContent; //ScrollContent 차일드로 생성될 Parent 객체
    public GameObject m_Item_NodeObj = null; // Node Prefab

    SkillNodeCtrl[] m_CrNodeList;      //<---스크롤에 붙어 있는 Item 목록들...

    //-- 지금 뭘 구입하려고 시도한 건지?
    CharType m_BuyCrType;
    int m_SvMyGold = 0;  //서버에 전달하려고 하는 차감된 내 골드가 얼마인지?
    //-- 지금 뭘 구입하려고 시도한 건지?

    // Start is called before the first frame update
    void Start()
    {
        GlobalValue.InitData();

        if (m_ReturnBtn != null)
            m_ReturnBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("LobbyScene");
            });

        m_UserInfoTxt.text = "보유골드 : " + GlobalValue.g_UserGold + " 원";

        //----------------- 아이템 목록 추가
        GameObject a_ItemObj = null;
        SkillNodeCtrl a_CrItemNode = null;
        for (int ii = 0; ii < GlobalValue.m_CrDataList.Count; ii++)
        {
            a_ItemObj = (GameObject)Instantiate(m_Item_NodeObj);
            a_CrItemNode = a_ItemObj.GetComponent<SkillNodeCtrl>();

            a_CrItemNode.InitData(GlobalValue.m_CrDataList[ii].m_CrType);

            a_ItemObj.transform.SetParent(m_Item_ScrollContent.transform, false);
            // false Prefab의 로컬 포지션을 유지하면서 추가해 주겠다는 뜻.
        }

        RefreshCrItemList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RefreshCrItemList()
    {  //Count == 0 인 상태가 처음 나오는 아이템만 구매가능으로 표시해 준다.

        if (m_Item_ScrollContent != null)
        {
            if (m_CrNodeList == null || m_CrNodeList.Length <= 0)
                m_CrNodeList = m_Item_ScrollContent.GetComponentsInChildren<SkillNodeCtrl>();
        }

        int a_FindAv = -1;
        for (int ii = 0; ii < GlobalValue.m_CrDataList.Count; ii++)
        {
            if (m_CrNodeList[ii].m_CrType != GlobalValue.m_CrDataList[ii].m_CrType)
                continue;

            if (0 < GlobalValue.m_CrDataList[ii].m_Level) //구입상태
            {
                m_CrNodeList[ii].SetState(CrState.Active,
                                        GlobalValue.m_CrDataList[ii].m_UpPrice,
                                        GlobalValue.m_CrDataList[ii].m_Level);
                continue;
            }

            if (a_FindAv < 0)
            {
                //m_Level이 0보다 작은 상태가 처음 나온 상태
                //구입가능 표시
                m_CrNodeList[ii].SetState(CrState.BeforeBuy,
                                 GlobalValue.m_CrDataList[ii].m_Price);

                a_FindAv = ii;
            }
            else
            {
                //전부 Lock 표시
                m_CrNodeList[ii].SetState(CrState.Lock,
                                GlobalValue.m_CrDataList[ii].m_Price);
            }
        }
    }

    public void BuyCharItem(CharType a_ChType)
    {//리스트뷰에 있는 캐릭터 가격버튼을 눌러 구입시도를 한 경우
        m_BuyCrType = a_ChType;
        BuyBeforeJobCo();
    }

    bool isDifferent = false;
    void BuyBeforeJobCo() //구매 1단계 함수
    { //(서버로부터 골드, 아이템 상태 받아와서 클라이언트와 동기화 시켜주기..)

        if (GlobalValue.g_Unique_ID == "")
        {
            return;            //로그인 실패 상태라면 그냥 리턴
        }

        isDifferent = false;

        //< 플레이어 데이터(타이틀) >값 활용 코드
        //using PlayFab;
        //using PlayFab.ClientModels;
        var request = new GetUserDataRequest()
        {
            PlayFabId = GlobalValue.g_Unique_ID
        };

        PlayFabClientAPI.GetUserData(request,
            (result) =>
            {   //유저 정보 받아오기 성공 했을 때
                PlayerDataParse(result);
            },
            (error) =>
            {  //유저 정보 받아오기 실패 했을 때
                Debug.Log("데이터 불러오기 실패");
            }
        );
    }//void BuyBeforeJobCo() //구매 1단계 함수

    void PlayerDataParse(GetUserDataResult result)
    {
        int a_GetValue = 0;
        int Idx = 0;
        foreach (var eachData in result.Data)
        {
            if (eachData.Key == "UserGold")
            {
                if (int.TryParse(eachData.Value.Value, out a_GetValue) == false)
                    continue;

                if (a_GetValue != GlobalValue.g_UserGold)
                    isDifferent = true;
                GlobalValue.g_UserGold = a_GetValue;
            }
            else if (eachData.Key.Contains("ChrItem_") == true)
            {
                Idx = 0;
                string[] strArr = eachData.Key.Split('_');
                if (2 <= strArr.Length)
                {
                    if (int.TryParse(strArr[1], out Idx) == false)
                        Debug.Log("string -> int : TryParse 실패");
                }

                if (GlobalValue.m_CrDataList.Count <= Idx)
                    continue;

                if (int.TryParse(eachData.Value.Value, out a_GetValue) == false)
                    Debug.Log("string -> int : TryParse 실패");

                if (a_GetValue != GlobalValue.m_CrDataList[Idx].m_Level)
                    isDifferent = true;
                GlobalValue.m_CrDataList[Idx].m_Level = a_GetValue;
            }
        }

        string a_Mess = "";
        CrState a_CrState = CrState.Lock;
        bool a_NeedDelegate = false;
        CharInfo a_CrInfo = GlobalValue.m_CrDataList[(int)m_BuyCrType];
        if (m_CrNodeList != null && (int)m_BuyCrType < m_CrNodeList.Length)
        {
            a_CrState = m_CrNodeList[(int)m_BuyCrType].m_CrState;
        }

        if (a_CrState == CrState.Lock) //잠긴 상태
        {
            a_Mess = "이 아이템은 Lock 상태로 구입할 수 없습니다.";
        }
        else if (a_CrState == CrState.BeforeBuy) //구매 가능 상태
        {
            if (GlobalValue.g_UserGold < a_CrInfo.m_Price)
            {
                a_Mess = "보유(누적) 골드가 모자랍니다.";
            }
            else
            {
                a_Mess = "정말 구입하시겠습니까?";
                a_NeedDelegate = true; //-----> 이 조건일 때 구매
            }
        }
        else if (a_CrState == CrState.Active) //활성화(업그레이드가능) 상태
        {
            int a_Cost = a_CrInfo.m_UpPrice +
                         (a_CrInfo.m_UpPrice * (a_CrInfo.m_Level - 1));
            if (5 <= a_CrInfo.m_Level)
            {
                a_Mess = "최고 레벨입니다.";
            }
            else if (GlobalValue.g_UserGold < a_Cost)
            {
                a_Mess = "레벨업에 필요한 보유(누적) 골드가 모자랍니다.";
            }
            else
            {
                a_Mess = "정말 업그레이드하시겠습니까?";
                //-----> 이 조건일 때 업그레이드
                a_NeedDelegate = true; //-----> 이 조건일 때 구매
            }
        }//else if (a_CrState == CrState.Active) 
        if (isDifferent == true)
            a_Mess += "\n(서버와 다른 정보가 있어서 수정되었습니다.)";

        GameObject a_DlgRsc = Resources.Load("DlgBox") as GameObject;
        GameObject a_DlgBoxObj = (GameObject)Instantiate(a_DlgRsc);
        GameObject a_Canvas = GameObject.Find("Canvas");
        a_DlgBoxObj.transform.SetParent(a_Canvas.transform, false);
        // false Prefab의 로컬 포지션을 유지하면서 추가해 주겠다는 뜻.
        DlgCtrl a_DlgBox = a_DlgBoxObj.GetComponent<DlgCtrl>();
        if (a_DlgBox != null)
        {
            if (a_NeedDelegate == true)
                a_DlgBox.SetMessage(a_Mess, TryBuyCrItem);
            else
                a_DlgBox.SetMessage(a_Mess);
        }
    }

    List<int> a_SetLevel = new List<int>();
    public void TryBuyCrItem()  //구매 2단계 함수 
    {   //(서버에 전달할 변동된 데이터 값 만들기...)

        bool a_BuyOK = false;
        CharInfo a_CrInfo = null;
        a_SetLevel.Clear();   //서버에 값을 전달하기 위한 배열

        for (int ii = 0; ii < GlobalValue.m_CrDataList.Count; ii++)
        {
            a_CrInfo = GlobalValue.m_CrDataList[ii];
            a_SetLevel.Add(a_CrInfo.m_Level);

            if (ii != (int)m_BuyCrType || 5 <= a_CrInfo.m_Level) //구매 조건 체크
                continue;

            int a_Cost = a_CrInfo.m_Price;
            if (0 < a_CrInfo.m_Level)
                a_Cost = a_CrInfo.m_UpPrice +
                    (a_CrInfo.m_UpPrice * (a_CrInfo.m_Level - 1));

            if (GlobalValue.g_UserGold < a_Cost)
                continue;

            //1, 여기서 계산(차감)하고 서버에 결과값을 전달한다.
            //GlobalValue.g_UserGold -= a_Cost; 골드값 차감하기 
            //a_SetLevel[ii]++; 레벨증가 

            //2, 서버로부터 응답을 받은 다음에 계산(차감)해 주는 방법도 있다.
            m_SvMyGold = GlobalValue.g_UserGold;
            m_SvMyGold -= a_Cost; //골드값 차감하기 백업해 놓기
            a_SetLevel[ii]++;     //레벨증가 백업해 놓기

            a_BuyOK = true; //서버에 아이템 구매 요청이 확실히 필요하다는 의미 
        }

        if (a_BuyOK == true)
            BuyRequestCo();

    } //public void TryBuyCrItem()  //구매 2단계 함수 

    void BuyRequestCo() //구매 3단계 함수 (서버에 데이터 값 전달하기...)
    {
        if (GlobalValue.g_Unique_ID == "")
            return;            //로그인 상태가 아니면 그냥 리턴

        if (a_SetLevel.Count <= 0)
            return;            //아이템 목록이 정상적으로 만들어지지 않았으면 리턴

        Dictionary<string, string> a_ItemList = new Dictionary<string, string>();
        string a_MkKey = "";
        a_ItemList.Clear();
        a_ItemList.Add("UserGold", m_SvMyGold.ToString());
        //Dictionary 노드 추가 방법
        for (int ii = 0; ii < GlobalValue.m_CrDataList.Count; ii++)
        {
            a_MkKey = "ChrItem_" + ii.ToString();
            a_ItemList.Add(a_MkKey, a_SetLevel[ii].ToString());
        }

        var request = new UpdateUserDataRequest()
        {
            Data = a_ItemList
        };

        PlayFabClientAPI.UpdateUserData(request,
            (result) =>
            {
                //StatText.text = "데이터 저장 성공"; 
                //매뉴상태를 갱신해 주어야 한다.
                RefreshMyInfoCo();
            },
            (error) =>
            {
                //StatText.text = "데이터 저장 실패"; 
            }
         );

    }

    void RefreshMyInfoCo() //구매 4단계 함수 (로컬의 변수, UI 값들을 갱신해 주기...)
    {
        if (m_BuyCrType < CharType.Char_0 || CharType.CrCount <= m_BuyCrType)
            return;

        GlobalValue.g_UserGold = m_SvMyGold;
        GlobalValue.m_CrDataList[(int)m_BuyCrType].m_Level
                                    = a_SetLevel[(int)m_BuyCrType];

        RefreshCrItemList();
        m_UserInfoTxt.text = "보유골드 : " + GlobalValue.g_UserGold + " 원";
    }
}
