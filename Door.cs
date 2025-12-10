using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public float openHeight = 3f;     // 문이 위로 열리는 거리
    public float moveDuration = 1.0f;   // 문이 열리거나 닫히는 데 걸리는 시간 (초)

    [Header("Current State")]
    public bool isDoorOpen = false;

    public string roomID;
    [SerializeField] private string conditionToOpen;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine movingRoutine;

    void OnEnable()
    {// p53에서 일어나는 일 구독 
        p53Log.OnRoomConditionMet += HandleRoomConditionMet;

    }
    void OnDisable()
    {
        p53Log.OnRoomConditionMet -= HandleRoomConditionMet;
    }

    void Start()
    {
        setThisDoorKey();
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0, openHeight * 10, 0);
    }
    public void OpenTheDoor(bool open)
    {
        if (open == isDoorOpen || movingRoutine != null) return;

        isDoorOpen = open;
        Vector3 target = open ? openPosition : closedPosition; // 목표 위치 설정

        // 기존 코루틴이 있다면 멈추고 새로운 동작 시작
        if (movingRoutine != null) StopCoroutine(movingRoutine);
        movingRoutine = StartCoroutine(MoveDoor(target));
    }

    void HandleRoomConditionMet(string p53roomID, string conditionKey)
    {
        if (p53roomID == roomID && conditionKey == conditionToOpen)
            OpenTheDoor(true);
    }

    void setThisDoorKey()
    {
        switch (roomID)
        {
            case "tut_00":
                conditionToOpen = "hasCircuit";
                break;

            case "tut_01":
                conditionToOpen = "repaired";
                break;

            case "tut_04":
                conditionToOpen = "endTutorial";
                break;
        }
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