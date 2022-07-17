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
    [Header("Main")]
    public Button LoginPanelBtn;
    public Button CreateAccPanelBtn;
    public Button QuitBtn;

    [Header("LoginPanel")]
    public GameObject LoginPanel;
    public Button StartBtn;
    public Button LoginExitBtn;
    public InputField IDInputField;     //Email 로 받을 것임
    public InputField PassInputField;

    [Header("CreateAccPanel")]
    public GameObject CreateAccPanel;
    public Button CreateAccBtn;
    public Button CrAccExitBtn;
    public InputField New_IDInputField;  //Email 로 받을 것임
    public InputField New_PassInputField;

    [Header("Normal")]
    public Text MessageText;
    float ShowMsTimer = 0.0f;

    bool invalidEmailType = false;       // 이메일 포맷이 올바른지 체크
    bool isValidFormat = false;          // 올바른 형식인지 아닌지 체크

    // Start is called before the first frame update
    void Start()
    {
        GlobalUserData.LoadGameInfo();

        // 타이틀화면 버튼들
        if (LoginPanelBtn != null)
            LoginPanelBtn.onClick.AddListener(() =>
            {
                LoginPanel.gameObject.SetActive(true);
            });

        if (CreateAccPanelBtn != null)
            CreateAccPanelBtn.onClick.AddListener(() =>
            {
                CreateAccPanel.gameObject.SetActive(true);
            });

        // 취소버튼들
        if (LoginExitBtn != null)
            LoginExitBtn.onClick.AddListener(() =>
            {
                LoginPanel.gameObject.SetActive(false);
            });

        if (CrAccExitBtn != null)
            CrAccExitBtn.onClick.AddListener(() =>
            {
                CreateAccPanel.gameObject.SetActive(false);
            });

        if (QuitBtn != null)
            QuitBtn.onClick.AddListener(() =>
            {
                Application.Quit();
            });

        // 회원가입 적용 (나중에)
        if (StartBtn != null)
            StartBtn.onClick.AddListener(LoginBtn);

        if (CreateAccBtn != null)
            CreateAccBtn.onClick.AddListener(CreateAccountBtn);

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

        GlobalUserData.g_Unique_ID = result.PlayFabId;

        if (result.InfoResultPayload != null)
        {
            foreach (var eachStat in result.InfoResultPayload.PlayerStatistics)
            {
                if (eachStat.StatisticName == "UserCristal")
                {
                    GlobalUserData.s_CristalCount = eachStat.Value;
                }
            }

            int a_GetValue = 0;
            int Idx = 0;
            foreach (var eachData in result.InfoResultPayload.UserData)
            {
                if (eachData.Key == "UserGold")
                {
                    if (int.TryParse(eachData.Value.Value, out a_GetValue) == true)
                        GlobalUserData.s_GoldCount = a_GetValue;
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

                    if (int.TryParse(eachData.Value.Value, out a_GetValue) == false)
                        MessageOnOff("string -> int : TryParse 실패");
                }
            }
        }

        SceneManager.LoadScene("StageChoice");

    }//void OnLoginSuccess(LoginResult result)

    private void OnLoginFailure(PlayFabError error)
    {
        MessageOnOff("로그인 실패 : " + error.GenerateErrorReport());
    }

    public void OpenCreateAccBtn()
    {
        if (LoginPanel != null)
            LoginPanel.SetActive(false);

        if (CreateAccPanel != null)
            CreateAccPanel.SetActive(true);
    }

    public void CreateCancelBtn()
    {
        if (LoginPanel != null)
            LoginPanel.SetActive(true);

        if (CreateAccPanel != null)
            CreateAccPanel.SetActive(false);
    }

    public void CreateAccountBtn() //계정 생성 요청 함수
    {
        string a_IdStr = New_IDInputField.text;
        string a_PwStr = New_PassInputField.text;

        if (a_IdStr.Trim() == "" || a_PwStr.Trim() == "")
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

        if (!CheckEmailAddress(a_IdStr))
        {
            MessageOnOff("Email 형식이 맞지 않습니다.");
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            Email = New_IDInputField.text,
            Password = New_PassInputField.text,
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