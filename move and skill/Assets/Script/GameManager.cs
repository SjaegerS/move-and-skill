using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PoolManager pool;
    public Player player;

    [Header("게임 진행 정보")]
    public float gameTime;
    public bool isLive;
    public bool isPaused;
    public bool isMagnetActive = false;

    [Header("플레이어 스탯")]
    public int health;
    public int maxHealth = 100;

    [Header("경험치 및 레벨")]
    public int level = 1;
    public int exp = 0;
    public int[] nextExp = { 10, 30, 60, 100, 150, 210, 280, 360, 450, 600 };

    [Header("기획서 기반 강화 레벨")]
    public int[] statLevels = new int[4];
    public int[] maxStatLevels = { 5, 5, 5, 5 };
    public int currentJobPath = -1;

    [Header("아티팩트 정보")]
    public int[] artifactLevels = new int[3];
    public float[] artifactMaxCooldowns = { 25f, 15f, 20f };
    public float[] artifactCooldowns = new float[3];

    [Header("UI 연결")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultTimeText;
    public GameObject pausePanel;

    [Header("레벨업 팝업 UI")]
    public GameObject levelUpPanel;
    public TextMeshProUGUI[] choiceNameTexts;
    public TextMeshProUGUI[] choiceDescTexts;
    private int[] currentChoices = new int[3];

    [Header("전직 팝업 UI")]
    public GameObject jobChangePanel;
    public TextMeshProUGUI jobChangeTitleText;
    public TextMeshProUGUI jobChangeDescText;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        health = maxHealth;
        isLive = true;
        isPaused = false;

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHp(health, maxHealth);
            UIManager.instance.levelText.text = "Lv." + level;
            UIManager.instance.expBar.value = 0f;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isLive)
        {
            TogglePause();
        }

        if (!isLive || isPaused || (levelUpPanel != null && levelUpPanel.activeSelf) || (jobChangePanel != null && jobChangePanel.activeSelf))
        {
            return;
        }

        gameTime += Time.deltaTime;

        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateTime(gameTime);
        }

        // 아티팩트 자동 발동
        for (int i = 0; i < 3; i++)
        {
            if (artifactLevels[i] > 0)
            {
                artifactCooldowns[i] -= Time.deltaTime;
                if (artifactCooldowns[i] <= 0)
                {
                    FireArtifact(i);
                    artifactCooldowns[i] = artifactMaxCooldowns[i];
                }

                if (UIManager.instance != null)
                {
                    UIManager.instance.UpdateArtifactCooldown(i, artifactCooldowns[i], artifactMaxCooldowns[i]);
                }
            }
        }

        // ★ 테스트용 치트키: 숫자 '1' 누르면 레벨업
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GetExp();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gameTime += 10f;
            Debug.Log("현재 게임 시간: " + gameTime + "초");
        }
    }

    public void GetExp()
    {
        if (!isLive) return;

        exp++;
        int expIndex = Mathf.Min(level - 1, nextExp.Length - 1);
        int maxExp = nextExp[expIndex];

        if (exp >= maxExp)
        {
            level++;
            exp = 0;
            GenerateRandomChoices();
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.levelText.text = "Lv." + level;
            UIManager.instance.expBar.value = (float)exp / maxExp;
        }
    }

    void GenerateRandomChoices()
    {
        List<int> availableUpgrades = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            if (statLevels[i] < maxStatLevels[i]) availableUpgrades.Add(i);
        }

        for (int i = 0; i < 3; i++)
        {
            if (artifactLevels[i] < 3) availableUpgrades.Add(i + 4);
        }

        int choiceCount = Mathf.Min(3, availableUpgrades.Count);

        for (int i = 0; i < availableUpgrades.Count; i++)
        {
            int temp = availableUpgrades[i];
            int randomIndex = Random.Range(i, availableUpgrades.Count);
            availableUpgrades[i] = availableUpgrades[randomIndex];
            availableUpgrades[randomIndex] = temp;
        }

        for (int i = 0; i < 3; i++)
        {
            if (i < choiceCount)
            {
                int upgradeID = availableUpgrades[i];
                currentChoices[i] = upgradeID;
                ApplyTextToButton(i, upgradeID);
                choiceNameTexts[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                choiceNameTexts[i].transform.parent.gameObject.SetActive(false);
            }
        }

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }
        Time.timeScale = 0;
    }

    void ApplyTextToButton(int buttonIndex, int id)
    {
        string name = "";
        string desc = "";

        switch (id)
        {
            case 0: name = "힘"; desc = "데미지가 증가합니다."; break;
            case 1: name = "속도"; desc = "이동/공격속도 증가"; break;
            case 2: name = "방어"; desc = "최대 HP 증가 및 회복"; break;
            case 3: name = "기술"; desc = "스킬/아티팩트 쿨타임 감소"; break;
            case 4: name = "검의 결계"; desc = "[아티팩트] 유도 검 발사"; break;
            case 5: name = "불의 잔"; desc = "[아티팩트] 화염구 투척"; break;
            case 6: name = "궁니르"; desc = "[아티팩트] 관통 창 발사"; break;
        }

        choiceNameTexts[buttonIndex].text = name;
        choiceDescTexts[buttonIndex].text = desc;
    }

    public void SelectUpgrade(int buttonIndex)
    {
        int selectedID = currentChoices[buttonIndex];

        if (selectedID < 4) // 스탯
        {
            int statIndex = selectedID;
            statLevels[statIndex]++;
            ApplyStatEffect(statIndex);

            if (UIManager.instance != null)
            {
                UIManager.instance.UpdateStat(statIndex, statLevels[statIndex]);
            }

            // 1차 전직 확인 (5레벨 달성 시)
            if (statLevels[statIndex] == 5 && currentJobPath == -1)
            {
                currentJobPath = statIndex;
                player.isSkill1Unlocked = true;
                if (statIndex != 3) maxStatLevels[statIndex] = 8;

                if (levelUpPanel != null) levelUpPanel.SetActive(false);
                ShowJobChangePopup(statIndex, 1);
                return;
            }
            // 2차 전직 (8레벨 달성 시)
            else if (statLevels[statIndex] == 8 && currentJobPath == statIndex && statIndex != 3)
            {
                player.isSkill2Unlocked = true;

                if (levelUpPanel != null) levelUpPanel.SetActive(false);
                ShowJobChangePopup(statIndex, 2);
                return;
            }
        }
        else // 아티팩트
        {
            int artifactIndex = selectedID - 4;
            artifactLevels[artifactIndex]++;

            if (UIManager.instance != null)
            {
                UIManager.instance.UpdateArtifactLevel(artifactIndex, artifactLevels[artifactIndex]);
            }

            // 오버로더 2차 전직 확인 시 팝업 띄우기
            if (currentJobPath == 3 && artifactLevels[artifactIndex] == 3 && !player.isSkill2Unlocked)
            {
                player.isSkill2Unlocked = true;

                if (levelUpPanel != null) levelUpPanel.SetActive(false);
                ShowJobChangePopup(3, 2);
                return;
            }
        }

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
        Time.timeScale = 1;
    }

    void ApplyStatEffect(int statIndex)
    {
        switch (statIndex)
        {
            case 0: // 힘
                player.bonusDamage += 3;
                break;
            case 1: // 속도
                player.speed += 0.5f;
                player.attackCooldown = Mathf.Max(0.1f, player.attackCooldown - 0.05f);
                break;
            case 2: // 방어
                maxHealth += 20;
                health += 20;
                if (UIManager.instance != null) UIManager.instance.UpdateHp(health, maxHealth);
                break;
            case 3: // 기술
                player.skill1Cooldown = Mathf.Max(1f, player.skill1Cooldown - 0.5f);
                player.skill2Cooldown = Mathf.Max(1f, player.skill2Cooldown - 0.5f);
                for (int i = 0; i < 3; i++)
                {
                    artifactMaxCooldowns[i] = Mathf.Max(1f, artifactMaxCooldowns[i] - 1f);
                }
                break;
        }
    }

    void FireArtifact(int index)
    {
        Debug.Log(index + "번 아티팩트 자동 발사됨! (레벨:" + artifactLevels[index] + ")");
    }

    void ShowJobChangePopup(int jobIndex, int tier)
    {
        string title = "";
        string desc = "";

        if (tier == 1) // 1차 전직
        {
            switch (jobIndex)
            {
                case 0: title = "1차 전직 : 검제"; desc = "- 마우스 우클릭 스킬 1 개방!\n- 다음 목표: 힘 8레벨 달성 시 2차 전직(검성)"; break;
                case 1: title = "1차 전직 : 암검"; desc = "- 마우스 우클릭 스킬 1 개방!\n- 다음 목표: 속도 8레벨 달성 시 2차 전직(무영)"; break;
                case 2: title = "1차 전직 : 기사"; desc = "- 마우스 우클릭 스킬 1 개방!\n- 다음 목표: 방어 8레벨 달성 시 2차 전직(검황)"; break;
                case 3: title = "1차 전직 : 마검"; desc = "- 마우스 우클릭 스킬 1 개방!\n- 다음 목표: 임의의 아티팩트 최대 레벨 달성 시 2차 전직(오버로더)"; break;
            }
        }
        else // 2차 전직
        {
            switch (jobIndex)
            {
                case 0: title = "2차 전직 : 검성"; desc = "- 키보드 E 스킬 2 개방!\n- 검제 트리의 최종 전직입니다."; break;
                case 1: title = "2차 전직 : 무영"; desc = "- 키보드 E 스킬 2 개방!\n- 암검 트리의 최종 전직입니다."; break;
                case 2: title = "2차 전직 : 검황"; desc = "- 키보드 E 스킬 2 개방!\n- 기사 트리의 최종 전직입니다."; break;
                case 3: title = "2차 전직 : 오버로더"; desc = "- 키보드 E 스킬 2 개방!\n- 마검 트리의 최종 전직입니다."; break;
            }
        }

        if (jobChangeTitleText != null) jobChangeTitleText.text = title;
        if (jobChangeDescText != null) jobChangeDescText.text = desc;

        if (jobChangePanel != null) jobChangePanel.SetActive(true);
    }

    public void CloseJobChangePopup()
    {
        if (jobChangePanel != null) jobChangePanel.SetActive(false);
        Time.timeScale = 1;
    }

    // ★ 날아갔던 게임오버 & 버튼 함수들 완벽 복구
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    public void GameOver()
    {
        isLive = false;
        Time.timeScale = 0;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (resultTimeText != null)
        {
            int min = Mathf.FloorToInt(gameTime / 60);
            int sec = Mathf.FloorToInt(gameTime % 60);
            resultTimeText.text = string.Format("버틴 시간 \n {0:D2}:{1:D2}", min, sec);
        }
    }

    public void Retry()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToTitle()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Title");
    }
}