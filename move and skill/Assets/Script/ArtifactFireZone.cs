using UnityEngine;

public class ArtifactFireZone : MonoBehaviour
{
    public int tickDamage = 5;
    public float damageInterval = 0.5f; // 0.5초마다 데미지
    private float timer;

    void Start()
    {
        Destroy(gameObject, 6f); // 4초간 불탐
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            timer += Time.deltaTime;
            if (timer >= damageInterval)
            {
                collision.SendMessage("TakeDamage", tickDamage + (GameManager.instance.player.bonusDamage / 2), SendMessageOptions.DontRequireReceiver);
                timer = 0f;
            }
        }
    }
}