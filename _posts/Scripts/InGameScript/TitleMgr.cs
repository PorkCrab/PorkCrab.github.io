using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using SimpleJSON;

//----------------- 이메일형식이 맞는지 확인하는 방법 스크립트
using System.Globalization;
using System.Text.RegularExpressions;
using System;
//----------------- 이메일형식이 맞는지 확인하는 방법 스크립트

public class TitleMgr : MonoBehaviour
{
    public Button Start_Btn = null;

    public AudioClip titleBGM;
    AudioSource audioSource;

    [Header("LoginPanel")]              //이렇게 쓰면 편집창에 태그들이 나온다.
    public GameObject m_LoginPanelObj;
    public Button m_LoginBtn = null;
    public Button m_CreateAccOpenBtn = null;
    public InputField IDInputField;     //Email 로 받을 것임
    public InputField PassInputField;

    [Header("CreateAccountPanel")]
    public GameObject m_CreateAccPanelObj;
    public InputField New_IDInputField;  //Email 로 받을 것임
    public InputField New_PassInputField;
    public InputField New_NickInputField;
    public Button m_CreateAccountBtn = null;
    public Button m_CancelButton = null;

    [Header("Normal")]
    public Text MessageText;
    float ShowMsTimer = 0.0f;

    bool invalidEmailType = false;       // 이메일 포맷이 올바른지 체크
    bool isValidFormat = false;          // 올바른 형식인지 아닌지 체크

