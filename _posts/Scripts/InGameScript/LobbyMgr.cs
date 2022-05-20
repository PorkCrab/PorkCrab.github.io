using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using SimpleJSON;  

public class LobbyMgr : MonoBehaviour
{
    [Header("Button")]
    public Button Start_Btn = null;
    public Button Store_Btn = null;
    public Button Logout_Btn = null;
    public Button Refresh_Btn = null;

    [Header("Normal")]
    public Text MyInfo_Text = null;
    public Text Ranking_Text = null;

    int m_My_Rank = 0;

    float RestoreTimer = 0.0f; //랭킹 갱신 타이머
    public Button RestRk_Btn;  //Restore Ranking Button
    float DelayGetLB = 3.0f;   //로비 진입 후 3.0f초 뒤에 랭킹(리더보드) 한번 더 로딩하기..

    float ShowMsTimer = 0.0f;  //메시지를 몇 초동안 보이게 할건지에 대한 타이머
    public Text MessageText;   //메시지 내용을 표시할 UI

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f; //원래 속도로...
        GlobalValue.InitData();

        Ranking_Text.text = "";

        if (Logout_Btn != null)
            Logout_Btn.onClick.AddListener(() =>
            {
                GlobalValue.g_Unique_ID = "";
                GlobalValue.g_NickName = "";
                GlobalValue.g_BestScore = 0;
                GlobalValue.g_UserGold = 0;

                PlayFabClientAPI.ForgetAllCredentials(); 
                SceneManager.LoadScene("TitleScene");
            });

        if (Start_Btn != null)
            Start_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("InGame");
            });

        if (Store_Btn != null)
            Store_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("StoreScene");
            });

#if AutoRestore
        //<---자동 리셋인 경우
        RestoreTimer = 10.0f;  //<---자동 리셋 //초기 딜레이 = 3.0f + 7.0f;
        if (RestRk_Btn != null) //<---수동 리셋
            RestRk_Btn.gameObject.SetActive(false);
        //<---자동 리셋인 경우
#else
        //< ---수동 리셋인 경우
        if (RestRk_Btn != null) //<---수동 리셋
            RestRk_Btn.onClick.AddListener(RestoreRank);
        //< ---수동 리셋인 경우
#endif

        GetLeaderboard();  //랭킹 불러오기...

        RefreshMyInfo(); //우선 셋팅해 놓고 내 등수 불러온 후 다시 갱신
    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < DelayGetLB)
        {
            DelayGetLB -= Time.deltaTime;
            if (DelayGetLB <= 0.0f)
            {
                GetLeaderboard();
            }
        }

#if AutoRestore
        //<---자동 리셋인 경우
        RestoreTimer -= Time.deltaTime;
        if (RestoreTimer <= 0.0f)
        {
            GetLeaderboard();
            RestoreTimer = 7.0f;  //주기
        }
        //<---자동 리셋인 경우
#else
        //< ---수동 리셋인 경우
        if (0.0f < RestoreTimer)
        {
            RestoreTimer -= Time.deltaTime;
        }
        //< ---수동 리셋인 경우
