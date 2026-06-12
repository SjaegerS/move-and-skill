using UnityEngine;

// 인스펙터에서 관리할 소환 데이터 묶음
[System.Serializable]
public class SpawnData
{
    public float spawnTime; // 소환 간격 (작을수록 빨리 나옴)
    public int meleeRatio;  // 근접 몹 비율 (0~10)
    public int health;      // 기본 체력
    public float speed;     // 기본 이동 속도
}

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    public float levelTime = 10f; // 몇 초마다 다음 레벨(단계)로 넘어갈지

    public int level;
    float timer;

    void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 1. 현재 레벨 계산 (예: 10초마다 1레벨씩 무한 상승!)
        level = Mathf.FloorToInt(GameManager.instance.gameTime / levelTime);

        // 2. 인덱스 제한 (기획해둔 데이터를 넘어가면 에러가 나지 않게 마지막 데이터를 고정으로 사용)
        int dataIndex = Mathf.Min(level, spawnData.Length - 1);

        // 3. 소환 로직
        if (timer > spawnData[dataIndex].spawnTime)
        {
            timer = 0;
            Spawn(dataIndex);
        }
    }

    void Spawn(int dataIndex)
    {
        // 비율에 맞춰 근접/원거리 결정
        int randomValue = Random.Range(0, 10);
        int enemyIndex = (randomValue < spawnData[dataIndex].meleeRatio) ? 0 : 1;

        GameObject enemy = GameManager.instance.pool.Get(enemyIndex);
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;

        // ★ 무한 게임의 핵심: 배열에 설정한 레벨이 끝나도 게임 레벨은 계속 오르므로 초과된 레벨을 계산
        int extraLevel = Mathf.Max(0, level - (spawnData.Length - 1));

        // 몹에게 스탯 주입
        if (enemyIndex == 0)
        {
            enemy.GetComponent<ShortEnemy>().Init(spawnData[dataIndex], extraLevel);
        }
        else
        {
            enemy.GetComponent<LongEnemy>().Init(spawnData[dataIndex], extraLevel);
        }
    }
}