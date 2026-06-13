using UnityEngine;

// 필드에 떨어지는 아이템(포션 3색 / 자석).
// - 자석 활성 시 플레이어 쪽으로 흡인
// - 생성 후 lifeTime(8초) 내 미습득 시 페이드되며 소멸
// - 플레이어 습득 시 종류별 효과 적용
public class DropItem : MonoBehaviour
{
    public enum ItemType { PotionRed, PotionPurple, PotionGreen, Magnet }
    public ItemType type;

    [Header("수명 / 소멸")]
    public float lifeTime = 8f;   // 8초 내 미습득 시 사라짐(기획서 명세)
    public float fadeTime = 2f;   // 마지막 2초간 깜빡이며 소멸 예고

    [Header("흡인 속도")]
    public float baseSpeed = 5f;

    SpriteRenderer spriter;
    float timer;
    float currentSpeed;
    bool collected = false;

    void Awake() { spriter = GetComponent<SpriteRenderer>(); }

    void OnEnable()
    {
        timer = 0f;
        currentSpeed = baseSpeed;
        collected = false;
        if (spriter != null)
        {
            Color c = spriter.color; c.a = 1f; spriter.color = c;
        }
    }

    void Update()
    {
        if (GameManager.instance == null || !GameManager.instance.isLive) return;

        // 1) 수명 카운트 + 페이드 소멸
        timer += Time.deltaTime;
        if (timer >= lifeTime) { Disappear(); return; }
        if (timer >= lifeTime - fadeTime && spriter != null)
        {
            Color c = spriter.color;
            c.a = Mathf.PingPong((lifeTime - timer) * 4f, 1f); // 깜빡임 효과
            spriter.color = c;
        }

        // 2) 자석 활성 시 흡인 (기획: 자석은 경험치/포션을 끌어당김)
        if (GameManager.instance.player == null) return;
        Vector2 playerPos = GameManager.instance.player.transform.position;
        float dist = Vector2.Distance(transform.position, playerPos);
        float range = GameManager.instance.isMagnetActive ? 10f : 1f;
        if (dist <= range)
        {
            float spd = GameManager.instance.isMagnetActive ? currentSpeed * 3f : currentSpeed;
            transform.position = Vector2.MoveTowards(transform.position, playerPos, spd * Time.deltaTime);
            currentSpeed += 10f * Time.deltaTime; // 가속
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected || !collision.CompareTag("Player")) return;
        collected = true;
        ApplyEffect();
        Disappear();
    }

    void ApplyEffect()
    {
        GameManager gm = GameManager.instance;
        if (gm == null) return;

        switch (type)
        {
            case ItemType.PotionRed:    // 빨강: 최대 체력의 절반 즉시 회복
                gm.HealInstant(Mathf.RoundToInt(gm.maxHealth * gm.redHealRatio));
                break;
            case ItemType.PotionPurple: // 보라: 총량은 더 많게, 7초에 나눠 회복
                gm.HealOverTime(Mathf.RoundToInt(gm.maxHealth * gm.purpleHealRatio), gm.purpleHealDuration);
                break;
            case ItemType.PotionGreen:  // 초록: 즉시 가득 채움
                gm.HealInstant(gm.maxHealth);
                break;
            case ItemType.Magnet:       // 자석: 일정 시간 광역 흡인 활성
                gm.ActivateMagnet(gm.magnetDuration);
                break;
        }
    }

    void Disappear()
    {
        // 드롭 빈도가 낮아 풀링 없이 파괴 처리
        Destroy(gameObject);
    }
}