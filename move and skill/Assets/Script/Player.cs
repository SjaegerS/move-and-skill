using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("이동 및 회피")]
    public float speed = 5f;
    public float dodgeSpeed = 15f;
    public float dodgeTime = 0.2f;
    public float dodgeCooldown = 3.0f;
    private float currentCooldown = 0f;

    public Vector2 inputVec;
    private Vector2 lastInputVec = Vector2.down;

    private float dodgeTimer;
    private bool isDodging;
    public bool isInvincible;

    [Header("전투 및 전직 시스템")]
    public float attackCooldown = 0.5f;
    private float currentAttackCooldown = 0f;

    public GameObject basicAttackPrefab;
    public float attackOffset = 1.2f;
    public float effectAngleOffset = -90f;

    public Weapon equippedWeapon;

    [Header("회피 잔상 (Trail)")]
    public float ghostDelay = 0.03f;
    public float ghostLifetime = 0.25f;
    private float ghostTimer;

    [Header("조준선 (Crosshair)")]
    public Transform crosshair;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    Vector2 mousePos;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        Cursor.visible = false;
    }

    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            UIManager.instance.UpdateCooldown(1, currentCooldown, dodgeCooldown);
        }

        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown -= Time.deltaTime;
            UIManager.instance.UpdateCooldown(0, currentAttackCooldown, attackCooldown);
        }

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (crosshair != null)
        {
            crosshair.position = new Vector3(mousePos.x, mousePos.y, 0f);
        }

        if (isDodging)
        {
            if (ghostTimer <= 0)
            {
                CreateGhost();
                ghostTimer = ghostDelay;
            }
            else
            {
                ghostTimer -= Time.deltaTime;
            }

            dodgeTimer -= Time.deltaTime;
            if (dodgeTimer <= 0)
            {
                isDodging = false;
                isInvincible = false;
            }
            return;
        }

        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        if (inputVec != Vector2.zero)
        {
            lastInputVec = inputVec.normalized;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && currentCooldown <= 0)
        {
            isDodging = true;
            isInvincible = true;
            dodgeTimer = dodgeTime;
            ghostTimer = 0f;
            currentCooldown = dodgeCooldown;

            rigid.linearVelocity = lastInputVec * dodgeSpeed;
        }

        if (Input.GetMouseButton(0) && currentAttackCooldown <= 0 && !isDodging)
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        if (isDodging) return;

        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        if (isDodging) return;

        if (mousePos.x != rigid.position.x)
        {
            spriter.flipX = mousePos.x < rigid.position.x;
        }
    }

    void CreateGhost()
    {
        GameObject ghost = new GameObject("Ghost");
        ghost.transform.position = transform.position;
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer ghostSprite = ghost.AddComponent<SpriteRenderer>();
        ghostSprite.sprite = spriter.sprite;
        ghostSprite.flipX = spriter.flipX;

        ghostSprite.color = new Color(1f, 1f, 1f, 0.5f);
        ghostSprite.sortingOrder = spriter.sortingOrder - 1;

        Destroy(ghost, ghostLifetime);
    }

    void Attack()
    {
        currentAttackCooldown = attackCooldown;

        if (equippedWeapon != null)
        {
            equippedWeapon.Swing();
        }

        if (basicAttackPrefab == null) return;

        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion rotation = Quaternion.Euler(0, 0, angle + effectAngleOffset);
        Vector2 spawnPosition = (Vector2)transform.position + direction * attackOffset;

        GameObject slash = Instantiate(basicAttackPrefab, spawnPosition, rotation);

        // ★ 해결됨: Transform Scale(-1)로 인한 이펙트 증발 버그를 막기 위해
        // 렌더러 자체의 Flip 기능을 사용하여 거울처럼 완벽하게 뒤집어줍니다.
        if (mousePos.x < transform.position.x)
        {
            ParticleSystemRenderer[] renderers = slash.GetComponentsInChildren<ParticleSystemRenderer>();
            if (renderers.Length > 0)
            {
                foreach (ParticleSystemRenderer psr in renderers)
                {
                    // 파티클의 Y축(세로) 이미지를 100%(1) 뒤집습니다.
                    psr.flip = new Vector3(psr.flip.x, 1, psr.flip.z);

                    // 만약 칼등으로 때리는 것처럼 앞뒤가 엇나간다면 위 코드를 지우고 
                    // 아래 코드로 X축을 뒤집게 바꿔주시면 됩니다!
                    // psr.flip = new Vector3(1, psr.flip.y, psr.flip.z); 
                }
            }
            else
            {
                // 혹시 나중에 일반 스프라이트 이펙트를 쓸 경우를 대비한 안전장치
                SpriteRenderer[] spriters = slash.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sr in spriters)
                {
                    sr.flipY = true;
                }
            }
        }
    }
}