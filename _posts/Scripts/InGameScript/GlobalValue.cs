using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharType
{
    Char_0 = 0,
    Char_1,
    Char_2,
    Char_3,
    Char_4,
    CrCount
}

public class CharInfo
{
    public string m_Name = "";              //캐릭터 이름
    public CharType m_CrType = CharType.Char_0; //캐릭터 타입
    public Vector2 m_IconSize = Vector2.one;  //아이콘의 가로 사이즈, 세로 사이즈
    public int m_Price = 500;   //아이템 기본 가격 
    public int m_UpPrice = 250; //업그레이드 가격, 타입에 따라서
    public int m_Level = 0;
    //그전엔 Lock, 레벨0 이면 아직 구매 안됨 (구매가 완료되면 레벨 1부터)
    public int m_CurSkillCount = 1;   //사용할 수 있는 스킬 카운트
    //public int m_MaxUsable = 1;     //사용할 수 있는 최대 스킬 카운트는 Level과 같다.
    public string m_SkillExp = "";    //스킬 효과 설명
    public Sprite m_IconImg = null;   //캐릭터 아이템에 사용될 이미지

    public void SetType(CharType a_CrType)
    {
        m_CrType = a_CrType;
        if (a_CrType == CharType.Char_0)
        {
            m_Name = "스킬1";
            m_IconSize.x = 0.766f;   //세로에 대한 가로 비율
            m_IconSize.y = 1.0f;     //세로를 기준으로 잡을 것이기 때문에 그냥 1.0f = 103 픽셀

            m_Price = 500; //기본가격
            m_UpPrice = 250; //Lv1->Lv2  (m_UpPrice + (m_UpPrice * (m_Level - 1)) 가격 필요

            m_SkillExp = "Hp 50% 회복";
            m_IconImg = Resources.Load("Skill/item_life", typeof(Sprite)) as Sprite;
        }
        else if (a_CrType == CharType.Char_1)
        {
            m_Name = "스킬2";
            m_IconSize.x = 0.81f;    //세로에 대한 가로 비율
            m_IconSize.y = 1.0f;     //세로를 기준으로 잡을 것이기 때문에 그냥 1.0f

            m_Price = 1000; //기본가격
            m_UpPrice = 500; //Lv1->Lv2  (m_UpPrice + (m_UpPrice * (m_Level - 1)) 가격 필요

            m_SkillExp = "Hp 100% 회복";
            m_IconImg = Resources.Load("Skill/item_life", typeof(Sprite)) as Sprite;
        }
        else if (a_CrType == CharType.Char_2)
        {
            m_Name = "스킬3";
            m_IconSize.x = 0.946f;     //세로에 대한 가로 비율
            m_IconSize.y = 1.0f;     //세로를 기준으로 잡을 것이기 때문에 그냥 1.0f

            m_Price = 2000; //기본가격
            m_UpPrice = 1000; //Lv1->Lv2  (m_UpPrice + (m_UpPrice * (m_Level - 1)) 가격 필요

            m_SkillExp = "보호막";
            m_IconImg = Resources.Load("Skill/Pink_Circle", typeof(Sprite)) as Sprite;
        }
        else if (a_CrType == CharType.Char_3)
        {
            m_Name = "스킬5";
            m_IconSize.x = 0.93f;     //세로에 대한 가로 비율
            m_IconSize.y = 1.0f;     //세로를 기준으로 잡을 것이기 때문에 그냥 1.0f

            m_Price = 8000; //기본가격
            m_UpPrice = 4000; //Lv1->Lv2  (m_UpPrice + (m_UpPrice * (m_Level - 1)) 가격 필요

            m_SkillExp = "레이저빔";
            m_IconImg = Resources.Load("Skill/item_bomb", typeof(Sprite)) as Sprite;
        }
        else if (a_CrType == CharType.Char_4)
        {
            m_Name = "스킬6";
            m_IconSize.x = 0.93f;     //세로에 대한 가로 비율
            m_IconSize.y = 1.0f;     //세로를 기준으로 잡을 것이기 때문에 그냥 1.0f

            m_Price = 15000; //기본가격
            m_UpPrice = 8000; //Lv1->Lv2  (m_UpPrice + (m_UpPrice * (m_Level - 1)) 가격 필요

            m_SkillExp = "소환수 공격";
            m_IconImg = Resources.Load("Skill/App Icon", typeof(Sprite)) as Sprite;
        }
    }

}

public class GlobalValue
{
    public static string g_Unique_ID = "";  //유저의 고유번호

    public static string g_NickName = "";   //유저의 별명
    public static int g_BestScore = 0;      //게임점수
    public static int g_UserGold = 0;       //게임머니
    public static int g_Level = 0;
    public static int g_Exp = 0;

    public static List<CharInfo> m_CrDataList = new List<CharInfo>();

    public static void InitData()
    {
        if (0 < m_CrDataList.Count)
            return;

        CharInfo a_CrItemNd;
        for (int ii = 0; ii < (int)CharType.CrCount; ii++)
        {
            a_CrItemNd = new CharInfo();
            a_CrItemNd.SetType((CharType)ii);
            m_CrDataList.Add(a_CrItemNd);
        }
    }
}
