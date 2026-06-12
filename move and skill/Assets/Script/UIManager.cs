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