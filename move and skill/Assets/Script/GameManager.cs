using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;

    [Header("게임 진행 정보")]
    public float gameTime;           // 현재 생존 시간
    public float maxGameTime = 600f; // 10분(600초) - 기획서 상 보스 등장 시간

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // 1. 매 프레임마다 실제 시간을 더해줍니다.
        gameTime += Time.deltaTime;

        // 2. UIManager가 씬에 있다면, 시간을 UI에 표시하도록 넘겨줍니다.
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateTime(gameTime);
        }
    }
}