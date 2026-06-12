using System.Collections;
using UnityEngine;

public class ShortEnemy : MonoBehaviour
{
    [Header("몹 스탯")]
    public float speed;
    public int maxHp;
    public int currentHp;
    public int damage = 5;

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
    }

    public void Init(SpawnData data, int extraLevel)
    {
        maxHp = data.health + (extraLevel * 5);
        currentHp = maxHp;
        speed = data.speed + (extraLevel * 0.05f);
    }

    void FixedUpdate()
    {
        if (!isLive || target == null || isKnockback) return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;
        spriter.flipX = target.position.x < rigid.position.x;
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

        // ★ 에러 해결: transform.position 앞에 (Vector2)를 붙여서 2D 좌표로 강제 변환했습니다.
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

        // ★ 추가됨: 죽을 때 PoolManager의 2번 프리팹(보석)을 꺼내 내 위치에 떨어뜨림
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