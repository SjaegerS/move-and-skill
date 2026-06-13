using UnityEngine;

public class RePosition : MonoBehaviour
{
    Collider2D coli;

    [Header("맵 이동 설정")]
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

        // ★ 버그 수정됨: inputVec(키보드)에 의존하지 않고, 실제 타일과 플레이어의 위치를 직접 비교!
        float dirX = playerPos.x > myPos.x ? 1 : -1;
        float dirY = playerPos.y > myPos.y ? 1 : -1;

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