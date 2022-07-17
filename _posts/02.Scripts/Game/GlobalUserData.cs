using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalUserData : MonoBehaviour
{
    public static string g_Unique_ID = "";  //유저의 고유번호

    public static int s_GoldCount = 0;
    public static int s_CristalCount = 0;
    public static int s_HpCount = 3;
    public static int s_MpCount = 3;

    public static ulong UniqueCount = 0; //임시 Item 고유키 발급기...
    public static List<ItemValue> g_ItemList = new List<ItemValue>();

    public static void LoadGameInfo()
    {
        s_GoldCount = PlayerPrefs.GetInt("GoldCount", 0);
        s_CristalCount = PlayerPrefs.GetInt("CristalCount", 0);
        s_HpCount = PlayerPrefs.GetInt("HpCount", 3);
        s_MpCount = PlayerPrefs.GetInt("MpCount", 3);

        ReflashItemLoad();

    }

    public static void ClearGameInfo()
    {
        PlayerPrefs.DeleteAll();
        LoadGameInfo();
    }

    public static ulong GetUnique() //임시 고유키 발급기...
    {
        UniqueCount = (ulong)PlayerPrefs.GetInt("SvUnique", 0);
        UniqueCount++;
        ulong a_Index = UniqueCount;

        //<--자신의 인벤토리에 있는 아이템 번호랑 겹치는 번호보다는 큰 수로 
        //유니크ID가 발급되게 처리하는 부분
        if (0 < g_ItemList.Count)
            for (int a_bb = 0; a_bb < g_ItemList.Count; ++a_bb)
            {
                if (g_ItemList[a_bb] == null)
                    continue;

                if (a_Index < g_ItemList[a_bb].UniqueID)
                {
                    a_Index = g_ItemList[a_bb].UniqueID + 1;
                }
            }//for (int a_bb = 0; a_bb < g_ItemList.Count; ++a_bb)

        UniqueCount = a_Index;
        PlayerPrefs.SetInt("SvUnique", (int)UniqueCount);
        return a_Index;
    }

    //------------ Item Reflash
    public static void ReflashItemLoad()  //<---- g_ItemList  갱신
    {
        g_ItemList.Clear();

        ItemValue a_LdNode;
        string a_KeyBuff = "";
        int a_ItmCount = PlayerPrefs.GetInt("Item_Count", 0);
        for (int a_ii = 0; a_ii < a_ItmCount; a_ii++)
        {
            a_LdNode = new ItemValue();
            a_KeyBuff = string.Format("IT_{0}_stUniqueID", a_ii);
            string stUniqueID = PlayerPrefs.GetString(a_KeyBuff, "");
            if (stUniqueID != "")
                a_LdNode.UniqueID = ulong.Parse(stUniqueID);
            a_KeyBuff = string.Format("IT_{0}_Item_Type", a_ii);
            a_LdNode.m_Item_Type = (Item_Type)PlayerPrefs.GetInt(a_KeyBuff, 0);
            a_KeyBuff = string.Format("IT_{0}_ItemName", a_ii);
            a_LdNode.m_ItemName = PlayerPrefs.GetString(a_KeyBuff, "");
            a_KeyBuff = string.Format("IT_{0}_ItemLevel", a_ii);
            a_LdNode.m_ItemLevel = PlayerPrefs.GetInt(a_KeyBuff, 0);
            a_KeyBuff = string.Format("IT_{0}_ItemStar", a_ii);
            a_LdNode.m_ItemStar = PlayerPrefs.GetInt(a_KeyBuff, 0);

            g_ItemList.Add(a_LdNode);
        }
    }

    public static void ReflashItemSave()  //<-- 리스트 다시 저장
    {
        //---------기존에 저장되어 있었던 아이템 목록 제거
        ItemValue a_SvNode;
        string a_KeyBuff = "";
        int a_ItmCount = PlayerPrefs.GetInt("Item_Count", 0);
        for (int a_ii = 0; a_ii < a_ItmCount + 10; a_ii++)
        {
            a_KeyBuff = string.Format("IT_{0}_stUniqueID", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
            a_KeyBuff = string.Format("IT_{0}_Item_Type", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
            a_KeyBuff = string.Format("IT_{0}_ItemName", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
            a_KeyBuff = string.Format("IT_{0}_ItemLevel", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
            a_KeyBuff = string.Format("IT_{0}_ItemStar", a_ii);
            PlayerPrefs.DeleteKey(a_KeyBuff);
        }
        PlayerPrefs.DeleteKey("Item_Count");  //아이템 수 제거
        PlayerPrefs.Save(); //폰에서 마지막 저장상태를 확실히 저장하게 하기 위하여...
        //---------기존에 저장되어 있었던 아이템 목록 제거

        //---------- 새로운 리스트 저장
        PlayerPrefs.SetInt("Item_Count", g_ItemList.Count);
        for (int a_ii = 0; a_ii < g_ItemList.Count; a_ii++)
        {
            a_SvNode = g_ItemList[a_ii];
            a_KeyBuff = string.Format("IT_{0}_stUniqueID", a_ii);
            PlayerPrefs.SetString(a_KeyBuff, a_SvNode.UniqueID.ToString());
            a_KeyBuff = string.Format("IT_{0}_Item_Type", a_ii);
            PlayerPrefs.SetInt(a_KeyBuff, (int)a_SvNode.m_Item_Type);
            a_KeyBuff = string.Format("IT_{0}_ItemName", a_ii);
            PlayerPrefs.SetString(a_KeyBuff, a_SvNode.m_ItemName);
            a_KeyBuff = string.Format("IT_{0}_ItemLevel", a_ii);
            PlayerPrefs.SetInt(a_KeyBuff, a_SvNode.m_ItemLevel);
            a_KeyBuff = string.Format("IT_{0}_ItemStar", a_ii);
            PlayerPrefs.SetInt(a_KeyBuff, a_SvNode.m_ItemStar);
        }
        PlayerPrefs.Save(); //폰에서 마지막 저장상태를 확실히 저장하게 하기 위하여...
        //---------- 새로운 리스트 저장
    }
}