    void Awake()
    {
        this.audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource.clip = titleBGM;
        audioSource.Play();

        if (Start_Btn != null)
            Start_Btn.onClick.AddListener(() =>
            {
                m_LoginPanelObj.SetActive(true);
            });

        GlobalValue.InitData();

        //------- LoginPanel
        if (m_LoginBtn != null)
            m_LoginBtn.onClick.AddListener(LoginBtn);

        if (m_CreateAccOpenBtn != null)
            m_CreateAccOpenBtn.onClick.AddListener(OpenCreateAccBtn);

        //------- CreateAccountPanel
        if (m_CancelButton != null)
            m_CancelButton.onClick.AddListener(CreateCancelBtn);

        if (m_CreateAccountBtn != null)
            m_CreateAccountBtn.onClick.AddListener(CreateAccountBtn);

    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if (ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false); //메시지 끄기
            }
        }
    }

    public void LoginBtn()
    {
        string a_IdStr = IDInputField.text;
        string a_PwStr = PassInputField.text;

        if (a_IdStr.Trim() == "" || a_PwStr.Trim() == "")
        {
            MessageOnOff("ID, PW 빈칸 없이 입력해 주셔야 합니다.");
            return;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length < 20))  //3~20
        {
            MessageOnOff("ID는 3글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }
        if (!(6 <= a_PwStr.Length && a_PwStr.Length < 20))  //6~100
        {
            MessageOnOff("비밀번호는 6글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        if (!CheckEmailAddress(IDInputField.text))
        {
            MessageOnOff("Email 형식이 맞지 않습니다.");
            return;
        }

        //------- 이 옵션을 추가해 줘야 로그인하면서 유저의 각종 정보를 가져올 수 있다.
        var option = new GetPlayerCombinedInfoRequestParams()
        {
            //- 이 옵션으로 DisplayName(닉네임), AvatarUrl을 가져올 수 있다.
            GetPlayerProfile = true,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true,  //DisplayName(닉네임) 가져오길 요청하는 옵션
                ShowAvatarUrl = true     //아바타 URL을 가져오는 옵션
            },
            //- 이 옵션으로 DisplayName(닉네임), AvatarUrl을 가져올 수 있다.

            GetPlayerStatistics = true,
            //- 이 옵션으로 통계값(순위표에 관여하는)을 불러올 수 있다.
            GetUserData = true
            //- 이 옵션으로 < 플레이어 데이터(타이틀) >값을 불러올 수 있다.
        };
        //------- 이 옵션을 추가해 줘야 로그인하면서 유저의 각종 정보를 가져올 수 있다.

        var request = new LoginWithEmailAddressRequest
        {
            Email = IDInputField.text,
            Password = PassInputField.text,
            InfoRequestParameters = option
        };

        PlayFabClientAPI.LoginWithEmailAddress(request,
                                        OnLoginSuccess, OnLoginFailure);

    }//public void LoginBtn()

    void OnLoginSuccess(LoginResult result)
    {
        MessageOnOff("로그인 성공");

        GlobalValue.g_Unique_ID = result.PlayFabId;

        if (result.InfoResultPayload != null)
        {
            GlobalValue.g_NickName = result.InfoResultPayload.PlayerProfile.DisplayName;

            //------------- 경험치 가져오기
            string a_AvatarUrl = result.InfoResultPayload.PlayerProfile.AvatarUrl;
            ////---JSON파싱 
            if (string.IsNullOrEmpty(a_AvatarUrl) == false &&
                a_AvatarUrl.Contains("{\"") == true) //JSON 형식인지 확인하는 코드
            {
                JSONNode a_ParseJs = JSON.Parse(a_AvatarUrl); //using SimpleJSON; 
                if (a_ParseJs["UserExp"] != null)
                {
                    GlobalValue.g_Exp = a_ParseJs["UserExp"].AsInt;
                }
            }//if (string.IsNullOrEmpty(

            foreach (var eachStat in result.InfoResultPayload.PlayerStatistics)
            {
                if (eachStat.StatisticName == "BestScore")
                {
                    GlobalValue.g_BestScore = eachStat.Value;
                }
            }

            int a_GetValue = 0;
            int Idx = 0;
            foreach (var eachData in result.InfoResultPayload.UserData)
            {
                if (eachData.Key == "UserGold")
                {
                    if (int.TryParse(eachData.Value.Value, out a_GetValue) == true)
                        GlobalValue.g_UserGold = a_GetValue;
                }
                else if (eachData.Key.Contains("ChrItem_") == true)
                {
                    Idx = 0;
                    string[] strArr = eachData.Key.Split('_');
                    if (2 <= strArr.Length)
                    {
                        if (int.TryParse(strArr[1], out Idx) == false)
                            MessageOnOff("string -> int : TryParse 실패");
                    }

                    if (GlobalValue.m_CrDataList.Count <= Idx)
                        continue;

                    if (int.TryParse(eachData.Value.Value, out a_GetValue) == false)
                        MessageOnOff("string -> int : TryParse 실패");

                    GlobalValue.m_CrDataList[Idx].m_Level = a_GetValue;
                }
            }
        }

        SceneManager.LoadScene("LobbyScene");

    }//void OnLoginSuccess(LoginResult result)

    private void OnLoginFailure(PlayFabError error)
    {
        MessageOnOff("로그인 실패 : " + error.GenerateErrorReport());
    }

    public void OpenCreateAccBtn()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(false);

        if (m_CreateAccPanelObj != null)
            m_CreateAccPanelObj.SetActive(true);
    }

    public void CreateCancelBtn()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(true);

        if (m_CreateAccPanelObj != null)
            m_CreateAccPanelObj.SetActive(false);
    }

    public void CreateAccountBtn() //계정 생성 요청 함수
    {
        string a_IdStr = New_IDInputField.text;
        string a_PwStr = New_PassInputField.text;
        string a_NickStr = New_NickInputField.text;

        if (a_IdStr.Trim() == "" || a_PwStr.Trim() == "" || a_NickStr.Trim() == "")
        {
            MessageOnOff("ID, PW, 별명 빈칸 없이 입력해 주셔야 합니다.");
            return;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length < 20))  //3~20
        {
            MessageOnOff("ID는 3글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        if (!(6 <= a_PwStr.Length && a_PwStr.Length < 20))  //6~100
        {
            MessageOnOff("비밀번호는 6글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        if (!(2 <= a_NickStr.Length && a_NickStr.Length < 20))  //2~20
        {
            MessageOnOff("별명은 2글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        if (!CheckEmailAddress(a_IdStr))
        {
            MessageOnOff("Email 형식이 맞지 않습니다.");
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            Email = New_IDInputField.text,
            Password = New_PassInputField.text,
            DisplayName = New_NickInputField.text,  //Username = New_NickInputField.text(Username <-- 앤 한글이 안된다.), 
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request,
                                RegisterSuccess, RegisterFailure);

    }

    private void RegisterSuccess(RegisterPlayFabUserResult result)
    {
        MessageOnOff("가입 성공");
    }

    private void RegisterFailure(PlayFabError error)
    {
        MessageOnOff("가입 실패 : " + error.GenerateErrorReport());
    }

    void MessageOnOff(string Mess = "", bool isOn = true)
    {
        if (isOn == true)
        {
            MessageText.text = Mess;
            MessageText.gameObject.SetActive(true);
            ShowMsTimer = 7.0f;
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);
        }
    }

    //----------------- 이메일형식이 맞는지 확인하는 방법 스크립트
    // <summary>
    /// 올바른 이메일인지 체크.
    /// </summary>
    private bool CheckEmailAddress(string EmailStr)
    {
        if (string.IsNullOrEmpty(EmailStr)) isValidFormat = false;

        EmailStr = Regex.Replace(EmailStr, @"(@)(.+)$", this.DomainMapper, RegexOptions.None);
        if (invalidEmailType) isValidFormat = false;

        // true 로 반환할 시, 올바른 이메일 포맷임.
        isValidFormat = Regex.IsMatch(EmailStr,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase);
        return isValidFormat;
    }

    /// <summary>
    /// 도메인으로 변경해줌.
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    private string DomainMapper(Match match)
    {
        // IdnMapping class with default property values.
        IdnMapping idn = new IdnMapping();

        string domainName = match.Groups[2].Value;
        try
        {
            domainName = idn.GetAscii(domainName);
        }
        catch (ArgumentException)
        {
            invalidEmailType = true;
        }
        return match.Groups[1].Value + domainName;
    }
    //----------------- 이메일형식이 맞는지 확인하는 방법 스크립트
}
