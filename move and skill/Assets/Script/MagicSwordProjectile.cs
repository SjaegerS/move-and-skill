using UnityEngine;

public class MagicSwordProjectile : MonoBehaviour
{
    public float speed = 15f;
    public int baseDamage = 25;
    private Vector2 direction = Vector2.right;

    public void Init(Vector2 dir)
    {
        direction = dir;
        // 날아가는 방향으로 이미지 각도 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, 3f); // 3초 뒤 화면 밖으로 나가면 자동 소멸
    }

    void Update()
    {
        // 앞으로 직진!
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            int finalDamage = baseDamage + GameManager.instance.player.bonusDamage;
            // 관통형이므로 Destroy(gameObject)를 쓰지 않고 데미지만 입히고 지나갑니다!
            collision.SendMessage("TakeDamage", finalDamage, SendMessageOptions.DontRequireReceiver);
        }
    }
}