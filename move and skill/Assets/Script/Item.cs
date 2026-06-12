using UnityEngine;

public class Item : MonoBehaviour
{
    public float speed = 5f;
    private float currentSpeed;

    void OnEnable()
    {
        currentSpeed = speed;
    }

    void Update()
    {
        if (GameManager.instance.player == null || !GameManager.instance.isLive) return;

        Vector2 playerPos = GameManager.instance.player.transform.position;
        float distance = Vector2.Distance(transform.position, playerPos);

        // ★ 평소엔 1.5의 좁은 범위, 자석 아이템 먹으면 15의 초광역 범위
        float activeRange = GameManager.instance.isMagnetActive ? 10f : 1f;

        if (distance <= activeRange)
        {
            // ★ 평소엔 기본 속도, 자석 아이템 먹으면 3배 빠른 속도로 빨려감
            float activeSpeed = GameManager.instance.isMagnetActive ? currentSpeed * 3f : currentSpeed;

            transform.position = Vector2.MoveTowards(transform.position, playerPos, activeSpeed * Time.deltaTime);
            currentSpeed += 10f * Time.deltaTime; // 가속도
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.instance.GetExp();
            gameObject.SetActive(false);
        }
    }
}