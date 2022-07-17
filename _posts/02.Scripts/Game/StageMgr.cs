using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class StageMgr : MonoBehaviour
{
    public Button SkelDungeon;
    public Button DragonDungeon;
    public Button DevilDungeon;

    public Button BackBtn;
    public GameObject redCircle;

    public GameObject SkelPanel;
    public GameObject DrangonPanel;
    public GameObject DevilPanel;

    public Button[] QuitBtn;
    public Button[] EnterBtn;


    // Start is called before the first frame update
    void Start()
    {
        if (SkelDungeon != null)
            SkelDungeon.onClick.AddListener(SkelDunFunc);

        if (DragonDungeon != null)
            DragonDungeon.onClick.AddListener(DragonDunFunc);

        if (DevilDungeon != null)
            DevilDungeon.onClick.AddListener(DevilDunFunc);

        if (BackBtn != null)
            BackBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("TitleScene");
            });

        for (int i = 0; i < QuitBtn.Length; i++)
        {
            QuitBtn[i].onClick.AddListener(() =>
            {
                SkelPanel.SetActive(false);
                DrangonPanel.SetActive(false);
                DevilPanel.SetActive(false);
            });
        }

        for (int i = 0; i < EnterBtn.Length; i++)
        {
            EnterBtn[i].onClick.AddListener(() =>
            {
                if (EnterBtn[1])
                    SceneManager.LoadScene("SkelDungeon");
                //if (EnterBtn[2])
                //    SceneManager.LoadScene("SkelDungeon");
                //if (EnterBtn[3])
                //    SceneManager.LoadScene("SkelDungeon");
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SkelDunFunc()
    {
        SkelPanel.SetActive(true);
    }

    void DragonDunFunc()
    {
        DrangonPanel.SetActive(true);
    }

    void DevilDunFunc()
    {
        DevilPanel.SetActive(true);
    }
}
