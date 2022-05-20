using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCoolCtrl : MonoBehaviour
{
    [HideInInspector] public CharType m_CrType;
    float skill_Time = 0.0f;
    float skill_Delay = 0.0f;
    public Image time_Image = null;
    public Image icon_Image = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        skill_Time -= Time.deltaTime;
        time_Image.fillAmount = skill_Time / skill_Delay;

        if (skill_Time <= 0.0f)
            Destroy(this.gameObject);
    }

    public void InitState(CharInfo a_CrInfo, float a_Time, float a_Delay)
    {
        m_CrType = a_CrInfo.m_CrType;
        icon_Image.sprite = a_CrInfo.m_IconImg;
        icon_Image.GetComponent<RectTransform>().sizeDelta =
            new Vector2(a_CrInfo.m_IconSize.x * 30.0f, 30.0f);

        skill_Time = a_Time;
        skill_Delay = a_Delay;
    }

}