#endif

        if (0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if (ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false); //메시지 끄기
            }
        }
    }

    void RefreshMyInfo()
    {
        MyInfo_Text.text = "이름(" + GlobalValue.g_NickName +
                   ") : 순위(" + m_My_Rank + "등) : 점수(" +
                   GlobalValue.g_BestScore.ToString() + "점) : 골드(" +
                   GlobalValue.g_UserGold.ToString() + ")";
    }

    void MessageOnOff(string Mess = "", bool isOn = true)
    {
        if (isOn == true)
        {
            MessageText.text = Mess;
            MessageText.gameObject.SetActive(true);
            ShowMsTimer = 5.0f;
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);
        }
    }

    void RestoreRank()  //<---수동 리셋인 경우
    {
        if (0.0f < RestoreTimer)
        {
            MessageOnOff("최소 7초 주기로만 갱신됩니다.");
            return;
        }

        DelayGetLB = 0.0f;  //딜레이 로딩 즉시 취소하고 수동로딩 우선으로...
        GetLeaderboard();

        RestoreTimer = 7.0f;
    }

    void GetLeaderboard()  //순위 불러오기...
    {
        if (GlobalValue.g_Unique_ID == "") //로그인 상태에서만...
            return;

        var request = new GetLeaderboardRequest
        {
            StartPosition = 0,           //0번인덱스 즉 1등부터
            StatisticName = "BestScore", //관리자페이지의 순위표 변수 중 "BestScore" 기준
            MaxResultsCount = 10,        //10명까지
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true,  //닉네임도 요청
                ShowAvatarUrl = true
                //유저 사진 썸네일 주소도 요청(이건 경험치로 사용)
            }
        };

        PlayFabClientAPI.GetLeaderboard(request,
                 (result) =>
                 { //랭킹 리스트 받아오기 성공
                 if (Ranking_Text == null)
                         return;

                     Ranking_Text.text = "";

                     for (int i = 0; i < result.Leaderboard.Count; i++)
                     {
                         var curBoard = result.Leaderboard[i];
                         int a_curExp = ExpMyJsonParser(curBoard.Profile.AvatarUrl);
                         int a_curLv = (int)Mathf.Sqrt((float)a_curExp);

                     //등수 안에 내가 있다면 색 표시
                     if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                             Ranking_Text.text += "<color=#00ff00>";

                         Ranking_Text.text += (i + 1).ToString() + "등 : " +
                         curBoard.DisplayName + " : " +
                         curBoard.StatValue + "점 : " + "Lv " + a_curLv + "\n";

                     //Ranking_Text.text += "\n";

                     //등수 안에 내가 있다면 색 표시
                     if (curBoard.PlayFabId == GlobalValue.g_Unique_ID)
                             Ranking_Text.text += "</color>";

                     } //for (int i = 0; i < result.Leaderboard.Count; i++)

                 GetMyRanking(); //리더보드 등수를 불러온 직 후 내 등수를 불러 온다.
             },
                 (error) =>
                 {
                     Debug.Log("리더보드 불러오기 실패");
                 }
        );
    }

    void GetMyRanking() //나의 등수 불러 오기...
    {
        //원래 GetLeaderboardAroundPlayer는 
        //특정 PlayFabId 주변으로 리스트를 불러오는 함수이다.
        var request = new GetLeaderboardAroundPlayerRequest
        {
            //PlayFabId = GlobalValue.g_Unique_ID,
            //지정하지 않으면 내 등수를 얻어오게 된다.
            StatisticName = "BestScore",
            MaxResultsCount = 1, //한명에 정보만 받아오라는 뜻
                                 //GetPlayerStatistics = true, 
                                 //- 이 옵션으로 통계값(순위표에 관여하는)을 불러올 수 있다.
                                 //ProfileConstraints = new PlayerProfileViewConstraints() 
                                 //{ ShowDisplayName = true }
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request,
            (result) =>
            {
                if (MyInfo_Text == null)
                    return;

                if (0 < result.Leaderboard.Count)
                {
                    var curBoard = result.Leaderboard[0];
                    m_My_Rank = curBoard.Position + 1; //내 등수 가져오기...
                GlobalValue.g_BestScore = curBoard.StatValue; //내 점수 갱신
            } //if (0 < result.Leaderboard.Count)

            RefreshMyInfo(); //<-- UI 갱신
        },
            (error) =>
            {
                Debug.Log("내 등수 불러오기 실패");
            }
        );
    }// void GetMyRanking() //나의 등수 불러 오기...

    int ExpMyJsonParser(string AvatarUrl)
    {
        int a_Exp = 0;
        //------------- 경험치 가져오기
        ////---JSON파싱 
        if (string.IsNullOrEmpty(AvatarUrl) == true)
            return 0;

        if (AvatarUrl.Contains("{\"") == false)
            return 0;

        JSONNode a_ParseJs = JSON.Parse(AvatarUrl);
        if (a_ParseJs["UserExp"] != null)
        {
            a_Exp = a_ParseJs["UserExp"].AsInt;
            return a_Exp;
        }
        ////---JSON파싱 
        //------------- 경험치 가져오기
        return 0;
    }
}
