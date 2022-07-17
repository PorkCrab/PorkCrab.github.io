using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Item_Type
{
    IT_bcrystal1,
    IT_hpcrystal,
    IT_mpcrystal,
}

public class ItemValue
{
    public ulong UniqueID = 0;
    public Item_Type m_Item_Type;
    public string m_ItemName = "";
    public int m_ItemLevel = 0;
    public int m_ItemStar = 0; 

    public float m_AddAttack = 0.0f;
    //public MyItemNode m_RefMyItemInfo = null;
}

public class ItemObjInfo : MonoBehaviour
{
    [HideInInspector] public ItemValue m_ItemValue = new ItemValue();

    public void InitItem(Item_Type a_Item_Type, string a_Name,
                                int a_ItemLevel, int a_ItemStar)
    {
        m_ItemValue.UniqueID = GlobalUserData.GetUnique();
        m_ItemValue.m_Item_Type = a_Item_Type;
        m_ItemValue.m_ItemName = a_Name;
        m_ItemValue.m_ItemLevel = a_ItemLevel;
        m_ItemValue.m_ItemStar = a_ItemStar;
    }
}
