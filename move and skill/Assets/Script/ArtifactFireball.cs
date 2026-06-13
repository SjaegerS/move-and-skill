using UnityEngine;

public class ArtifactFireball : MonoBehaviour
{
    public float speed = 7f;
    public int baseDamage = 10;
    public GameObject fireZonePrefab; // 폭발 장판 프리팹
    private Transform target;
    private int currentLevel;

    public void Init(Transform enemyTarget, int level)
    {
        target = enemyTarget;
        currentLevel = level;
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        if (target != null && target.gameObject.activeSelf)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
        else
        {
            // 목표물이 사라지면 제자리에서 즉시 폭발
            Explode();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.SendMessage("TakeDamage", baseDamage + GameManager.instance.player.bonusDamage, SendMessageOptions.DontRequireReceiver);
            Explode();
        }
    }

    void Explode()
    {
        if (fireZonePrefab != null)
        {
            GameObject zone = Instantiate(fireZonePrefab, transform.position, Quaternion.identity);

            // ★ 기획서 맞춤: 레벨업 시 장판(폭발) 크기 증가
            float scale = 1f + (currentLevel * 0.5f);
            zone.transform.localScale = new Vector3(scale, scale, 1f);
        }
        Destroy(gameObject);
    }
}