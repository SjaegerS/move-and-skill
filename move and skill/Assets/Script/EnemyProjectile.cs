using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 3.5f;
    public int damage = 10;
    public float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && GameManager.instance.isLive)
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage); // 플레이어 체력 깎기
            }
            Destroy(gameObject); // 맞으면 총알 소멸
        }
    }
}