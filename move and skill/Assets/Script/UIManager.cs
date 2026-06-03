using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 제어하기 위해 반드시 필요합니다.

public class UIManager : MonoBehaviour
{
    public static UIManager instance; // 어디서든 쉽게 접근할 수 있도록 싱글톤 설정

    [Header("상단 패널 (시간, 레벨, 경험치)")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI levelText;
    public Slider expBar;

    [Header("하단 패널 (체력)")]
    public Slider hpBar;

    // 기획서 기준: 0=평타, 1=회피, 2=스킬1, 3=스킬2
    [Header("스킬 쿨타임 UI")]
    public Image[] skillMasks;          // 어두운 반투명 마스크 (Radial 360)
    public TextMeshProUGUI[] skillTexts;// 남은 초 표시 텍스트

    // 기획서 기준: 0=힘, 1=속도, 2=방어, 3=기술
    [Header("스탯 강화 UI")]
    public TextMeshProUGUI[] statLevelTexts; // Lv 표시 텍스트
    public Slider[] statBars;                // 작대기(Slider)

    void Awake()
    {
        instance = this;
    }

    // 1. 시간 업데이트 (00:00 포맷)
    public void UpdateTime(float time)
    {
        int min = Mathf.FloorToInt(time / 60);
        int sec = Mathf.FloorToInt(time % 60);
        timeText.text = string.Format("{0:00} : {1:00}", min, sec);
    }

    // 2. 체력 바 업데이트
    public void UpdateHp(float currentHp, float maxHp)
    {
        hpBar.value = currentHp / maxHp;
    }

    // 3. 쿨타임 시계 모양 애니메이션 및 텍스트 업데이트
    public void UpdateCooldown(int skillIndex, float currentCooldown, float maxCooldown)
    {
        if (currentCooldown > 0)
        {
            // 쿨타임이 돌고 있을 때: 마스크 비율 조절 및 텍스트 표시
            skillMasks[skillIndex].fillAmount = currentCooldown / maxCooldown;
            skillTexts[skillIndex].text = currentCooldown.ToString("F1"); // 소수점 첫째 자리까지 표시
        }
        else
        {
            // 쿨타임이 끝났을 때: 마스크 지우고 텍스트 비우기
            skillMasks[skillIndex].fillAmount = 0;
            skillTexts[skillIndex].text = "";
        }
    }

    // 4. 스탯 막대 및 레벨 업데이트 (5단계/8단계 상한 기준)
    public void UpdateStat(int statIndex, int level, int maxLevel)
    {
        statLevelTexts[statIndex].text = "Lv." + level;
        statBars[statIndex].value = (float)level / maxLevel;
    }
}