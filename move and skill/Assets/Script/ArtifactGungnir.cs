using UnityEngine;

public class ArtifactGungnir : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 30;
    private Vector2 direction;

    public void Init(Transform target)
    {
        // 타겟 방향을 계산하고 그 방향으로 고정시킴
        direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        Destroy(gameObject, 4f); // 화면 밖으로 나가면 소멸
    }

    void Update()
    {
        // 유도 기능 없이 직선으로 뚫고 감
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // ★ 관통 무기이므로 Destroy 하지 않고 데미지만 줌
            collision.SendMessage("TakeDamage", damage + GameManager.instance.player.bonusDamage, SendMessageOptions.DontRequireReceiver);
        }
    }
}