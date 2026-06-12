using UnityEngine;

public class RePosition : MonoBehaviour
{
    Collider2D coli;

    [Header("맵 이동 설정")]
    // ★ 추가됨: 유니티 인스펙터 창에서 이 수치를 조절하여 맵 구멍을 없앱니다!
    public float mapSize = 40f;

    void Awake()
    {
        coli = GetComponent<Collider2D>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area"))
        {
            return;
        }

        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 myPos = transform.position;
        float diffX = Mathf.Abs(playerPos.x - myPos.x);
        float diffY = Mathf.Abs(playerPos.y - myPos.y);

        Vector3 playerDir = GameManager.instance.player.inputVec;
        float dirX = playerDir.x < 0 ? -1 : 1;
        float dirY = playerDir.y < 0 ? -1 : 1;

        switch (transform.tag)
        {
            case "Ground":
                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * mapSize);
                }
                else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * dirY * mapSize);
                }
                else
                {
                    // 대각선 이동 시 맵 구멍 뚫리는 버그 방지 (이전 수정본 반영)
                    transform.Translate(new Vector3(dirX * mapSize, dirY * mapSize, 0));
                }
                break;
            case "Enemy":
                if (coli.enabled)
                {
                    Vector3 dist = playerPos - myPos;
                    Vector3 ran = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f);
                    transform.Translate(ran + dist * 2);
                }
                break;
        }
    }
}