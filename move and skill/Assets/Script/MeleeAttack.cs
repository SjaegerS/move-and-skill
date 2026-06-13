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
            // 기존에 있던 잡몹 타격 코드
            ShortEnemy se = collision.GetComponent<ShortEnemy>();
            LongEnemy le = collision.GetComponent<LongEnemy>();
            if (se != null) se.TakeDamage(damage);
            if (le != null) le.TakeDamage(damage);
        }
    }
}