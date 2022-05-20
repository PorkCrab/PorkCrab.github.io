using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // 텍스트파일읽기

public class EnemyGenerator : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    public float nextSpawnDelay;
    public float curSpawnDelay;

    public GameObject player;

    public float playtime = 0.0f;

    public List<Spawn> spawnList;
    public int spawnIndex;
    public bool spawnEnd;

    void Awake()
    {
        spawnList = new List<Spawn>();
        ReadSpawnFile();
    }

    void ReadSpawnFile()
    {
        // 변수초기화
        spawnList.Clear();
        spawnIndex = 0;
        spawnEnd = false;

        // 텍스트파일읽기
        TextAsset textFile = Resources.Load("stage0") as TextAsset;
        StringReader stringReader = new StringReader(textFile.text);

        while (stringReader != null)
        {
            string line = stringReader.ReadLine();

            if (line == null)
                break;

            // 리스폰 데이터 읽기
            Spawn spawnData = new Spawn();
            spawnData.delay = float.Parse(line.Split(',')[0]);
            spawnData.type = line.Split(',')[1];
            spawnData.point = int.Parse(line.Split(',')[2]);
            spawnList.Add(spawnData);
        }

        // 텍스트파일 닫기
        stringReader.Close();

        nextSpawnDelay = spawnList[0].delay;
    }

    void Update()
    {
        curSpawnDelay += Time.deltaTime;

        if (InGameMgr.m_InGState == InGameState.GameIng)
        {
            if (curSpawnDelay > nextSpawnDelay && !spawnEnd)
            {
                SpawnEnemy();
                curSpawnDelay = 0;
            }
        }
    }


    void SpawnEnemy()
    {
        int enemyIndex = 0;
        switch(spawnList[spawnIndex].type)
        {
            case "S":
                enemyIndex = 0;
                break;
            case "M":
                enemyIndex = 1;
                break;
            case "L":
                enemyIndex = 2;
                break;
            case "B":
                enemyIndex = 3;
                break;
        }

        int enemyPoint = spawnList[spawnIndex].point;
        GameObject enemy = Instantiate(enemyPrefabs[enemyIndex],
                                       spawnPoints[enemyPoint].position,
                                       spawnPoints[enemyPoint].rotation);

        Rigidbody2D rigid = enemy.GetComponent<Rigidbody2D>();
        EnemyCtrl enemyLogic = enemy.GetComponent<EnemyCtrl>();
        enemyLogic.player = player;

        if (enemyPoint == 7 || enemyPoint == 8) // 오른쪽스폰
        {
            rigid.velocity = new Vector2(enemyLogic.m_Speed * (-1), 1);
            enemy.transform.Rotate(Vector3.back * 90);
        }
        else if (enemyPoint == 5 || enemyPoint ==  6) // 왼쪽스폰
        {
            enemy.transform.Rotate(Vector3.forward * 90);
            rigid.velocity = new Vector2(enemyLogic.m_Speed, -1);
        }
        else
        {
            rigid.velocity = new Vector2(0, enemyLogic.m_Speed * (-1));
        }

        // 리스폰 증가
        spawnIndex++;
        if (spawnIndex == spawnList.Count)
        {
            spawnEnd = true;
            return;
        }

        // 다음리스폰 딜레이
        nextSpawnDelay = spawnList[spawnIndex].delay;
    }


}
