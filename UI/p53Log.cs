using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class p53Log : MonoBehaviour
{
    public ChatLog chatLog;
    private bool isPlayerIn = false;
    [SerializeField] private float delay = 2f;

    [Header("Room Settings")]
    public string roomID;
    public Spawner linkedSpawner;
    public Door linkedDoor;

    [Header("Dialogue State")]
    public int chatLogLine = 0;
    public bool isDialogueFinished = false;

    private List<string> currentRoomLines;
    private Coroutine dialogueCoroutine;
    private Player currentPlayer;

    // 💡 방금 재생한 상태를 기억하는 변수 (중복 실행 방지용)
    private string lastConditionKey = "";

    void Start()
    {
        var currentRoom = Util.FindCurrentRoom(this.transform.position);
        if (currentRoom != null) roomID = currentRoom.roomID;
    }

    // 💡 1. 실시간 상태 감지 (Update)
    void Update()
    {
        // 플레이어가 안에 있고, 플레이어 정보가 있을 때만 실행
        if (isPlayerIn && currentPlayer != null)
        {
            // (1) 현재 상황 파악 (방문 횟수, 회로 소지 여부 등)
            int visitCount = currentPlayer.GetVisitCount(roomID);
            Debug.Log("player visted count" + roomID + "," + visitCount);
            string newConditionKey = CheckCurrentCondition(visitCount);

            // (2) 상황이 바뀌었는지 체크! (입장 직후 or 회로 획득 시)
            if (newConditionKey != lastConditionKey)
            {
                Debug.Log($"상태 변경 감지: {lastConditionKey} -> {newConditionKey}");

                // (3) 변경된 상황 처리 (대사 재생 + 문 열기 등)
                ProcessCondition(newConditionKey);
            }
        }
    }

    // 💡 2. 입장 처리 (OnTriggerEnter)
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlayerIn) return;

        isPlayerIn = true;
        isDialogueFinished = false;
        currentPlayer = other.GetComponent<Player>();

        if (currentPlayer != null)
        {
            lastConditionKey = "";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isPlayerIn == false) return;
        if (!isDialogueFinished && currentRoomLines != null && currentRoomLines.Count > 0)
        {
            chatLog.Post("안내를 끝까지 듣고 이동하시길 바랍니다.");
            Debug.Log("대사 중단됨.");
        }

        // 코루틴 정리
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        isPlayerIn = false;
        currentPlayer = null;
        lastConditionKey = "";
    }

    // 💡 4. 액션 및 대사 실행 통합 함수
    void ProcessCondition(string conditionKey)
    {
        // (1) 상태 업데이트 (중복 방지)
        lastConditionKey = conditionKey;

        // (2) 액션 실행: 문 열기 등 물리적 변화
        if (roomID == "tut_00" && conditionKey == "hasCircuit")
        {
            if (linkedDoor != null) { linkedDoor.OpenTheDoor(true); }
        }
        if (roomID == "tut_01" && conditionKey == "repaired")
        {
            if (linkedDoor != null) linkedDoor.OpenTheDoor(true);
        }


        // (3) 대사 실행 로직
        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine); // 기존 대사 끊기

        currentRoomLines = DialogueData.GetDialogueLines(roomID, conditionKey);
        chatLogLine = 0;

        if (currentRoomLines != null && currentRoomLines.Count > 0)
        {
            isDialogueFinished = false;
            dialogueCoroutine = StartCoroutine(Dialog());
        }
    }

    // 💡 5. 조건 체크 로직 (순수하게 키값만 반환)
    // 💡 상태 체크 함수 (수정본)
    string CheckCurrentCondition(int visited)
    {

        if (roomID == "tut_00" && visited == 0)
        {
            if (Player.circuit) return "hasCircuit";
        }

        if (roomID == "tut_01")
        {
            if (linkedSpawner != null && linkedSpawner.SpawnerHasCircuit)
                return "repaired";
        }
        if (visited == 0) return "startEvent";

        return "endEvent";
    }

    IEnumerator Dialog()
    {
        for (int i = chatLogLine; i < currentRoomLines.Count; i++)
        {
            chatLog.Post(currentRoomLines[i]);
            chatLogLine++;

            float t = 0f;
            while (t < delay)
            {
                if (!isPlayerIn) yield break;
                t += Time.deltaTime;
                yield return null;
            }
        }

        if (currentRoomLines.Count == chatLogLine)
        {
            isDialogueFinished = true;
            if (currentPlayer != null)
            {
                currentPlayer.AddVisitRecord(roomID);
                Debug.Log($"{roomID} 대사 완료.{currentPlayer.GetVisitCount(roomID)}");
            }
        }
    }
}