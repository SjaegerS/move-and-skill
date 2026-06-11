using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public float lifeTime = 0.2f; // 이펙트가 화면에 남아있는 시간 (0.2초면 짧게 휙 벱니다)
    public int damage = 10;       // 나중에 몬스터에게 줄 데미지

    void Start()
    {
        // 생성되자마자 lifeTime 초 뒤에 자신을 파괴하도록 예약합니다.
        Destroy(gameObject, lifeTime);
    }

    // (참고) 나중에 여기에 OnTriggerEnter2D 함수를 추가해서 몹과 충돌 시 데미지를 입힐 예정입니다.
}