using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("상단 패널 (시간, 레벨, 경험치)")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI levelText;
    public Slider expBar;

    [Header("하단 패널 (체력)")]
    public Image hpBar;

    [Header("스킬 쿨타임 UI")]
    public Image[] skillMasks;
    public TextMeshProUGUI[] skillTexts;

    [Header("스킬 잠금(X) 표시  [0]=S1, [1]=S2")]
    // 전직 전 잠긴 스킬 위에 띄울 X 아이콘 오브젝트.
    // 인스펙터에서 S1, S2 버튼 위에 올린 'X 이미지' 오브젝트를 순서대로 연결한다.
    public GameObject[] skillLockIcons;

    [Header("스탯 강화 UI (칸칸이 블록 형태)")]
    public TextMeshProUGUI[] statLevelTexts;
    public GameObject[] statBlockParents;

    [Header("아티팩트 강화 UI (칸칸이 블록 형태)")]
    public GameObject[] artifactUIGroups;
    public Image[] artifactMasks;
    public TextMeshProUGUI[] artifactLevelTexts;
    public GameObject[] artifactBlockParents;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 게임 시작 시 모든 쿨다운 마스크(0:평타 / 1:회피 / 2:스킬1 / 3:스킬2)를 0으로 정리한다.
        // 에디터(Inspector)에 fillAmount가 1로 방치되어 있어도 시작 화면에서 마스크가 보이지 않도록 통일한다.
        if (skillMasks != null)
        {
            for (int i = 0; i < skillMasks.Length; i++)
            {
                if (skillMasks[i] != null) skillMasks[i].fillAmount = 0f;
                if (skillTexts != null && i < skillTexts.Length && skillTexts[i] != null)
                    skillTexts[i].text = "";
            }
        }

        // 전직 전이므로 S1, S2 모두 잠금(X 표시) 상태로 시작한다.
        SetSkillLock(0, true);
        SetSkillLock(1, true);
    }

    // 스킬 잠금 X 표시를 켜고 끈다.  lockIndex 0 = S1, 1 = S2 / locked=true 면 X 표시(잠김)
    public void SetSkillLock(int lockIndex, bool locked)
    {
        if (skillLockIcons != null && lockIndex >= 0 && lockIndex < skillLockIcons.Length
            && skillLockIcons[lockIndex] != null)
        {
            skillLockIcons[lockIndex].SetActive(locked);
        }
    }

    public void UpdateTime(float time)
    {
        int min = Mathf.FloorToInt(time / 60);
        int sec = Mathf.FloorToInt(time % 60);
        timeText.text = string.Format("{0:00} : {1:00}", min, sec);
    }

    public void UpdateHp(float currentHp, float maxHp)
    {
        hpBar.fillAmount = currentHp / maxHp;
    }

    public void UpdateCooldown(int skillIndex, float currentCooldown, float maxCooldown)
    {
        if (currentCooldown > 0)
        {
            skillMasks[skillIndex].fillAmount = currentCooldown / maxCooldown;
            skillTexts[skillIndex].text = currentCooldown.ToString("F1");
        }
        else
        {
            skillMasks[skillIndex].fillAmount = 0;
            skillTexts[skillIndex].text = "";
        }
    }

    public void UpdateStat(int statIndex, int level)
    {
        if (statLevelTexts != null && statLevelTexts.Length > statIndex && statLevelTexts[statIndex] != null)
            statLevelTexts[statIndex].text = "Lv." + level;

        if (statBlockParents != null && statBlockParents.Length > statIndex && statBlockParents[statIndex] != null)
        {
            Transform parent = statBlockParents[statIndex].transform;
            for (int i = 0; i < parent.childCount; i++)
            {
                parent.GetChild(i).gameObject.SetActive(i < level);
            }
        }
    }

    public void UpdateArtifactLevel(int index, int level)
    {
        if (artifactUIGroups != null && artifactUIGroups.Length > index && artifactUIGroups[index] != null)
        {
            if (!artifactUIGroups[index].activeSelf) artifactUIGroups[index].SetActive(true);
        }

        if (artifactLevelTexts != null && artifactLevelTexts.Length > index && artifactLevelTexts[index] != null)
            artifactLevelTexts[index].text = "Lv." + level;

        if (artifactBlockParents != null && artifactBlockParents.Length > index && artifactBlockParents[index] != null)
        {
            Transform parent = artifactBlockParents[index].transform;
            for (int i = 0; i < parent.childCount; i++)
            {
                parent.GetChild(i).gameObject.SetActive(i < level);
            }
        }
    }

    // ★ 빼먹었던 문제의 아티팩트 쿨타임 함수 복구!!
    public void UpdateArtifactCooldown(int index, float currentCooldown, float maxCooldown)
    {
        if (artifactMasks != null && artifactMasks.Length > index && artifactMasks[index] != null)
        {
            if (currentCooldown > 0) artifactMasks[index].fillAmount = currentCooldown / maxCooldown;
            else artifactMasks[index].fillAmount = 0;
        }
    }
}