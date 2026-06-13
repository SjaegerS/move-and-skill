using System.Collections;
using System.Collections.Generic;
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

    private bool isKnightCharging = false;
    private float knightChargeTimer = 0f;
    private bool hasDarkSwordReset = false;
    private bool isShadowlessActive = false;
    private bool isSwordEmperorActive = false;

    [Header("스킬 이펙트 (검제/검성)")]
    public GameObject swordMasterSkill1Effect;
    public GameObject swordSaintSkill2Effect_1;
    public GameObject swordSaintSkill2Effect_2;

    [Header("스킬 이펙트 (암검/무영)")]
    public GameObject darkSwordSkill1Effect;
    public GameObject shadowlessSkill2Effect;

    [Header("스킬 이펙트 (기사/검황)")]
    public GameObject knightSkill1Effect;
    public float knightEffectAngleOffset = -90f;
    public GameObject swordEmperorSkill2Effect;

    // ★ 추가됨: 마검/오버로더 이펙트 연결 칸
    [Header("스킬 이펙트 (마검/오버로더)")]
    public GameObject magicSwordProjectilePrefab; // 마검 1차 (초승달 투사체)
    public GameObject overlordSkill2Effect;       // 오버로더 2차 (찌르기 이펙트)

    [Header("스킬 타격 범위 세팅 (검제/검성)")]
    public float skill1Radius = 3.5f;
    public float skill2Hit1Distance = 2.0f;
    public float skill2Hit1Radius = 2.0f;
    public float skill2Hit2Distance = 3.0f;
    public float skill2Hit2Radius = 4.0f;

    [Header("스킬 타격 범위 세팅 (암검/무영)")]
    public Vector2 darkSwordHitBox = new Vector2(2f, 2f);
    public float shadowlessHitRadius = 3.5f;

    // ★ 추가됨: 오버로더 얇은 찌르기 판정 조절 (X: 두께, Y: 길이)
    [Header("스킬 타격 범위 세팅 (마검/오버로더)")]
    public Vector2 overlordHitBox = new Vector2(1.5f, 6f);

    [Header("피격 설정")]
    private float hitDelay = 0f;

    [Header("아이템 이펙트")]
    public GameObject magnetAura;

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
            if (currentSkill1Cooldown <= 0)
            {
                currentSkill1Cooldown = 0;
                hasDarkSwordReset = false;
            }

            // 일반 쿨다운 전직(0:검제 / 1:암검 / 3:마검)은 '쿨다운이 도는 동안에만' 갱신.
            // 가드(if > 0) 안으로 되돌려 평타/회피/스킬2와 동일한 패턴으로 통일한다.
            // 쿨다운이 0으로 끝나는 프레임에 마스크를 0으로 정리하고, 이후엔 손대지 않는다.
            if (GameManager.instance.currentJobPath != 2)
                UIManager.instance.UpdateCooldown(2, currentSkill1Cooldown, skill1Cooldown);
        }

        // 기사/검황(전직 2)만 '차징' 방식이라 게이지가 차고 빠지는 것을 매 프레임 실시간 갱신해야 한다.
        // 이 경로만 별도로 분리하여, 다른 전직이 기사용 코드를 공유하지 않도록 한다.
        if (GameManager.instance.currentJobPath == 2)
        {
            UIManager.instance.UpdateCooldown(2, isKnightCharging ? knightChargeTimer : 0f, 3f);
        }

        if (currentSkill2Cooldown > 0)
        {
            currentSkill2Cooldown -= Time.deltaTime;
            if (currentSkill2Cooldown <= 0) currentSkill2Cooldown = 0;
            UIManager.instance.UpdateCooldown(3, currentSkill2Cooldown, skill2Cooldown);
        }

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (crosshair != null) crosshair.position = new Vector3(mousePos.x, mousePos.y, 0f);

        if (isDodging || isShadowlessActive || isSwordEmperorActive)
        {
            if (isDodging)
            {
                if (ghostTimer <= 0) { CreateGhost(); ghostTimer = ghostDelay; }
                else ghostTimer -= Time.deltaTime;

                dodgeTimer -= Time.deltaTime;
                if (dodgeTimer <= 0) { isDodging = false; isInvincible = false; }
            }
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

        if (isSkill1Unlocked && !isDodging)
        {
            int jobPath = GameManager.instance.currentJobPath;

            if (jobPath == 2)
            {
                if (Input.GetMouseButton(1))
                {
                    isKnightCharging = true;
                    knightChargeTimer += Time.deltaTime;
                    if (knightChargeTimer > 3f) knightChargeTimer = 3f;
                }

                if (Input.GetMouseButtonUp(1) && isKnightCharging)
                {
                    UseKnightChargeSkill(knightChargeTimer);
                    isKnightCharging = false;
                    knightChargeTimer = 0f;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(1) && currentSkill1Cooldown <= 0) UseSkill1(jobPath);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && isSkill2Unlocked && currentSkill2Cooldown <= 0 && !isDodging)
        {
            UseSkill2(GameManager.instance.currentJobPath);
        }
    }

    void UseSkill1(int jobPath)
    {
        currentSkill1Cooldown = skill1Cooldown;

        switch (jobPath)
        {
            case 0:
                if (swordMasterSkill1Effect != null) Destroy(Instantiate(swordMasterSkill1Effect, transform.position, Quaternion.identity), 1.5f);
                Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, skill1Radius);
                foreach (var t in targets) if (t.CompareTag("Enemy")) t.SendMessage("TakeDamage", 20 + bonusDamage, SendMessageOptions.DontRequireReceiver);
                break;

            case 1: StartCoroutine(DarkSwordSkillRoutine()); break;

            case 3: // ★ 마검 1차: 투사체 발사 및 아티팩트 쿨감
                Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
                if (magicSwordProjectilePrefab != null)
                {
                    GameObject proj = Instantiate(magicSwordProjectilePrefab, transform.position, Quaternion.identity);
                    MagicSwordProjectile script = proj.GetComponent<MagicSwordProjectile>();
                    if (script != null) script.Init(dir);
                }
                ReduceRandomArtifactCooldown(2f); // 쿨타임 2초 삭감 기믹
                break;
        }
    }

    void UseSkill2(int jobPath)
    {
        currentSkill2Cooldown = skill2Cooldown;

        switch (jobPath)
        {
            case 0: StartCoroutine(SwordSaintSkillRoutine()); break;
            case 1: StartCoroutine(ShadowlessSkillRoutine()); break;
            case 2: StartCoroutine(SwordEmperorSkillRoutine()); break;
            case 3: StartCoroutine(OverlordSkillRoutine()); break; // ★ 오버로더 2차
        }
    }

    // ==========================================================
    // 마검/오버로더 전용 아티팩트 시너지 로직
    // ==========================================================

    void ReduceRandomArtifactCooldown(float timeAmount)
    {
        List<int> ownedArtifacts = new List<int>();
        for (int i = 0; i < 3; i++) { if (GameManager.instance.artifactLevels[i] > 0) ownedArtifacts.Add(i); }

        if (ownedArtifacts.Count > 0)
        {
            int randIndex = ownedArtifacts[Random.Range(0, ownedArtifacts.Count)];
            GameManager.instance.artifactCooldowns[randIndex] = Mathf.Max(0f, GameManager.instance.artifactCooldowns[randIndex] - timeAmount);
            Debug.Log($"마검 효과: {randIndex}번 아티팩트 쿨타임 {timeAmount}초 감소!");
        }
    }

    void TriggerRandomArtifact()
    {
        List<int> ownedArtifacts = new List<int>();
        for (int i = 0; i < 3; i++) { if (GameManager.instance.artifactLevels[i] > 0) ownedArtifacts.Add(i); }

        if (ownedArtifacts.Count > 0)
        {
            int randIndex = ownedArtifacts[Random.Range(0, ownedArtifacts.Count)];
            // GameManager의 private 함수인 FireArtifact를 SendMessage로 강제 호출합니다!
            GameManager.instance.SendMessage("FireArtifact", randIndex, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"오버로더 효과: {randIndex}번 아티팩트 즉시 발동!");
        }
    }

    // ==========================================================
    // 스킬 코루틴 모음
    // ==========================================================

    IEnumerator OverlordSkillRoutine()
    {
        Vector2 startPos = transform.position;
        Vector2 dir = (mousePos - startPos).normalized;

        if (overlordSkill2Effect != null)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            GameObject effect = Instantiate(overlordSkill2Effect, startPos, Quaternion.Euler(0, 0, angle));
            Destroy(effect, 0.5f);
        }

        // 얇고 긴 BoxCast 연산 (HitBox.x = 두께, HitBox.y = 거리)
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector2 center = startPos + dir * (overlordHitBox.y / 2f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(overlordHitBox.y, overlordHitBox.x), angleDeg);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy")) hit.SendMessage("TakeDamage", 35 + bonusDamage, SendMessageOptions.DontRequireReceiver);
        }

        TriggerRandomArtifact(); // 찌르기와 동시에 랜덤 아티팩트 펑!
        yield return null;
    }

    IEnumerator SwordSaintSkillRoutine()
    {
        rigid.linearVelocity = Vector2.zero;
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        Vector2 hitPoint1 = (Vector2)transform.position + dir * skill2Hit1Distance;
        if (swordSaintSkill2Effect_1 != null) Destroy(Instantiate(swordSaintSkill2Effect_1, hitPoint1, Quaternion.identity), 1.5f);
        Collider2D[] targets1 = Physics2D.OverlapCircleAll(hitPoint1, skill2Hit1Radius);
        foreach (var t in targets1) if (t.CompareTag("Enemy")) t.SendMessage("TakeDamage", 40 + (bonusDamage * 2), SendMessageOptions.DontRequireReceiver);

        yield return new WaitForSeconds(0.3f);

        Vector2 hitPoint2 = (Vector2)transform.position + dir * skill2Hit2Distance;
        if (swordSaintSkill2Effect_2 != null) Destroy(Instantiate(swordSaintSkill2Effect_2, hitPoint2, Quaternion.identity), 2f);
        Collider2D[] targets2 = Physics2D.OverlapCircleAll(hitPoint2, skill2Hit2Radius);
        foreach (var t in targets2)
        {
            if (t.CompareTag("Enemy")) { t.SendMessage("TakeDamage", 15 + bonusDamage, SendMessageOptions.DontRequireReceiver); t.SendMessage("ApplyStun", 2f, SendMessageOptions.DontRequireReceiver); }
        }
    }

    IEnumerator DarkSwordSkillRoutine()
    {
        rigid.linearVelocity = Vector2.zero;
        Vector2 startPos = transform.position;
        Vector2 targetPos = mousePos;

        float maxDashDistance = 12f;
        Vector2 dir = (targetPos - startPos).normalized;
        float dist = Vector2.Distance(startPos, targetPos);

        if (dist > maxDashDistance)
        {
            dist = maxDashDistance;
            targetPos = startPos + dir * dist;
        }

        if (darkSwordSkill1Effect != null)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            GameObject effect = Instantiate(darkSwordSkill1Effect, startPos, Quaternion.Euler(0, 0, angle));
            Destroy(effect, 0.5f);
        }

        RaycastHit2D[] hits = Physics2D.BoxCastAll(startPos, darkSwordHitBox, 0f, dir, dist);
        bool isAnyEnemyKilled = false;

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                ShortEnemy se = hit.collider.GetComponent<ShortEnemy>();
                LongEnemy le = hit.collider.GetComponent<LongEnemy>();

                int finalDamage = 30 + bonusDamage;

                if (se != null) { if (se.currentHp <= finalDamage) isAnyEnemyKilled = true; se.TakeDamage(finalDamage); }
                if (le != null) { if (le.currentHp <= finalDamage) isAnyEnemyKilled = true; le.TakeDamage(finalDamage); }
            }
        }

        transform.position = targetPos;

        if (isAnyEnemyKilled && !hasDarkSwordReset)
        {
            currentSkill1Cooldown = 0f;
            hasDarkSwordReset = true;
        }
        yield return null;
    }

    IEnumerator ShadowlessSkillRoutine()
    {
        isShadowlessActive = true;
        isInvincible = true;
        rigid.linearVelocity = Vector2.zero;

        Vector2 targetArea = mousePos;

        spriter.enabled = false;
        if (equippedWeapon != null) equippedWeapon.gameObject.SetActive(false);

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.2f);

            if (shadowlessSkill2Effect != null)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 1.5f;
                GameObject effect = Instantiate(shadowlessSkill2Effect, targetArea + randomOffset, Quaternion.identity);
                Destroy(effect, 0.5f);
            }

            Collider2D[] targets = Physics2D.OverlapCircleAll(targetArea, shadowlessHitRadius);
            foreach (var t in targets)
            {
                if (t.CompareTag("Enemy")) t.SendMessage("TakeDamage", 25 + bonusDamage, SendMessageOptions.DontRequireReceiver);
            }
        }

        yield return new WaitForSeconds(0.2f);

        spriter.enabled = true;
        if (equippedWeapon != null) equippedWeapon.gameObject.SetActive(true);
        isInvincible = false;
        isShadowlessActive = false;
    }

    void UseKnightChargeSkill(float chargeTime)
    {
        float clampedTime = Mathf.Clamp(chargeTime, 0f, 3f);
        float radius = 3f + clampedTime;
        int damage = (int)(20 + (clampedTime * 15)) + bonusDamage;

        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        if (knightSkill1Effect != null)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            GameObject effect = Instantiate(knightSkill1Effect, transform.position, Quaternion.Euler(0, 0, angle + knightEffectAngleOffset));
            effect.transform.localScale = new Vector3(radius * 0.5f, radius * 0.5f, 1f);
            Destroy(effect, 1f);
        }

        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var t in targets)
        {
            if (t.CompareTag("Enemy"))
            {
                Vector2 targetDir = (t.transform.position - transform.position).normalized;
                float targetAngle = Vector2.Angle(dir, targetDir);
                if (targetAngle <= 60f)
                {
                    t.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    IEnumerator SwordEmperorSkillRoutine()
    {
        isSwordEmperorActive = true;
        isInvincible = true;
        rigid.linearVelocity = Vector2.zero;

        Vector2 startPos = transform.position;
        Vector2 targetPos = mousePos;
        Vector2 dir = (targetPos - startPos).normalized;
        float dashDist = 3f;
        Vector2 dashDest = startPos + dir * dashDist;

        float dashTime = 0.15f;
        float timer = 0f;
        while (timer < dashTime)
        {
            timer += Time.deltaTime;
            rigid.MovePosition(Vector2.Lerp(startPos, dashDest, timer / dashTime));

            Collider2D[] pushTargets = Physics2D.OverlapCircleAll(transform.position, 2f);
            foreach (var t in pushTargets)
            {
                if (t.CompareTag("Enemy")) t.SendMessage("TakeDamage", 5 + bonusDamage, SendMessageOptions.DontRequireReceiver);
            }
            yield return null;
        }

        rigid.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.1f);

        int maxHpDamage = (int)(GameManager.instance.maxHealth * 0.3f);
        int finalDamage = maxHpDamage + bonusDamage;

        Vector2 slashPos = (Vector2)transform.position + dir * 2f;

        if (swordEmperorSkill2Effect != null)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            GameObject effect = Instantiate(swordEmperorSkill2Effect, slashPos, Quaternion.Euler(0, 0, angle));
            Destroy(effect, 1f);
        }

        bool enemyKilled = false;
        Collider2D[] slashTargets = Physics2D.OverlapCircleAll(slashPos, 3.5f);
        foreach (var t in slashTargets)
        {
            if (t.CompareTag("Enemy"))
            {
                ShortEnemy se = t.GetComponent<ShortEnemy>();
                LongEnemy le = t.GetComponent<LongEnemy>();

                if (se != null) { if (se.currentHp <= finalDamage) enemyKilled = true; se.TakeDamage(finalDamage); }
                if (le != null) { if (le.currentHp <= finalDamage) enemyKilled = true; le.TakeDamage(finalDamage); }
            }
        }

        if (enemyKilled)
        {
            GameManager.instance.health = Mathf.Min(GameManager.instance.maxHealth, GameManager.instance.health + 10);
            UIManager.instance.UpdateHp(GameManager.instance.health, GameManager.instance.maxHealth);
        }

        yield return new WaitForSeconds(0.2f);
        isInvincible = false;
        isSwordEmperorActive = false;
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive || isDodging || isShadowlessActive || isSwordEmperorActive) return;

        float currentSpeed = isKnightCharging ? speed * 0.5f : speed;
        Vector2 nextVec = inputVec.normalized * currentSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive || isDodging || isShadowlessActive || isSwordEmperorActive) return;
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

        if (isKnightCharging) damage = Mathf.Max(1, damage / 2);

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

    public void CancelAllSkills()
    {
        StopAllCoroutines();
        isDodging = false;
        isInvincible = false;
        isKnightCharging = false;
        isShadowlessActive = false;
        isSwordEmperorActive = false;
        knightChargeTimer = 0f;

        if (spriter != null)
        {
            spriter.enabled = true;
            spriter.color = Color.white;
        }

        if (equippedWeapon != null) equippedWeapon.gameObject.SetActive(true);
        if (rigid != null) rigid.linearVelocity = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 dir = Vector2.right;
        Vector2 targetPos = (Vector2)transform.position + Vector2.right * 3f;

        if (Application.isPlaying)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dir = ((Vector2)mousePosition - (Vector2)transform.position).normalized;
            targetPos = mousePosition;
        }

        if (!Application.isPlaying || (GameManager.instance != null && GameManager.instance.currentJobPath == 0))
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, skill1Radius);

            Vector2 hitPoint1 = (Vector2)transform.position + dir * skill2Hit1Distance;
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(hitPoint1, skill2Hit1Radius);

            Vector2 hitPoint2 = (Vector2)transform.position + dir * skill2Hit2Distance;
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            Gizmos.DrawSphere(hitPoint2, skill2Hit2Radius);
        }

        if (Application.isPlaying && GameManager.instance != null && GameManager.instance.currentJobPath == 1)
        {
            Gizmos.color = new Color(1, 0, 1, 0.5f);
            Gizmos.DrawLine(transform.position, targetPos);
            Gizmos.DrawWireCube(targetPos, darkSwordHitBox);

            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawSphere(targetPos, shadowlessHitRadius);
        }

        if (Application.isPlaying && GameManager.instance != null && GameManager.instance.currentJobPath == 2)
        {
            Gizmos.color = new Color(1, 0.9f, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, 6f);

            Vector2 seHitPoint = (Vector2)transform.position + dir * 2f;
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawSphere(seHitPoint, 3.5f);
        }

        // ★ [3번 트리: 마검/오버로더 기즈모]
        if (Application.isPlaying && GameManager.instance != null && GameManager.instance.currentJobPath == 3)
        {
            // 오버로더 2차 찌르기 범위 (얇고 긴 박스)
            Gizmos.color = new Color(0, 1, 1, 0.5f); // 청록색
            Matrix4x4 oldMatrix = Gizmos.matrix;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Vector3 center = transform.position + (Vector3)(dir * (overlordHitBox.y / 2f));
            // 박스를 회전시켜서 그리기 위한 매트릭스 변환!
            Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(overlordHitBox.y, overlordHitBox.x, 1));
            Gizmos.matrix = oldMatrix;
        }
    }
}