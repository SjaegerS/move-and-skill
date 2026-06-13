using System.Collections;
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

    [Header("★ 아티팩트 세팅")]
    public int[] artifactLevels = new int[3];
    public float[] artifactMaxCooldowns = { 25f, 15f, 20f };
    public float[] artifactCooldowns = new float[3];

    // 유니티 인스펙터에서 무기 프리팹들을 넣을 공간!
    public GameObject swordPrefab;     // 검의 결계 프리팹
    public GameObject fireballPrefab;  // 불의 잔 프리팹
    public GameObject spearPrefab;     // 궁니르 프리팹

    [Header("★ 아이템(포션/자석) 프리팹")]
    public GameObject potionRedPrefab;     // 빨강: 절반 즉시
    public GameObject potionPurplePrefab;  // 보라: 7초 분할
    public GameObject potionGreenPrefab;   // 초록: 가득
    public GameObject magnetPrefab;        // 자석

    [Header("★ 아이템 드롭 설정")]
    [Range(0f, 1f)] public float itemDropChance = 0.1f; // 몹 사망 시 드롭 '시도' 확률

    [Header("★ 포션 회복 설정")]
    public float redHealRatio = 0.5f;     // 빨강: 최대 체력의 50%
    public float purpleHealRatio = 0.7f;  // 보라: 최대 체력의 70%(7초 분할)
    public float purpleHealDuration = 7f;

    [Header("★ 자석 설정")]
    public float magnetDuration = 20f;
    private Coroutine magnetCo;

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

    void Awake() { instance = this; }

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
        if (Input.GetKeyDown(KeyCode.Escape) && isLive) TogglePause();

        if (!isLive || isPaused || (levelUpPanel != null && levelUpPanel.activeSelf) || (jobChangePanel != null && jobChangePanel.activeSelf)) return;

        gameTime += Time.deltaTime;
        if (UIManager.instance != null) UIManager.instance.UpdateTime(gameTime);
        
        // ★ 아티팩트 자동 발동 엔진
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
                    UIManager.instance.UpdateArtifactCooldown(i, artifactCooldowns[i], artifactMaxCooldowns[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) GetExp();
        if (Input.GetKeyDown(KeyCode.Alpha2)) { gameTime += 10f; Debug.Log("현재 시간: " + gameTime); }
    }

    public void GetExp()
    {
        if (!isLive) return;
        exp++;
        int expIndex = Mathf.Min(level - 1, nextExp.Length - 1);
        int maxExp = nextExp[expIndex];

        if (exp >= maxExp)
        {
            level++; exp = 0;
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

        for (int i = 0; i < 4; i++) { if (statLevels[i] < maxStatLevels[i]) availableUpgrades.Add(i); }
        for (int i = 0; i < 3; i++) { if (artifactLevels[i] < 3) availableUpgrades.Add(i + 4); }

        if (availableUpgrades.Count == 0)
        {
            health = Mathf.Min(maxHealth, health + 20);
            if (UIManager.instance != null) UIManager.instance.UpdateHp(health, maxHealth);
            return;
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
            else choiceNameTexts[i].transform.parent.gameObject.SetActive(false);
        }

        if (levelUpPanel != null) levelUpPanel.SetActive(true);
        Cursor.visible = true; // ★ 레벨업 창이 뜰 때 마우스 켜기!
        Time.timeScale = 0;
    }

    void ApplyTextToButton(int buttonIndex, int id)
    {
        string name = ""; string desc = "";
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

            if (UIManager.instance != null) UIManager.instance.UpdateStat(statIndex, statLevels[statIndex]);

            // 1차 전직 확인
            if (statLevels[statIndex] == 5 && currentJobPath == -1)
            {
                currentJobPath = statIndex;
                player.isSkill1Unlocked = true;
                if (UIManager.instance != null) UIManager.instance.SetSkillLock(0, false); // S1 잠금 해제
                if (statIndex != 3) maxStatLevels[statIndex] = 8;

                if (levelUpPanel != null) levelUpPanel.SetActive(false);

                // 마검 트리(3번)를 탔는데, 이미 3레벨짜리 아티팩트가 있다면?!
                if (statIndex == 3)
                {
                    bool hasMaxArtifact = false;
                    for (int i = 0; i < 3; i++)
                    {
                        if (artifactLevels[i] >= 3) hasMaxArtifact = true;
                    }

                    // 1, 2차 전직 동시 달성 처리!
                    if (hasMaxArtifact)
                    {
                        player.isSkill2Unlocked = true;
                        if (UIManager.instance != null) UIManager.instance.SetSkillLock(1, false); // S2 잠금 해제(1·2차 동시)

                        if (jobChangeTitleText != null) jobChangeTitleText.text = "히든 승급 : 오버로더!";
                        if (jobChangeDescText != null) jobChangeDescText.text = "- 만렙 아티팩트 보유로 1, 2차 연속 전직에 성공했습니다!\n- 우클릭 스킬 1 & E 스킬 2 동시 개방!";
                        if (jobChangePanel != null) jobChangePanel.SetActive(true);
                        return;
                    }
                }

                ShowJobChangePopup(statIndex, 1);
                return;
            }
            // 2차 전직 확인 (마검 트리는 스탯으로 2차 전직을 하지 않음)
            else if (statLevels[statIndex] == 8 && currentJobPath == statIndex && statIndex != 3)
            {
                player.isSkill2Unlocked = true;
                if (UIManager.instance != null) UIManager.instance.SetSkillLock(1, false); // S2 잠금 해제

                if (levelUpPanel != null) levelUpPanel.SetActive(false);
                ShowJobChangePopup(statIndex, 2);
                return;
            }
        }
        else // 아티팩트
        {
            int artifactIndex = selectedID - 4;
            artifactLevels[artifactIndex]++;

            // 궁니르(창) 쿨타임 감소 기믹
            if (artifactIndex == 2)
            {
                artifactMaxCooldowns[2] = Mathf.Max(5f, artifactMaxCooldowns[2] - 5f);
            }

            if (UIManager.instance != null) UIManager.instance.UpdateArtifactLevel(artifactIndex, artifactLevels[artifactIndex]);

            // 오버로더 2차 전직 확인 (마검 1차 상태에서 아티팩트가 만렙을 찍었을 때)
            if (currentJobPath == 3 && artifactLevels[artifactIndex] == 3 && !player.isSkill2Unlocked)
            {
                player.isSkill2Unlocked = true;
                if (UIManager.instance != null) UIManager.instance.SetSkillLock(1, false); // S2 잠금 해제(오버로더)

                if (levelUpPanel != null) levelUpPanel.SetActive(false);
                ShowJobChangePopup(3, 2);
                return;
            }
        }

        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        Cursor.visible = false; // ★ 선택 완료 후 게임으로 돌아갈 때 마우스 숨기기!
        Time.timeScale = 1;
    }

    void ApplyStatEffect(int statIndex)
    {
        switch (statIndex)
        {
            case 0: player.bonusDamage += 3; break;
            case 1: player.speed += 0.5f; player.attackCooldown = Mathf.Max(0.1f, player.attackCooldown - 0.05f); break;
            case 2: maxHealth += 20; health += 20; if (UIManager.instance != null) UIManager.instance.UpdateHp(health, maxHealth); break;
            case 3:
                player.skill1Cooldown = Mathf.Max(1f, player.skill1Cooldown - 0.5f);
                player.skill2Cooldown = Mathf.Max(1f, player.skill2Cooldown - 0.5f);
                for (int i = 0; i < 3; i++) artifactMaxCooldowns[i] = Mathf.Max(1f, artifactMaxCooldowns[i] - 1f);
                break;
        }
    }

    // ★ 몹 사망 시 아이템 드롭 시도
    public void TryDropItem(Vector3 pos)
    {
        if (Random.value > itemDropChance) return; // 드롭 실패(대부분)

        int roll = Random.Range(0, 3); // 0:꽝 / 1:포션 / 2:자석
        if (roll == 0) return;         // 꽝

        if (roll == 1) // 포션 (색상 랜덤)
        {
            GameObject[] potions = { potionRedPrefab, potionPurplePrefab, potionGreenPrefab };
            GameObject pick = potions[Random.Range(0, potions.Length)];
            if (pick != null) Instantiate(pick, pos, Quaternion.identity);
        }
        else // 자석
        {
            if (magnetPrefab != null) Instantiate(magnetPrefab, pos, Quaternion.identity);
        }
    }

    // ★ 즉시 회복
    public void HealInstant(int amount)
    {
        health = Mathf.Min(maxHealth, health + amount);
        if (UIManager.instance != null) UIManager.instance.UpdateHp(health, maxHealth);
    }

    // ★ 지속 회복 (보라 포션)
    public void HealOverTime(int total, float duration)
    {
        StartCoroutine(HealOverTimeRoutine(total, duration));
    }

    IEnumerator HealOverTimeRoutine(int total, float duration)
    {
        int applied = 0;
        float elapsed = 0f;
        float tick = 0.5f;

        while (elapsed < duration && isLive)
        {
            yield return new WaitForSeconds(tick);
            elapsed += tick;
            int shouldHave = Mathf.RoundToInt(total * Mathf.Clamp01(elapsed / duration));
            int delta = shouldHave - applied;
            if (delta > 0) { HealInstant(delta); applied += delta; }
        }
    }

    // ====================================================================
    // ★ 업그레이드된 자석 로직 (테두리 ON/OFF 및 2초 전 깜빡임 구현)
    // ====================================================================
    public void ActivateMagnet(float duration)
    {
        if (magnetCo != null) StopCoroutine(magnetCo);
        magnetCo = StartCoroutine(MagnetRoutine(duration));
    }

    IEnumerator MagnetRoutine(float duration)
    {
        isMagnetActive = true;

        // 1. 자석을 먹는 즉시 푸른색 오라 켜기
        if (player != null && player.magnetAura != null) player.magnetAura.SetActive(true);

        float elapsed = 0f;

        // 2. 지속시간 종료 2초 전까지 대기
        while (elapsed < duration - 2f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. 남은 2초간 오라를 0.15초 간격으로 깜빡이기
        float blinkTimer = 0f;
        bool isAuraOn = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= 0.15f)
            {
                blinkTimer = 0f;
                isAuraOn = !isAuraOn; // 상태 반전
                if (player != null && player.magnetAura != null) player.magnetAura.SetActive(isAuraOn);
            }
            yield return null;
        }

        isMagnetActive = false;

        // 4. 자석 효과가 완전히 끝나면 오라를 확실히 끄기
        if (player != null && player.magnetAura != null) player.magnetAura.SetActive(false);
        magnetCo = null;
    }
    // ====================================================================

    // ★ 레이더: 주변 탐색
    public Transform GetClosestEnemy(Vector3 pos, float radius = 50f)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, radius);
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var coll in colliders)
        {
            if (coll.CompareTag("Enemy"))
            {
                float dist = Vector2.Distance(pos, coll.transform.position);
                if (dist < minDistance) { minDistance = dist; closest = coll.transform; }
            }
        }
        return closest;
    }

    void FireArtifact(int index)
    {
        Transform target = GetClosestEnemy(player.transform.position);
        if (target == null) return;

        switch (index)
        {
            case 0: // 검의 결계
                if (swordPrefab != null)
                {
                    int swordCount = 6 + ((artifactLevels[0] - 1) * 2);
                    StartCoroutine(SpawnSwordsRoutine(swordCount, target));
                }
                break;
            case 1: // 불의 잔
                if (fireballPrefab != null)
                {
                    GameObject fb = Instantiate(fireballPrefab, player.transform.position, Quaternion.identity);
                    fb.GetComponent<ArtifactFireball>().Init(target, artifactLevels[1]);
                }
                break;
            case 2: // 궁니르
                if (spearPrefab != null)
                {
                    GameObject spear = Instantiate(spearPrefab, player.transform.position, Quaternion.identity);
                    spear.GetComponent<ArtifactGungnir>().Init(target);
                }
                break;
        }
    }

    IEnumerator SpawnSwordsRoutine(int count, Transform firstTarget)
    {
        for (int i = 0; i < count; i++)
        {
            Transform currentTarget = (firstTarget != null && firstTarget.gameObject.activeSelf) ? firstTarget : GetClosestEnemy(player.transform.position);

            if (currentTarget != null)
            {
                GameObject sword = Instantiate(swordPrefab, player.transform.position, Quaternion.identity);
                sword.GetComponent<ArtifactSword>().Init(currentTarget);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void ShowJobChangePopup(int jobIndex, int tier)
    {
        string title = ""; string desc = "";
        if (tier == 1)
        {
            switch (jobIndex)
            {
                case 0: title = "1차 전직 : 검제"; desc = "- 마우스 우클릭 스킬 1 개방!\n- 다음 목표: 힘 8레벨 달성 시 2차 전직(검성)"; break;
                case 1: title = "1차 전직 : 암검"; desc = "- 마우스 우클릭 스킬 1 개방!\n- 다음 목표: 속도 8레벨 달성 시 2차 전직(무영)"; break;
                case 2: title = "1차 전직 : 기사"; desc = "- 마우스 우클릭 스킬 1 개방!\n- 다음 목표: 방어 8레벨 달성 시 2차 전직(검황)"; break;
                case 3: title = "1차 전직 : 마검"; desc = "- 마우스 우클릭 스킬 1 개방!\n- 다음 목표: 임의의 아티팩트 최대 레벨 달성 시 2차 전직(오버로더)"; break;
            }
        }
        else
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

        Cursor.visible = true; // ★ 전직 팝업이 뜰 때 마우스 켜기!
    }

    public void CloseJobChangePopup()
    {
        if (jobChangePanel != null) jobChangePanel.SetActive(false);
        Cursor.visible = false; // ★ 전직 창 닫고 게임 시작할 때 마우스 숨기기!
        Time.timeScale = 1;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        Cursor.visible = isPaused; // ★ 정지하면 마우스 켜고, 풀면 다시 숨기기!
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    public void GameOver()
    {
        isLive = false;
        Time.timeScale = 0;
        Cursor.visible = true; // ★ 게임 오버 시 마우스 확실하게 켜기!
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
        Cursor.visible = true; // ★ 타이틀 화면으로 넘어가기 전에 마우스 켜두기!
        SceneManager.LoadScene("Title");
    }
}