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

    // 추가됨: 힘 스탯을 찍을 때마다 올라갈 보너스 공격력
    public int bonusDamage = 0;

    public GameObject basicAttackPrefab;
    public float attackOffset = 1.2f;
    public float effectAngleOffset = -90f;
    public Weapon equippedWeapon;

    [Header("스킬 시스템 (잠금 및 쿨타임)")]
    public bool isSkill1Unlocked = false;
    public bool isSkill2Unlocked = false;
    public float skill1Cooldown = 8f;
    private float currentSkill1Cooldown = 0f;
    public float skill2Cooldown = 15f;
    private float currentSkill2Cooldown = 0f;

    [Header("피격 설정")]
    private float hitDelay = 0f;

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
        if (!GameManager.instance.isLive) return;

        if (hitDelay > 0) hitDelay -= Time.deltaTime;

        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0) currentCooldown = 0;
            UIManager.instance.UpdateCooldown(1, currentCooldown, dodgeCooldown);
        }

        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown -= Time.deltaTime;
            if (currentAttackCooldown <= 0) currentAttackCooldown = 0;
            UIManager.instance.UpdateCooldown(0, currentAttackCooldown, attackCooldown);
        }

        if (currentSkill1Cooldown > 0)
        {
            currentSkill1Cooldown -= Time.deltaTime;
            if (currentSkill1Cooldown <= 0) currentSkill1Cooldown = 0;
            UIManager.instance.UpdateCooldown(2, currentSkill1Cooldown, skill1Cooldown);
        }

        if (currentSkill2Cooldown > 0)
        {
            currentSkill2Cooldown -= Time.deltaTime;
            if (currentSkill2Cooldown <= 0) currentSkill2Cooldown = 0;
            UIManager.instance.UpdateCooldown(3, currentSkill2Cooldown, skill2Cooldown);
        }

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (crosshair != null) crosshair.position = new Vector3(mousePos.x, mousePos.y, 0f);

        if (isDodging)
        {
            if (ghostTimer <= 0) { CreateGhost(); ghostTimer = ghostDelay; }
            else ghostTimer -= Time.deltaTime;

            dodgeTimer -= Time.deltaTime;
            if (dodgeTimer <= 0) { isDodging = false; isInvincible = false; }
            return;
        }

        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        if (inputVec != Vector2.zero) lastInputVec = inputVec.normalized;

        if (Input.GetKeyDown(KeyCode.LeftShift) && currentCooldown <= 0)
        {
            isDodging = true; isInvincible = true; dodgeTimer = dodgeTime; ghostTimer = 0f; currentCooldown = dodgeCooldown;
            rigid.linearVelocity = lastInputVec * dodgeSpeed;
        }

        if (Input.GetMouseButton(0) && currentAttackCooldown <= 0 && !isDodging) Attack();

        if (Input.GetMouseButtonDown(1) && isSkill1Unlocked && currentSkill1Cooldown <= 0 && !isDodging)
        {
            currentSkill1Cooldown = skill1Cooldown;
            Debug.Log("스킬 1 발동!");
        }

        if (Input.GetKeyDown(KeyCode.E) && isSkill2Unlocked && currentSkill2Cooldown <= 0 && !isDodging)
        {
            currentSkill2Cooldown = skill2Cooldown;
            Debug.Log("스킬 2 발동!");
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive || isDodging) return;
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive || isDodging) return;
        if (mousePos.x != rigid.position.x) spriter.flipX = mousePos.x < rigid.position.x;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive || isDodging || isInvincible) return;

        if (collision.gameObject.CompareTag("Enemy") && hitDelay <= 0)
        {
            int enemyDamage = 5;
            ShortEnemy enemy = collision.gameObject.GetComponent<ShortEnemy>();
            if (enemy != null) enemyDamage = enemy.damage;
            TakeDamage(enemyDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!GameManager.instance.isLive || isDodging || hitDelay > 0) return;

        GameManager.instance.health -= damage;

        if (GameManager.instance.health <= 0)
        {
            GameManager.instance.health = 0;
            UIManager.instance.UpdateHp(0, GameManager.instance.maxHealth);
            GameManager.instance.GameOver();
            return;
        }

        UIManager.instance.UpdateHp(GameManager.instance.health, GameManager.instance.maxHealth);
        spriter.color = new Color(1, 0.5f, 0.5f);
        Invoke("ResetColor", 0.1f);
        hitDelay = 0.5f;
    }

    void ResetColor() { spriter.color = Color.white; }

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
        if (equippedWeapon != null) equippedWeapon.Swing();
        if (basicAttackPrefab == null) return;

        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle + effectAngleOffset);
        Vector2 spawnPosition = (Vector2)transform.position + direction * attackOffset;

        GameObject slash = Instantiate(basicAttackPrefab, spawnPosition, rotation);

        if (mousePos.x < transform.position.x)
        {
            ParticleSystemRenderer[] renderers = slash.GetComponentsInChildren<ParticleSystemRenderer>();
            if (renderers.Length > 0) foreach (ParticleSystemRenderer psr in renderers) psr.flip = new Vector3(psr.flip.x, 1, psr.flip.z);
            else { SpriteRenderer[] spriters = slash.GetComponentsInChildren<SpriteRenderer>(); foreach (SpriteRenderer sr in spriters) sr.flipY = true; }
        }
    }
}