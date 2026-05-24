using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    private Vector2 lastInputVec = Vector2.down;

    public float speed;


    public float dodgeSpeed = 15f;
    public float dodgeTime = 0.2f;
    private float dodgeTimer;
    private bool isDodging;
    public bool isInvincible;


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
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (crosshair != null)
        {
            crosshair.position = new Vector3(mousePos.x, mousePos.y, 0f);
        }

        if (isDodging)
        {
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

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isDodging = true;
            isInvincible = true;
            dodgeTimer = dodgeTime;
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
        anim.SetFloat("speed", inputVec.magnitude);
        if (mousePos.x != rigid.position.x)
        {
            spriter.flipX = mousePos.x < rigid.position.x;
        }
    }
}