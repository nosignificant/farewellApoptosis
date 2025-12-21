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
    private Coroutine rotationRoutine;

    [Header("임시")]

    public float rotationSpeed = 50f; // 회전 속도 (양수: 시계, 음수: 반시계)
    public Vector3 axis = Vector3.up; // 회전축 (Vector3.up은 Y축 기준 뱅글뱅글)

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
        if (p53roomID == "tut_06" && roomID == "tut_06")
        {
            if (rotationRoutine != null) StopCoroutine(rotationRoutine);
            rotationRoutine = StartCoroutine(RotateTwoTimes());
        }
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
            case "tut_06":
                conditionToOpen = "startEvent";
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

    IEnumerator RotateTwoTimes()
    {
        float rotatedAmount = 0f;       // 현재까지 회전한 각도
        float targetAngle = 720f;       // 목표 각도 (360도 * 2바퀴 = 720도)

        while (rotatedAmount < targetAngle)
        {
            // 이번 프레임에 회전할 각도 계산
            float step = rotationSpeed * Time.deltaTime;

            // 만약 목표치보다 더 많이 돌 것 같으면, 딱 목표치까지만 돌도록 조정
            if (rotatedAmount + step > targetAngle)
            {
                step = targetAngle - rotatedAmount;
            }

            // 회전 적용
            transform.Rotate(axis * step);
            rotatedAmount += step;

            yield return null; // 다음 프레임 대기
        }

        rotationRoutine = null; // 코루틴 종료
    }
}