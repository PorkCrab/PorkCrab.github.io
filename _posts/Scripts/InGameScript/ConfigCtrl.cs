using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfigCtrl : MonoBehaviour
{
    public Button m_OK_Btn = null;
    public Button m_Close_Btn = null;
    public Button m_Lobby_Btn = null;
    public InputField IDInputField = null;

    float ShowMsTimer = 0.0f;
    public Text m_Message = null;

    // Start is called before the first frame update
    void Start()
    {
        if (m_OK_Btn != null)
            m_OK_Btn.onClick.AddListener(OKBtnFunction);

        if (m_Close_Btn != null)
            m_Close_Btn.onClick.AddListener(CloseBtnFunction);

        if (m_Lobby_Btn != null)
            m_Lobby_Btn.onClick.AddListener(LobbyBtnFunction);

        Text a_Placeholder = null;
        if (IDInputField != null)
        {
            Transform a_PlhTr = IDInputField.transform.Find("Placeholder");
            a_Placeholder = a_PlhTr.GetComponent<Text>();
        }

        if (a_Placeholder != null)
            a_Placeholder.text = GlobalValue.g_NickName;
    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.unscaledDeltaTime;
            if (ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false);
            }
        }
    }

    void OKBtnFunction()
    {
        string a_NickStr = IDInputField.text.Trim();
        if (a_NickStr == "")
        {
            MessageOnOff("별명은 빈칸 없이 입력해 주셔야 합니다.");
            return;
        }
        if (!(2 <= a_NickStr.Length && a_NickStr.Length < 20))  //2~20
        {
            MessageOnOff("별명은 2글자 이상 20글자 이하로 작성해 주세요.");
            return;
        }

        InGameMgr a_InGameMgr = GameObject.FindObjectOfType<InGameMgr>();
        if (a_InGameMgr != null)
        {
            a_InGameMgr.m_TempStrBuff = a_NickStr;
            a_InGameMgr.PushPacket(PacketType.NickUpdate);
        }
    }

    void CloseBtnFunction()
    {
        Time.timeScale = 1.0f;
        Destroy(this.gameObject);
    }

    void LobbyBtnFunction()
    {
        SceneManager.LoadScene("LobbyScene");
        InGameMgr.m_InGState = InGameState.GameExit;
        Destroy(this.gameObject);
    }

    void MessageOnOff(string Mess = "", bool isOn = true)
    {
        if (isOn == true)
        {
            m_Message.text = Mess;
            m_Message.gameObject.SetActive(true);
            ShowMsTimer = 5.0f;
        }
        else
        {
            m_Message.text = "";
            m_Message.gameObject.SetActive(false);
        }
    }
}
