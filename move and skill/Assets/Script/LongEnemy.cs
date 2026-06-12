using System.Collections;
using UnityEngine;

public class LongEnemy : MonoBehaviour
{
    [Header("몹 스탯")]
    public float speed;
    public int maxHp;
    public int currentHp;

    [Header("거리 유지 및 공격 세팅")]
    public float stopDistance = 6f;
    public float retreatDistance = 5f;
    public float retreatSpeed = 1.5f;
    public float attackCooldown = 2f;
    private float currentAttackCooldown;

    public GameObject projectilePrefab;

    public Rigidbody2D target;
    bool isLive = true;
    bool isKnockback = false;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Collider2D coll;
    WaitForFixedUpdate wait;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        wait = new WaitForFixedUpdate();
    }

    void OnEnable()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        }
        isLive = true;
        isKnockback = false;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.color = Color.white;
        transform.localScale = Vector3.one;
        currentAttackCooldown = attackCooldown;
    }

    public void Init(SpawnData data, int extraLevel)
    {
        maxHp = Mathf.RoundToInt((data.health + (extraLevel * 5)) * 0.7f);
        currentHp = maxHp;
        speed = data.speed + (extraLevel * 0.05f);
    }

    void FixedUpdate()
    {
        if (!isLive || target == null || isKnockback) return;

        Vector2 dirVec = target.position - rigid.position;
        float distance = dirVec.magnitude;

        if (distance > stopDistance)
        {
            Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
        }
        else if (distance < retreatDistance)
        {
            Vector2 nextVec = -dirVec.normalized * retreatSpeed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
        }

        if (distance <= stopDistance)
        {
            currentAttackCooldown -= Time.fixedDeltaTime;
            if (currentAttackCooldown <= 0)
            {
                Attack();
                currentAttackCooldown = attackCooldown;
            }
        }
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;
        spriter.flipX = target.position.x < rigid.position.x;
    }

    void Attack()
    {
        if (projectilePrefab == null) return;

        Vector2 dirVec = (target.position - rigid.position).normalized;
        float angle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        Instantiate(projectilePrefab, rigid.position, rot);
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isLive) return;
        currentHp -= damageAmount;

        if (currentHp > 0)
        {
            spriter.color = new Color(1, 0.5f, 0.5f);
            Invoke("ResetColor", 0.1f);
            StartCoroutine(KnockBack());
        }
        else
        {
            Die();
        }
    }

    IEnumerator KnockBack()
    {
        isKnockback = true;
        yield return wait;

        // ★ 에러 해결: (Vector2) 형변환 추가
        Vector2 dirVec = (Vector2)transform.position - target.position;
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce(dirVec.normalized * 5f, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.2f);
        isKnockback = false;
    }

    void ResetColor() { spriter.color = Color.white; }

    void Die()
    {
        isLive = false;
        coll.enabled = false;
        rigid.simulated = false;
        spriter.color = Color.gray;
        transform.localScale = new Vector3(transform.localScale.x, 0.3f, 1f);

        // ★ 추가됨: 보석 드랍
        GameObject gem = GameManager.instance.pool.Get(2);
        gem.transform.position = transform.position;

        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(0.3f);
        gameObject.SetActive(false);
    }
}