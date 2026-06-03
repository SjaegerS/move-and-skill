using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("이동 및 회피")]
    public float speed = 5f;
    public float dodgeSpeed = 15f;
    public float dodgeTime = 0.2f;
    public float dodgeCooldown = 3.0f;  // ★추가: 회피 쿨타임 (예: 3초)
    private float currentCooldown = 0f;
    public float attackCooldown = 1.5f;   // 공격 속도 (0.5초에 한 번씩 공격)
    private float currentAttackCooldown = 0f;

    public Vector2 inputVec;
    private Vector2 lastInputVec = Vector2.down;

    private float dodgeTimer;
    private bool isDodging;
    public bool isInvincible;

    [Header("회피 잔상 (Trail)")]
    public float ghostDelay = 0.03f;    // 잔상 생성 간격
    public float ghostLifetime = 0.25f; // 잔상 유지 시간
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

        // 기본 마우스 커서 숨기기
        Cursor.visible = false;
    }

    void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            // UIManager를 호출하여 시계방향 쿨타임 UI를 갱신합니다.
            UIManager.instance.UpdateCooldown(1, currentCooldown, dodgeCooldown);
        }

        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown -= Time.deltaTime;
            UIManager.instance.UpdateCooldown(0, currentAttackCooldown, attackCooldown);
        }

        // 1. 마우스 추적 및 조준선 이동
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (crosshair != null)
        {
            crosshair.position = new Vector3(mousePos.x, mousePos.y, 0f);
        }

        // 2. 회피(대시) 중일 때의 로직
        if (isDodging)
        {
            // 잔상 생성
            if (ghostTimer <= 0)
            {
                CreateGhost();
                ghostTimer = ghostDelay;
            }
            else
            {
                ghostTimer -= Time.deltaTime;
            }

            // 회피 시간 체크
            dodgeTimer -= Time.deltaTime;
            if (dodgeTimer <= 0)
            {
                isDodging = false;
                isInvincible = false;
            }
            return;
        }

        // 3. 일반 이동 입력
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        // 가만히 있을 때의 대시를 위해 마지막 방향 저장
        if (inputVec != Vector2.zero)
        {
            lastInputVec = inputVec.normalized;
        }

        if (Input.GetMouseButton(0) && currentAttackCooldown <= 0 && !isDodging)
        {
            Attack();
        }

        // 4. 회피 시작 (Shift 키)
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentCooldown <= 0)
        {
            isDodging = true;
            isInvincible = true;
            dodgeTimer = dodgeTime;
            ghostTimer = 0f;
            currentCooldown = dodgeCooldown; // 쿨타임 타이머 리셋

            rigid.linearVelocity = lastInputVec * dodgeSpeed;
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

        anim.SetFloat("Speed", inputVec.magnitude);

        // 시선(Flip)은 항상 마우스 커서를 향함
        if (mousePos.x != rigid.position.x)
        {
            spriter.flipX = mousePos.x < rigid.position.x;
        }
    }

    // 기본 반투명 흰색 잔상을 만드는 함수
    void CreateGhost()
    {
        GameObject ghost = new GameObject("Ghost");
        ghost.transform.position = transform.position;
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer ghostSprite = ghost.AddComponent<SpriteRenderer>();
        ghostSprite.sprite = spriter.sprite;
        ghostSprite.flipX = spriter.flipX;

        // 흰색(1,1,1)에 알파값 50%(0.5)를 주어 기본 반투명 효과 적용
        ghostSprite.color = new Color(1f, 1f, 1f, 0.5f);
        ghostSprite.sortingOrder = spriter.sortingOrder - 1;

        Destroy(ghost, ghostLifetime);
    }

    void Attack()
    {
        // 쿨타임 리셋
        currentAttackCooldown = attackCooldown;

        // 추후 여기에 반월 모양 검기(이펙트)를 마우스 방향으로 생성하는 코드가 들어갑니다.
        Debug.Log("평타 발동! 마우스 방향: " + mousePos);
    }
}