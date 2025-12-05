using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public float openHeight = 3f;     // 문이 위로 열리는 거리
    public float moveDuration = 1.0f;   // 문이 열리거나 닫히는 데 걸리는 시간 (초)

    [Header("Current State")]
    public bool isDoorOpen = false;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine movingRoutine; // 중복 동작 방지

    void Start()
    {
        // 1. 문이 닫혀있는 초기 위치 저장
        closedPosition = transform.position;

        // 2. 문이 완전히 열리는 목표 위치 계산 (현재 위치에서 Y축으로만 올림)
        openPosition = closedPosition + new Vector3(0, openHeight * 10, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) OpenTheDoor(true);
        if (Input.GetKeyDown(KeyCode.C)) OpenTheDoor(false);
    }

    // 외부에서 문 열림/닫힘을 요청할 때 사용하는 공용 함수
    public void OpenTheDoor(bool open)
    {
        // 이미 움직이고 있거나, 원하는 상태와 같다면 무시
        if (open == isDoorOpen || movingRoutine != null) return;

        isDoorOpen = open;
        Vector3 target = open ? openPosition : closedPosition; // 목표 위치 설정

        // 기존 코루틴이 있다면 멈추고 새로운 동작 시작
        if (movingRoutine != null) StopCoroutine(movingRoutine);
        movingRoutine = StartCoroutine(MoveDoor(target));
    }

    // (선택 사항) 토글 기능을 위한 함수
    public void ToggleDoor()
    {
        OpenTheDoor(!isDoorOpen);
    }

    // ----------------------------------------------------
    // 💡 문을 부드럽게 움직이는 코루틴
    // ----------------------------------------------------
    IEnumerator MoveDoor(Vector3 targetPos)
    {
        float timer = 0f;
        Vector3 startPos = transform.position;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / moveDuration;

            // 💡 Lerp 값에 이징(Easing)을 적용하여 움직임을 부드럽게 만듦 (선택 사항)
            t = t * t * (3f - 2f * t); // SmoothStep

            // 시작 위치에서 목표 위치로 시간에 따라 보간(Interpolate)
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null; // 다음 프레임까지 대기
        }

        // 최종적으로 목표 위치에 정확히 맞춥니다.
        transform.position = targetPos;
        movingRoutine = null; // 코루틴 종료 상태를 표시
    }
}