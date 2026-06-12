using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public float lifeTime = 0.2f;
    public int damage = 10;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // ★ 기획서 맞춤: 기본 데미지에 플레이어의 보너스 데미지를 합산해서 타격합니다!
            int totalDamage = damage + GameManager.instance.player.bonusDamage;

            ShortEnemy shortEnemy = collision.GetComponent<ShortEnemy>();
            if (shortEnemy != null) shortEnemy.TakeDamage(totalDamage);

            LongEnemy longEnemy = collision.GetComponent<LongEnemy>();
            if (longEnemy != null) longEnemy.TakeDamage(totalDamage);
        }
    }
}