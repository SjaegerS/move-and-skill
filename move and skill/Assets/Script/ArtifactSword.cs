using UnityEngine;

public class ArtifactSword : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 15;
    private Transform target;

    public void Init(Transform enemyTarget)
    {
        target = enemyTarget;
        Destroy(gameObject, 5f); // 5초 뒤 자동 소멸
    }

    void Update()
    {
        if (target != null && target.gameObject.activeSelf)
        {
            // 타겟을 향해 이동하며 칼날 방향 회전
            Vector2 dir = (target.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // 에셋 방향에 따라 -90f 수정 필요
        }
        else
        {
            // 타겟이 죽으면 가장 가까운 새로운 적 탐색
            target = GameManager.instance.GetClosestEnemy(transform.position, 15f);
            if (target == null) transform.Translate(Vector2.up * speed * Time.deltaTime); // 없으면 직진
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.SendMessage("TakeDamage", damage + GameManager.instance.player.bonusDamage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject); // 타격 후 소멸
        }
    }
}