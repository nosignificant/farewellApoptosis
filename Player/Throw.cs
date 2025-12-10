using UnityEngine;

public class Throw : MonoBehaviour
{
    // 누가 던졌는지 기억하기 위한 변수
    private PlayerControl owner;
    private Vector3 startPos; // 출발 위치
    private float maxRange;   // 최대 사거리
    private bool hasHit = false; // 중복 충돌 방지

    public void Initialize(PlayerControl player, Vector3 velocity, float range)
    {
        owner = player;
        startPos = transform.position; // 출발점 기억
        maxRange = range;              // 사거리 기억

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = velocity;
    }

    void Update()
    {
        if (hasHit || owner == null) return;

        // 1. 거리 체크: 출발점으로부터 현재 위치까지의 거리가 제한을 넘었나?
        float dist = Vector3.Distance(startPos, transform.position);

        if (dist >= maxRange)
        {
            // 제한 거리 도달! (공중에서 즉시 발동)
            TeleportAndDestroy(transform.position);
        }
    }

    // 어딘가(벽, 바닥)에 부딪혔을 때
    void OnCollisionEnter(Collision collision)
    {
        if (hasHit || owner == null) return;

        // 부딪힌 지점보다 살짝 위로 이동 (파묻힘 방지)
        Vector3 hitPos = collision.contacts[0].point + collision.contacts[0].normal * 0.5f;
        TeleportAndDestroy(hitPos);
    }

    void TeleportAndDestroy(Vector3 targetPos)
    {
        hasHit = true; // 중복 실행 방지

        // 주인님 소환
        owner.TeleportTo(targetPos);

        // (선택) 여기에 '펑' 하는 파티클 효과(Instantiate)를 넣으면 멋집니다.

        // 공 삭제
        Destroy(gameObject);
    }
}