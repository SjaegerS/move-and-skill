using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("무기 세팅")]
    public SpriteRenderer weaponSprite;
    public float swingSpeed = 1200f;
    public float swingAngle = 120f;

    private bool isSwinging = false;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = transform.root;
    }

    void Update()
    {
        if (!isSwinging)
        {
            AimAtMouse();
        }
    }

    void AimAtMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)playerTransform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // ★ 해결된 핵심 코드: WeaponSprite가 아닌 WeaponHolder(부모)의 축 자체를 뒤집습니다.
        // 이렇게 하면 칼 이미지가 어떻게 돌아가 있든 무조건 손 위치에 고정된 채로 예쁘게 대칭됩니다.
        if (mousePos.x < playerTransform.position.x)
        {
            transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void Swing()
    {
        if (!isSwinging)
        {
            StartCoroutine(SwingRoutine());
        }
    }

    IEnumerator SwingRoutine()
    {
        isSwinging = true;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)playerTransform.position).normalized;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float startOffset = (mousePos.x < playerTransform.position.x) ? -swingAngle / 2f : swingAngle / 2f;
        float endOffset = (mousePos.x < playerTransform.position.x) ? swingAngle / 2f : -swingAngle / 2f;

        float startAngle = baseAngle + startOffset;
        float endAngle = baseAngle + endOffset;

        transform.rotation = Quaternion.Euler(0, 0, startAngle);
        float currentAngle = startAngle;

        while (Mathf.Abs(Mathf.DeltaAngle(currentAngle, endAngle)) > 5f)
        {
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, endAngle, swingSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 0, baseAngle);
        isSwinging = false;
    }
}